using AutoMapper;
using MentorsAndStudents.Common;
using MentorsAndStudents.Context;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MentorsAndStudents
{
    public class SolutionsService : ISolutionsService
    {
        private readonly MentorsAndStudentsContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IValidators _validators;
        private readonly IProcessFiles _processFiles;
        private readonly IGenerateRandoms _generateRandoms;
        private readonly ILogger<SolutionsService> _logger;

        public SolutionsService(MentorsAndStudentsContext context, IMapper mapper, IConfiguration configuration,
                IValidators validators, IProcessFiles processFiles, IGenerateRandoms generateRandoms, ILogger<SolutionsService> logger)
        {
            _configuration = configuration;
            _validators = validators;
            _mapper = mapper;
            _context = context;
            _processFiles = processFiles;
            _generateRandoms = generateRandoms;
            _logger = logger;
        }

        public async Task<ActionResult<SolutionsViewResponse>> ViewSolutions(SolutionsViewRequest view)
        {
            try
            {
                if (_validators.IsUserGenuine((int)view.StudentId) == false)
                {
                    _logger.LogInformation("ViewSolutions failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + view.StudentId);

                    return new SolutionsViewResponse()
                    {
                        SolutionsViews = new List<SolutionView>(),
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers, Mentors and Student can view Solutions"
                    };
                }

                if (_validators.IsUserAdmin((int)view.StudentId) == false && _validators.IsUserSchoolManager((int)view.StudentId) == false
                    && _validators.IsUserMentor((int)view.StudentId) == false && _validators.IsUserStudent((int)view.StudentId) == false)
                {
                    _logger.LogInformation("ViewSolutions failed - Only Admins, School Managers, Mentors and Students can view Solutions: " + " UserId: " + view.StudentId);

                    return new SolutionsViewResponse()
                    {
                        SolutionsViews = new List<SolutionView>(),
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers, Mentors and Student can view Solutions"
                    };
                }

                List<SolutionView> solutionsViews = new List<SolutionView>();
                List<DBSolution> dbsolutions = new List<DBSolution>();

                if (_validators.IsUserAdmin((int)view.StudentId) == false && _validators.IsUserSchoolManager((int)view.StudentId) == false
                    && _validators.IsUserMentor((int)view.StudentId) == true)
                {
                    dbsolutions = _context.DBSolutions
                        .Include(s => s.Student)
                        .Include(s => s.Content)
                        .ThenInclude(t => t.Mentor)
                        .Include(s => s.Content)
                        .ThenInclude(t => t.Course)
                        .Where(s => s.ContentId == view.ContentId).ToList();
                }

                if (_validators.IsUserStudent((int)view.StudentId) == true)
                {
                    dbsolutions = _context.DBSolutions
                        .Include(s => s.Student)
                        .Include(s => s.Content)
                        .ThenInclude(t => t.Mentor)
                        .Include(s => s.Content)
                        .ThenInclude(t => t.Course)
                        .Where(s => s.Student.Id == view.StudentId && s.ContentId == view.ContentId).ToList();                   
                }

                foreach (DBSolution solution in dbsolutions)
                {
                    SolutionView solutionView = new SolutionView()
                    { 
                            Id = solution.Id,
                            StudentFullName = solution.Student.FirstName + " " + solution.Student.LastName,
                            Name = solution.Name,
                            FullText = solution.FullText,
                            MentorFullName = solution.Content.Mentor.FirstName + " " + solution.Content.Mentor.LastName,
                            TaskName = solution.Content.Name,
                            FileName = solution.FileName,
                            SolutionUpdateAllowed = solution.Content.SolutionUpdateAllowed,
                            CreatedDate = solution.CreatedDate,
                            LastAllowedDate = solution.Content.LastAllowedDate,
                            Grade = solution.Grade,
                            IsClosed = solution.IsClosed,
                            ModifiedDate = (DateTime)solution.ModifiedDate,
                            CourseName = solution.Content.Course.Name
                    };

                    solutionsViews.Add(solutionView);
                }

                return new SolutionsViewResponse()
                {
                        SolutionsViews = solutionsViews,
                        Result = "Success",
                        ErrorMessage = "Success"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewSolutions Service error: " + ex.Message + " UserId: " + view.StudentId);

                return new SolutionsViewResponse()
                {
                    SolutionsViews = new List<SolutionView>(),
                    Result = "Failure",
                    ErrorMessage = "ViewSolutions Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<SolutionActionResponse>> UploadSolution(SolutionRequest solution)
        {
            try
            {
                if (_validators.IsUserStudent((int) solution.StudentId) == false)
                {
                    _logger.LogInformation("UploadSolution failed - Only Students can upload Solutions: " + " UserId: " + solution.StudentId);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only students can upload solutions"
                    };
                }

                if (solution.Name == "" || solution.Name == null)
                {
                    _logger.LogInformation("UploadSolution failed - Only Students can upload Solutions: " + " UserId: " + solution.StudentId);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "No Name for the solution was provided"
                    };
                }

                if (solution.File == null || solution.File.Length == 0)
                {
                    _logger.LogInformation("UploadSolution failed - No File for the solution was provide: " + " UserId: " + solution.StudentId);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "No File for the solution was provided"
                    };
                }

                DBContent dbtask = _context.DBContents.SingleOrDefault(t => t.Id == solution.ContentId);
                Content task = _mapper.Map<Content>(dbtask);

                if (task.ContentType != 0)
                {
                    _logger.LogInformation("UploadSolution failed - Selected Content is not Task and hence can't have Solutions: " + " UserId: " + solution.StudentId + " ContentTypeId: " + task.ContentType);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Selected Content is not Task and hence can't have Solutions"
                    };
                }

                DBUser dbstudent = _context.DBUsers.SingleOrDefault(u => u.Id == solution.StudentId);
                User student = _mapper.Map<User>(dbstudent);

                if (task.LastAllowedDate < DateTime.Now)
                {
                    _logger.LogInformation("UploadSolution failed - No File for the solution was provide: " + " UserId: " + solution.StudentId + " ContentId: " + task.Id + " Last allowed date: "+task.LastAllowedDate.ToString());

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Time for uploading Solution has expired"
                    };
                }

                if (IsUserAlreadySubmittedSolutionToTask(task.Id, student.Id) == true)
                {
                    _logger.LogInformation("UploadSolution failed - No File for the solution was provide: " + " UserId: " + solution.StudentId + " ContentId: " + task.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided Student already uploaded solution for provided Task"
                    };
                }

                string filePathId = _generateRandoms.GenerateRandomString();
                string fileExtension = System.IO.Path.GetExtension(solution.File.FileName);

                string fileName = solution.Name + "_" + student.FirstName + "_" + student.LastName + "_" + filePathId + fileExtension;

                DBSolution newSolution = new DBSolution()
                {
                    Name = solution.Name,
                    FileName = fileName,   
                    FullText = solution.FullText,
                    Content = dbtask,
                    Student = dbstudent,
                    CreatedDate= DateTime.UtcNow,
                    Grade = "Not set",
                    IsClosed = false,
                    ModifiedDate= DateTime.UtcNow
                };

                bool fileUpload = await _processFiles.UploadFile(fileName, solution.File, "Solutions");

                if (fileUpload == true)
                {
                    _context.DBSolutions.Add(newSolution);
                    _context.SaveChanges();

                    _logger.LogInformation("Solution created successfully" + " UserId: " + solution.StudentId + " SolutionId: " + newSolution.Id + "File Name: " + fileName);

                    return new SolutionActionResponse()
                    {
                        Id = newSolution.Id,
                        FileName = fileName,
                        Result = "Success",
                        ErrorMessage = "Solution created successfully"
                    };
                }

                else
                {
                    _logger.LogInformation("UploadSolution failed - Failed to upload file: " + " UserId: " + solution.StudentId);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Failed to upload file"
                    };
                }
            }

            catch (Exception ex)
            {
                _logger.LogError("UploadSolution Service error: " + ex.Message + " UserId: " + solution.StudentId);

                return new SolutionActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UploadSolution Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<SolutionActionResponse>> UpdateSolution(SolutionRequest solution)
        {
            try
            {
                bool mustReUpload = false;

                if (_validators.IsUserStudent((int)solution.StudentId) == false)
                {
                    _logger.LogInformation("UpdateSolution failed - Only Students can update Solutions: " + " UserId: " + solution.StudentId + " SolutionId: " + solution.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only students can update solutions"
                    };
                }

                if (IsSolutionWithGivenIdExists((int) solution.Id) == false)
                {
                    _logger.LogInformation("UpdateSolution failed - Solution with given id not found: " + " UserId: " + solution.StudentId + " SolutionId: " + solution.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Solution with given id not found"
                    };
                }

                if (solution.Name == "" || solution.Name == null)
                {
                    _logger.LogInformation("UpdateSolution failed - No Name for the solution was provided: " + " UserId: " + solution.StudentId + " SolutionId: " + solution.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "No Name for the solution was provided"
                    };
                }


                if (!(solution.File == null || solution.File.Length == 0))
                {
                    mustReUpload = true;
                }

                DBSolution updatedSolution = _context                    
                    .DBSolutions
                    .Include(s => s.Content)
                    .ThenInclude(t => t.Mentor)
                    .Include(s => s.Student)
                    .SingleOrDefault(t => t.Id == solution.Id);


                if (updatedSolution.Content.ContentType != 0)
                {
                    _logger.LogInformation("UpdateSolution failed - Selected Content is not Task and hence can't have Solutions: " + " UserId: " + solution.StudentId + " ContentTypeId: " + updatedSolution.Content.ContentType);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Selected Content is not Task and hence can't have Solutions"
                    };
                }

                if (updatedSolution.UserId != solution.StudentId)
                {
                    _logger.LogInformation("UpdateSolution failed - Provided Student Id is different from stored Student Id: " + " UserId: " + solution.StudentId + " SolutionId: " + solution.Id  +" Stored StudentId: " + updatedSolution.Student.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided Student Id is different from stored Student Id"
                    };
                }

                if (updatedSolution.IsClosed == true)
                {
                    _logger.LogInformation("UpdateSolution failed - Solution is already closed by mentor: " + " StudentId: " + solution.StudentId + " SolutionId: " + solution.Id + " MentorId " + updatedSolution.Content.Mentor.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Solution is already closed by mentor"
                    };
                }

                if (updatedSolution.Content.LastAllowedDate < DateTime.Now)
                {
                    _logger.LogInformation("UpdateSolution failed - Time for updating Solution has expired: " + " StudentId: " + solution.StudentId + " SolutionId: " + solution.Id + " Last allowed date " + updatedSolution.Content.LastAllowedDate);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Time for updating Solution has expired"
                    };
                }

                if (updatedSolution.Content.SolutionUpdateAllowed == false)
                {
                    _logger.LogInformation("UpdateSolution failed - Mentor has not allowed re-uploading submissions: " + " StudentId: " + solution.StudentId + " SolutionId: " + solution.Id + " MentorId " + updatedSolution.Content.Mentor.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentor has not allowed re-uploading submissions"
                    };
                }

                updatedSolution.Name = solution.Name;
                updatedSolution.CreatedDate= DateTime.UtcNow;
                updatedSolution.ModifiedDate= DateTime.UtcNow;
                updatedSolution.FullText = solution.FullText;

                if (mustReUpload)
                {
                    bool fileUpload = await _processFiles.UploadFile(updatedSolution.FileName, solution.File, "Solutions");

                    if (fileUpload == true)
                    {
                        _context.SaveChanges();

                        _logger.LogInformation("Solution updated successfully" + " UserId: " + solution.StudentId + " SolutionId: " + updatedSolution.Id + "File Name: " + solution.File);

                        return new SolutionActionResponse()
                        {
                            Id = updatedSolution.Id,
                            FileName = updatedSolution.FileName,
                            Result = "Success",
                            ErrorMessage = "Solution updated successfully"
                        };
                    }

                    else
                    {
                        _logger.LogInformation("UpdateSolution failed - Failed to upload file: " + " UserId: " + solution.StudentId);

                        return new SolutionActionResponse()
                        {
                            Result = "Failure",
                            ErrorMessage = "Failed to upload file"
                        };
                    }
                }

                else
                {
                    _context.SaveChanges();

                    _logger.LogInformation("Solution updated successfully" + " UserId: " + solution.StudentId + " SolutionId: " + updatedSolution.Id);

                    return new SolutionActionResponse()
                    {
                        Id = updatedSolution.Id,
                        FileName = updatedSolution.FileName,
                        Result = "Success",
                        ErrorMessage = "Solution updated successfully"
                    };
                }
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateSolution Service error: " + ex.Message + " UserId: " + solution.StudentId);

                return new SolutionActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateSolution Service error: " + ex.Message
                };
            }
        }

        public async Task<FileStreamResult> DownloadSolution(SolutionRequest solution)
        {
            try
            {
                if (_validators.IsUserAdmin((int)solution.StudentId) == false && _validators.IsUserSchoolManager((int)solution.StudentId) == false
                    && _validators.IsUserMentor((int)solution.StudentId) == false && _validators.IsUserStudent((int)solution.StudentId) == false)
                {
                    _logger.LogInformation("DownloadSolution failed - Only Admins, School Managers, Mentors and Students can download Solutions: " + " UserId: " + solution.StudentId);

                    throw new Exception("Only Admins, School Managers, Mentors and Students can download Solutions");
                }

                DBSolution dbsolution = _context.DBSolutions.SingleOrDefault(t => t.Id == solution.Id);             

                return await _processFiles.DownloadFile(dbsolution.FileName, "Solutions");
            }

            catch (Exception ex)
            {
                _logger.LogError("DownloadSolution Service error: " + ex.Message + " UserId: " + solution.StudentId);

                throw new Exception("DownloadSolution Service exception");
            }
        }

        public async Task<ActionResult<SolutionActionResponse>> DeleteSolution(SolutionRequest solution)
        {
            try
            {
                if (_validators.IsUserStudent((int)solution.StudentId) == false)
                {
                    _logger.LogInformation("DeleteSolution failed - Only Students can delete Solutions: " + " UserId: " + solution.StudentId + " SolutionId: " + solution.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only students can delete solutions"
                    };
                }

                if (IsSolutionWithGivenIdExists((int) solution.Id) == false)
                {
                    _logger.LogInformation("DeleteSolution failed - Solution with given id not found: " + " UserId: " + solution.StudentId + " SolutionId: " + solution.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Solution with given id not found"
                    };
                }

                DBSolution dbsolution = _context.DBSolutions
                    .Include(s => s.Student)
                    .Include(s => s.Content)
                    .ThenInclude(t => t.Mentor)
                    .SingleOrDefault(t => t.Id == solution.Id);

                if (dbsolution.Student.Id != solution.StudentId)
                {
                    _logger.LogInformation("DeleteSolution failed - Provided Student Id is different from stored Student Id: " + " UserId: " + solution.StudentId + " SolutionId: " + solution.Id + " Stored StudentId: " + dbsolution.Student.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided Student Id is different from stored Student Id"
                    };
                }

                if (dbsolution.IsClosed == true)
                {
                    _logger.LogInformation("DeleteSolution failed - Solution is already closed by mentor: " + " StudentId: " + solution.StudentId + " SolutionId: " + solution.Id + " MentorId " + dbsolution.Content.Mentor.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Solution is already closed by mentor"
                    };
                }

                if (dbsolution.Content.SolutionUpdateAllowed == false)
                {
                    _logger.LogInformation("DeleteSolution failed - Mentor has not allowed deleting submissions: " + " StudentId: " + solution.StudentId + " SolutionId: " + solution.Id + " MentorId " + dbsolution.Content.Mentor.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentor has not allowed deleting submissions"
                    };
                }

                bool fileDelete = await _processFiles.DeleteFile(dbsolution.FileName, "Solutions");

                if (fileDelete == true)
                {
                    _context.DBSolutions.Remove(dbsolution);
                    _context.SaveChanges();

                    _logger.LogInformation("Solution deleted successfully" + " UserId: " + solution.StudentId + " SolutionId: " + dbsolution.Id);

                    return new SolutionActionResponse()
                    {
                        Result = "Success",
                        ErrorMessage = "Solution deleted successfully"
                    };
                }

                else
                {
                    _logger.LogInformation("DeleteSolution failed - Failed to delete file: " + " UserId: " + solution.StudentId);

                    return new SolutionActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Failed to delete file"
                    };
                }
            }

            catch (Exception ex)
            {
                _logger.LogError("DeleteSolution Service error: " + ex.Message + " UserId: " + solution.StudentId);

                return new SolutionActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "DeleteSolution Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<SolutionSetGradeResponse>> SetGradeSolution(SolutionSetGradeRequest solutionGradeModel)
        {
            try
            {
                if (_validators.IsUserMentor((int)solutionGradeModel.MentorId) == false)
                {
                    _logger.LogInformation("DeleteSolution failed - Solution with given id not found: " + " UserId: " + solutionGradeModel.MentorId);

                    return new SolutionSetGradeResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "DeleteSolution failed - Only mentors can set grades to solutions"
                    };
                }

                if (IsSolutionWithGivenIdExists(solutionGradeModel.SolutionId) == false)
                {
                    _logger.LogInformation("DeleteSolution failed - Solution with given id not found: " + " UserId: " + solutionGradeModel.MentorId + " SolutionId: " + solutionGradeModel.SolutionId);

                    return new SolutionSetGradeResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Solution with given id not found"
                    };
                }

                DBSolution dbsolution = _context
                    .DBSolutions
                    .Include(s => s.Content)
                    .ThenInclude(c => c.Course)
                    .ThenInclude(co => co.School)
                    .SingleOrDefault(t => t.Id == solutionGradeModel.SolutionId);
                dbsolution.Grade = solutionGradeModel.Grade;
                dbsolution.IsClosed = true;


                if (!_validators.IsValidGrade(solutionGradeModel.Grade, dbsolution.Content.Course.School.GradingSystem, out var errorMessage))
                {
                    _logger.LogInformation($"SetGradeSolution failed - {errorMessage}. UserId: {solutionGradeModel.MentorId}, Grade: {solutionGradeModel.Grade}, GradingSystem: {dbsolution.Content.Course.School.GradingSystem}");

                    return new SolutionSetGradeResponse
                    {
                        Result = "Failure",
                        ErrorMessage = errorMessage
                    };
                }

                _context.SaveChanges();

                _logger.LogInformation("Grade set successfully" + " UserId: " + solutionGradeModel.MentorId + " SolutionId: " + solutionGradeModel.SolutionId + " Grade: " + solutionGradeModel.Grade);

                return new SolutionSetGradeResponse()
                {
                    SolutionId = solutionGradeModel.SolutionId,
                    Grade = solutionGradeModel.Grade,
                    Result = "Success",
                    ErrorMessage = "Grade set successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("SetGradeSolution Service error: " + ex.Message + " UserId: " + solutionGradeModel.MentorId);

                return new SolutionSetGradeResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "SetGradeSolution Service error: " + ex.Message
                };
            }
        }

        private bool IsUserAlreadySubmittedSolutionToTask (int ContentId, int studentId)
        {
            List<DBSolution> solutions = _context.DBSolutions.Where(s => s.ContentId == ContentId && s.UserId == studentId).ToList();

            return solutions.Count > 0;
        }

        private bool IsSolutionWithGivenIdExists(int solutionId)
        {
            List<DBSolution> solutions = _context.DBSolutions.Where(s => s.Id == solutionId).ToList();

            return solutions.Count == 1;
        }
    }
}
