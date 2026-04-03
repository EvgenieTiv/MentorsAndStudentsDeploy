using AutoMapper;
using Azure.Core;
using MentorsAndStudents.Common;
using MentorsAndStudents.Context;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IO;
using System.Threading.Tasks;

namespace MentorsAndStudents
{
    public class ContentService : ITasksService
    {
        private readonly MentorsAndStudentsContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IValidators _validators;
        private readonly IProcessFiles _processFiles;
        private readonly IGenerateRandoms _generateRandoms;
        private readonly ILogger<ContentService> _logger;

        public ContentService(MentorsAndStudentsContext context, IMapper mapper, IConfiguration configuration,
                IValidators validators, IProcessFiles processFiles, IGenerateRandoms generateRandoms, ILogger<ContentService> logger)
        {
            _configuration = configuration;
            _validators = validators;
            _mapper = mapper;
            _context = context;
            _processFiles = processFiles;
            _generateRandoms = generateRandoms;
            _logger = logger;
        }

        public async Task<ActionResult<ContentViewResponse>> ViewContents(ContentsViewRequest view)
        {
            try
            {
                if (_validators.IsUserGenuine((int)view.MentorId) == false)
                {
                    _logger.LogInformation("ViewContents failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + view.MentorId);

                    return new ContentViewResponse()
                    {
                        TasksViews = new List<TaskView>(),
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin((int)view.MentorId) == false && _validators.IsUserSchoolManager((int)view.MentorId) == false
                    && _validators.IsUserMentor((int)view.MentorId) == false && _validators.IsUserStudent((int)view.MentorId) == false)
                {
                    _logger.LogInformation("ViewContents failed - Only Admins, School Managers, Mentors and Student can view Tasks: " + " UserId: " + view.MentorId);

                    return new ContentViewResponse()
                    {
                        TasksViews = new List<TaskView>(),
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers, Mentors and Student can view Tasks"
                    };
                }

                List<Content> tasks = new List<Content>();

                if (_validators.IsUserMentor((int)view.MentorId) == true)
                {
                    List<DBContent> dbtasks = _context.DBContents
                        .Include(t => t.Mentor)
                        .Include(t => t.Course)
                        .ThenInclude(c => c.Topic)
                        .Where(t => t.Mentor.Id == view.MentorId && t.Course.Id == view.CourseId).ToList();
                    tasks = _mapper.Map<List<Content>>(dbtasks);
                }

                if (_validators.IsUserAdmin((int)view.MentorId) == true || _validators.IsUserSchoolManager((int)view.MentorId) == true
                    || _validators.IsUserStudent((int)view.MentorId) == true)
                {
                    List<DBContent> dbtasks = _context.DBContents
                        .Include(t => t.Mentor)
                        .Include(t => t.Course)
                        .ThenInclude(c => c.Topic)
                        .Where(t => t.Course.Id == view.CourseId).ToList();
                    tasks = _mapper.Map<List<Content>>(dbtasks);
                }

                if (view.IsViewExpired == false)
                {
                    tasks = tasks
                        .Where(t => t.ContentType != 0 || t.LastAllowedDate >= DateTime.Now)
                        .ToList();
                }

                tasks = tasks.OrderBy(t => t.CreatedDate).ToList();

                List<TaskView> tasksViews = new List<TaskView>();

                foreach (Content task in tasks)
                {
                        TaskView taskView = new TaskView()
                        {
                            Id = task.Id,
                            MentorFullName = task.Mentor.FirstName + " " + task.Mentor.LastName,
                            TopicName = task.Course.Topic.Name,
                            Name = task.Name,
                            Summary = task.Summary,
                            FullText = task.FullText,
                            FileName = task.FileName,
                            SolutionUpdateAllowed = task.SolutionUpdateAllowed,
                            UpdateCreatedDate = task.UpdateCreatedDate,
                            CreatedDate = task.CreatedDate,
                            LastAllowedDate = task.LastAllowedDate,
                            ModifiedDate = task.ModifiedDate,
                            ContentType = task.ContentType
                        };

                        tasksViews.Add(taskView);
                }

                return new ContentViewResponse()
                {
                        TasksViews = tasksViews,
                        Result = "Success",
                        ErrorMessage = "Success"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewContents Service error: " + ex.Message + " UserId: " + view.MentorId);

                return new ContentViewResponse()
                {
                    TasksViews = new List<TaskView>(),
                    Result = "Failure",
                    ErrorMessage = "ViewContents Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<ContentActionResponse>> UploadContent(ContentRequest task)
        {
            try
            {
                if (_validators.IsUserGenuine((int)task.MentorId) == false)
                {
                    _logger.LogInformation("UploadContent failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserMentor((int)task.MentorId) == false)
                {
                    _logger.LogInformation("UploadContent failed - Only Mentors can alter Tasks: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Mentors can alter Tasks"
                    };
                }

                if (task.Name == "" || task.Name == null)
                {
                    _logger.LogInformation("UploadContent failed - No Name for the task was provided: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "No Name for the task was provided"
                    };
                }

                if (task.File == null || task.File.Length == 0)
                {
                    _logger.LogInformation("UploadContent failed - No File for the task was provided: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "No File for the task was provided"
                    };
                }

                if (task.ContentType == 0 && task.LastAllowedDate == null)
                {
                    _logger.LogInformation("UploadContent failed - Last Allowed Date was not provided: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Last Allowed Date was not provided"
                    };
                }

                if (!Enum.IsDefined(typeof(ContentType), task.ContentType))
                {
                    _logger.LogInformation("UploadContent failed - Invalid content type: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Invalid content type"
                    };
                }

                DBUser user = _context.DBUsers.SingleOrDefault(u => u.Id == task.MentorId);
                User mentor = _mapper.Map<User>(user);

                DBCourse dbcourse = _context.DBCourses.SingleOrDefault(u => u.Id == task.CourseId);
                Course course = _mapper.Map<Course>(dbcourse);

                string filePathId = _generateRandoms.GenerateRandomString();
                string fileExtension = System.IO.Path.GetExtension(task.File.FileName);

                string fileName = course.Name + "_" + mentor.FirstName + "_" + mentor.LastName + "_" + task.Name + "_" + filePathId + fileExtension;

                DBContent newTask = new DBContent()
                {
                    FileName = fileName,
                    Name = task.Name,                    
                    UserId = mentor.Id,
                    CourseId = (int) task.CourseId,
                    SolutionUpdateAllowed = (bool)task.SolutionUpdateAllowed,
                    UpdateCreatedDate = (bool)task.UpdateCreatedDate,
                    CreatedDate= DateTime.UtcNow,
                    LastAllowedDate = task.ContentType == 0 ? (DateTime) task.LastAllowedDate : DateTime.Now,
                    ModifiedDate= DateTime.UtcNow,
                    ContentType = (ContentType)task.ContentType,
                    Summary = task.Summary,
                    FullText = task.FullText != null ? task.FullText : String.Empty
                };

                bool fileUpload = await _processFiles.UploadFile(fileName, task.File, "Tasks");

                if (fileUpload == true)
                {
                    _context.DBContents.Add(newTask);
                    _context.SaveChanges();

                    _logger.LogInformation("Content created successfully" + " UserId: " + task.MentorId + " ContentId: " + newTask.Id + "File Name: " + fileName);

                    return new ContentActionResponse()
                    {
                        Id = newTask.Id,
                        FileName = newTask.FileName,
                        Result = "Success",
                        ErrorMessage = "Content created successfully"
                    };
                }

                else
                {
                    _logger.LogInformation("UploadContent failed - Failed to upload file: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Failed to upload file"
                    };
                }
            }

            catch (Exception ex)
            {
                _logger.LogError("UploadContent Service error: " + ex.Message + " UserId: " + task.MentorId);

                return new ContentActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UploadContent Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<ContentActionResponse>> UpdateContent(ContentRequest task)
        {
            try
            {
                bool mustReUpload = false;

                if (_validators.IsUserGenuine((int)task.MentorId) == false)
                {
                    _logger.LogInformation("UpdateContent failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserMentor((int)task.MentorId) == false)
                {
                    _logger.LogInformation("UpdateContent failed - Only Mentors can alter Tasks: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Mentors can alter Tasks"
                    };
                }

                if (IsContentWithGivenIdExists((int) task.Id) == false)
                {
                    _logger.LogInformation("UpdateContent failed - Task with given id not found: " + " UserId: " + task.MentorId + " ContentId: " + task.Id);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Task with given id not found"
                    };
                }

                if (task.Name == "" || task.Name == null)
                {
                    _logger.LogInformation("UpdateContent failed - No Name for the task was provided: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "No Name for the task was provided"
                    };
                }


                if (!(task.File == null || task.File.Length == 0))
                {
                    mustReUpload = true;
                }

                if (task.ContentType == 0 && task.UpdateCreatedDate == true && task.LastAllowedDate == null)
                {
                    _logger.LogInformation("UpdateContent failed - Last Allowed Date was not provided: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Last Allowed Date was not provided"
                    };
                }

                if (!Enum.IsDefined(typeof(ContentType), task.ContentType))
                {
                    _logger.LogInformation("UploadContent failed - Invalid content type: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Invalid content type"
                    };
                }

                DBContent dbtask = _context.DBContents.SingleOrDefault(t => t.Id == task.Id);

                if (dbtask.UserId != task.MentorId)
                {
                    _logger.LogInformation("UpdateContent failed - Provided Mentor Id is different from stored Mentor Id: " + " UserId: " + task.MentorId + " ContentId: " + task.Id + " Stored MentorId: " + dbtask.Mentor.Id);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided Mentor Id is different from stored mentor Id"
                    };
                }

                dbtask.Name = task.Name != null ? task.Name : dbtask.Name;
                dbtask.SolutionUpdateAllowed = (bool)(task.SolutionUpdateAllowed != null ? task.SolutionUpdateAllowed : dbtask.SolutionUpdateAllowed);
                dbtask.CreatedDate = task.UpdateCreatedDate == true ? DateTime.Now : dbtask.CreatedDate;
                dbtask.LastAllowedDate = task.ContentType == 0 ? (task.LastAllowedDate != null ? (DateTime)task.LastAllowedDate : dbtask.LastAllowedDate) : DateTime.Now;
                dbtask.ModifiedDate= DateTime.UtcNow;
                dbtask.ContentType = (ContentType)task.ContentType;
                dbtask.Summary = task.Summary;
                dbtask.FullText = task.FullText != null ? task.FullText : string.Empty;

                if (mustReUpload)
                {
                    bool fileUpload = await _processFiles.UploadFile(dbtask.FileName, task.File, "Tasks");

                    if (fileUpload == true)
                    {
                        _context.SaveChanges();

                        _logger.LogInformation("Task updated successfully" + " UserId: " + task.MentorId + " ContentId: " + dbtask.Id + "File Name: " + task.File);

                        return new ContentActionResponse()
                        {
                            Id = dbtask.Id,
                            FileName = dbtask.FileName,
                            Result = "Success",
                            ErrorMessage = "Task updated successfully"
                        };
                    }

                    else
                    {
                        _logger.LogInformation("UpdateContent failed - Failed to upload file: " + " UserId: " + task.MentorId);

                        return new ContentActionResponse()
                        {
                            Result = "Failure",
                            ErrorMessage = "Failed to upload file"
                        };
                    }
                }

                else
                {
                    _logger.LogInformation("Task updated successfully" + " UserId: " + task.MentorId + " ContentId: " + dbtask.Id);

                    _context.SaveChanges();

                    return new ContentActionResponse()
                    {
                        Id = dbtask.Id,
                        FileName = dbtask.FileName,
                        Result = "Success",
                        ErrorMessage = "Task updated successfully"
                    };
                }
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateContent Service error: " + ex.Message + " UserId: " + task.MentorId);

                return new ContentActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateContent Service error: " + ex.Message
                };
            }
        }

        public async Task<FileStreamResult> DownloadContent(ContentRequest task)
        {
            try
            {
                if (_validators.IsUserGenuine((int)task.MentorId) == false)
                {
                    _logger.LogInformation("UpdateContent failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + task.MentorId);

                    throw new Exception("Logged in UserId is not same as provided UserId (possible hack attempt!)");
                }

                if (_validators.IsUserAdmin((int)task.MentorId) == false && _validators.IsUserSchoolManager((int)task.MentorId) == false
                    && _validators.IsUserMentor((int)task.MentorId) == false && _validators.IsUserStudent((int)task.MentorId) == false)
                {
                    _logger.LogInformation("DownloadContent failed - Only Admins, School Managers, Mentors and Students can download Tasks: " + " UserId: " + task.MentorId);

                    throw new Exception("Only Admins, School Managers, Mentors and Students can download Tasks");
                }

                DBContent dbtask = _context.DBContents.SingleOrDefault(t => t.Id == task.Id);

                return await _processFiles.DownloadFile(dbtask.FileName, "Tasks");
            }

            catch (Exception ex)
            {
                _logger.LogError("DownloadContent Service error: " + ex.Message + " UserId: " + task.MentorId);

                throw new Exception("DownloadContent Service exception");
            }
        }

        public async Task<ActionResult<ContentActionResponse>> DeleteContent(DeleteContentRequest task)
        {
            try
            {
                if (_validators.IsUserGenuine((int)task.MentorId) == false)
                {
                    _logger.LogInformation("DeleteContent failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin((int)task.MentorId) == false && _validators.IsUserSchoolManager((int)task.MentorId) == false
                    && _validators.IsUserMentor((int)task.MentorId) == false)
                {
                    _logger.LogInformation("DeleteContent failed - Only Mentors can delete Tasks: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers and Mentors can alter Tasks"
                    };
                }

                if (IsContentWithGivenIdExists((int) task.Id) == false)
                {
                    _logger.LogInformation("DeleteContent failed - Task with given id not found: " + " UserId: " + task.MentorId + " ContentId: " + task.Id);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Task with given id not found"
                    };
                }

                DBContent dbtask = _context.DBContents.SingleOrDefault(t => t.Id == task.Id);

                if (IsTaskHasNoSolutions(dbtask.Id) == false)
                {
                    _logger.LogInformation("DeleteContent failed, Task can't be deleted, it has solutions: " + task.MentorId + " ContentId: " + dbtask.Id);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Task can't be deleted, it has solutions"
                    };
                }

                if (dbtask.UserId != task.MentorId)
                {
                    _logger.LogInformation("DeleteContent failed - Provided Mentor Id is different from stored Mentor Id: " + " UserId: " + task.MentorId + " ContentId: " + task.Id + " Stored MentorId: " + dbtask.Mentor.Id);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided Mentor Id is different from stored mentor Id"
                    };
                }

                bool fileDelete = await _processFiles.DeleteFile(dbtask.FileName, "Tasks");

                if (fileDelete == true)
                {
                    _context.DBContents.Remove(dbtask);
                    _context.SaveChanges();

                    _logger.LogInformation("Task deleted successfully" + " UserId: " + task.Id + " ContentId: " + dbtask.Id);

                    return new ContentActionResponse()
                    {
                        Result = "Success",
                        ErrorMessage = "Task deleted successfully"
                    };
                }

                else
                {
                    _logger.LogInformation("DeleteContent failed - Failed to delete file: " + " UserId: " + task.MentorId);

                    return new ContentActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Failed to delete file"
                    };
                }
            }

            catch (Exception ex)
            {
                _logger.LogError("DeleteContent Service error: " + ex.Message + " UserId: " + task.MentorId);

                return new ContentActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "DeleteContent Service error: " + ex.Message
                };
            }
        }

        private bool IsContentWithGivenIdExists(int ContentId)
        {
            List<DBContent> tasks = _context.DBContents.Where(s => s.Id == ContentId).ToList();

            return tasks.Count == 1;
        }

        private bool IsTaskHasNoSolutions(int ContentId)
        {
            List<DBSolution> solutions = _context.DBSolutions.Where(c => c.ContentId == ContentId).ToList();

            return solutions.Count == 0;
        }
    }
}
