using AutoMapper;
using Azure.Core;
using MentorsAndStudents.Common;
using MentorsAndStudents.Context;
using MentorsAndStudents.Requests;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;

namespace MentorsAndStudents
{
    public class CourseService: ICourseService
    {
        private readonly MentorsAndStudentsContext _context;
        private readonly IValidators _validators;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;

        public CourseService(MentorsAndStudentsContext context, IMapper mapper, IValidators validators, ILogger<CourseService> logger)
        {
            _context = context;
            _validators = validators;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ActionResult<CoursesViewResponse>> ViewCourses(CoursesViewRequest view)
        {
            try
            {
                List<Course> courses = new List<Course>();

                if (_validators.IsUserGenuine((int)view.UserId) == false)
                {
                    _logger.LogInformation("ViewCourses failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + view.UserId);

                    return new CoursesViewResponse()
                    {
                        CoursesViews = new List<Course>(),
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(view.UserId) == true)
                {
                    List<DBCourse> dbcourses = _context.DBCourses
                                                       .Include(c => c.School)
                                                       .Include(c => c.Topic).ToList();

                    courses = _mapper.Map<List<Course>>(dbcourses);
                }

                if (_validators.IsUserSchoolManager(view.UserId) == true || _validators.IsUserMentor(view.UserId) == true 
                    || _validators.IsUserStudent(view.UserId) == true)
                {
                    List<DBCourseUser> courseUsers = _context.DBCourseUsers.Where(s => s.UserId == view.UserId).ToList();
                    List<int> courseIds = courseUsers.ConvertAll(x => x.CourseId);

                    List<DBCourse> dbcourses = _context
                        .DBCourses
                        .Include(c => c.School)
                        .Include(c => c.Topic)
                        .Where(s => courseIds.Contains(s.Id)).ToList();
                    
                    courses = _mapper.Map<List<Course>>(dbcourses);

                    courses = courses.OrderBy(c => c.Name).ToList();

                    foreach(Course course in courses)
                    {
                        course.AssociatedMentors = GetCourseMentors(course.Id);
                    }
                }

                return new CoursesViewResponse()
                {
                    CoursesViews = courses,
                    Result = "Success",
                    ErrorMessage = "Courses shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewCourses Service error: " + ex.Message + " UserId: " + view.UserId);

                return new CoursesViewResponse()
                {
                    CoursesViews = new List<Course>(),
                    Result = "Failure",
                    ErrorMessage = "ViewCourses Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<CoursePropsResponse>> ViewSingleCourseProps(CoursesViewRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine((int)request.UserId) == false)
                {
                    _logger.LogInformation("ViewSingleCourseProps failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.UserId);

                    return new CoursePropsResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsCourseWithGivenIdExists((int)request.CourseId) == false)
                {
                    _logger.LogInformation("ViewSingleCourseProps  failed, Course with given id not found - AdminUserId: " + request.UserId + " CourseId: " + request.CourseId);

                    return new CoursePropsResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Course with given id not found"
                    };
                }

                DBCourse dbcourse = _context.DBCourses
                                        .Include(c => c.Topic)
                                        .SingleOrDefault(c => c.Id == request.CourseId);

                var content = _context.DBContents
                    .Where(c => c.CourseId == request.CourseId)
                    .OrderByDescending(c => c.CreatedDate)
                    .FirstOrDefault();

                return new CoursePropsResponse()
                {
                    Id = dbcourse.Id,
                    Name = dbcourse.Name,
                    TopicName = dbcourse.Topic.Name,
                    SchoolClass = dbcourse.SchoolClass,
                    StudentsCount = GetCourseStudentsCount((int)request.CourseId),
                    LastUpdated = content != null ? content.CreatedDate.Value : dbcourse.CreatedDate.Value,
                    Mentors = GetCourseMentors((int)request.CourseId),
                    Result = "Success",
                    ErrorMessage = "Course props shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewSingleCourseProps Service error: " + ex.Message + " UserId: " + request.UserId);

                return new CoursePropsResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "ViewSingleCourseProps Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<UsersViewResponse>> ViewUsersAssignedOrNotToCourse(UsersViewRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine((int)request.AdminUserId) == false)
                {
                    _logger.LogInformation("ViewUsersAssignedOrNotToCourse  failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.AdminUserId);

                    return new UsersViewResponse()
                    {
                        UsersViews = new List<User>(),
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsCourseWithGivenIdExists((int)request.CourseId) == false)
                {
                    _logger.LogInformation("ViewUsersAssignedOrNotToCourse  failed, Course with given id not found - AdminUserId: " + request.AdminUserId + " CourseId: " + request.CourseId);

                    return new UsersViewResponse()
                    {
                        UsersViews = new List<User>(),
                        Result = "Failure",
                        ErrorMessage = "Course with given id not found"
                    };
                }

                if (IsUserAssignedToCourse((int)request.CourseId, (int)request.AdminUserId) == false)
                {
                    _logger.LogInformation("ViewUsersAssignedOrNotToCourse  failed, Provided user is not assigned to desired course - AdminUserId: " + request.AdminUserId + " CourseId: " + request.CourseId);

                    return new UsersViewResponse()
                    {
                        UsersViews = new List<User>(),
                        Result = "Failure",
                        ErrorMessage = "Provided user is not assigned to desired course"
                    };
                }

                if (_validators.IsUserAdmin((int)request.AdminUserId) == false && _validators.IsUserSchoolManager((int)request.AdminUserId) == false
                        && _validators.IsUserMentor((int)request.AdminUserId) == false)
                {
                    _logger.LogInformation("ViewUsersAssignedOrNotToCourse  failed, Only Admins, School Managers and Mentors can view users assigned to course - AdminUserId: " + request.AdminUserId + " CourseId: " + request.CourseId);

                    return new UsersViewResponse()
                    {
                        UsersViews = new List<User>(),
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers and Mentors can view users assigned to course"
                    };
                }

                List<DBCourseUser> courseUsers = _context.DBCourseUsers.Where(s => s.CourseId == request.CourseId).ToList();
                List<int> courseUserIds = courseUsers.Select(x => x.UserId).ToList();

                List<DBSchoolUser> schoolUsers = _context.DBSchoolUsers.Where(s => s.SchoolId == request.SchoolId).ToList();
                List<int> schoolUserIds = schoolUsers.Select(x => x.UserId).ToList();

                List<int> userIds = courseUserIds.Intersect(schoolUserIds).ToList();

                List<DBUser> dbusers = request.ViewAssigned
                    ? _context.DBUsers.Where(u => userIds.Contains(u.Id)).ToList()
                    : _context.DBUsers.Where(u => !userIds.Contains(u.Id) && schoolUserIds.Contains(u.Id)).ToList();

                dbusers.RemoveAll(u => u.UserTypeId == 2);

                List<User> users = _mapper.Map<List<User>>(dbusers);

                users.ForEach(u => u.Password = null);

                return new UsersViewResponse()
                {
                    UsersViews = users,
                    CourseId = request.CourseId,
                    IsShowingAssigned = request.ViewAssigned,
                    Result = "Success",
                    ErrorMessage = request.ViewAssigned ? "Users of relevant course shown" : "Users not assigned to relevant course shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewUsersAssignedOrNotToCourse  Service error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new UsersViewResponse()
                {
                    UsersViews = new List<User>(),
                    Result = "Failure",
                    ErrorMessage = "ViewUsersAssignedOrNotToCourse  Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<SchoolOrCourseUsersByTypeAndCoursesCountResponse>> GetAllCourseUsersByTypeCount(UsersViewRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine((int)request.AdminUserId) == false)
                {
                    _logger.LogInformation("GetAllCourseUsersByTypeCount  failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.AdminUserId);

                    return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsCourseWithGivenIdExists((int)request.CourseId) == false)
                {
                    _logger.LogInformation("GetAllCourseUsersByTypeCount  failed, Course with given id not found - AdminUserId: " + request.AdminUserId + " CourseId: " + request.CourseId);

                    return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Course with given id not found"
                    };
                }

                List<DBCourseUser> courseUsers = _context.DBCourseUsers.Where(s => s.CourseId == request.CourseId).ToList();

                List<int> userIds = courseUsers.ConvertAll(x => x.UserId);

                List<DBUser> dbusers = _context.DBUsers.Where(u => userIds.Contains(u.Id)).ToList();

                int schoolManagersCount = dbusers.Where(u => u.UserTypeId == 3).ToList().Count;
                int mentorsCount = dbusers.Where(u => u.UserTypeId == 0).ToList().Count;
                int studentsCount = dbusers.Where(u => u.UserTypeId == 1).ToList().Count;

                return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                {
                    SchoolManagersCount = schoolManagersCount,
                    MentorsCount = mentorsCount,
                    StudentsCount = studentsCount,
                    CoursesCount = 0,
                    Result = "Success",
                    ErrorMessage = "Information about count of course users shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("GetAllCourseUsersByTypeCount  Service error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "GetAllCourseUsersByTypeCount  Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<GetLastContentUpdateDateResponse>> GetLastContentUpdateDate(UsersViewRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine(request.AdminUserId) == false)
                {
                    _logger.LogInformation("GetLastContentUpdateDate failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" +
                                           " Provided UserId: " + request.AdminUserId);

                    return new GetLastContentUpdateDateResponse
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (!IsCourseWithGivenIdExists(request.CourseId.Value))
                {
                    _logger.LogInformation("GetLastContentUpdateDate failed, Course with given id not found - AdminUserId: " +
                                           request.AdminUserId + " CourseId: " + request.CourseId);

                    return new GetLastContentUpdateDateResponse
                    {
                        Result = "Failure",
                        ErrorMessage = "Course with given id not found"
                    };
                }

                var content = _context.DBContents
                    .Where(c => c.CourseId == request.CourseId)
                    .OrderByDescending(c => c.CreatedDate)
                    .FirstOrDefault();

                if (content != null)
                {
                    return new GetLastContentUpdateDateResponse
                    {
                        LastContentUpdateDate = content.CreatedDate.Value,
                        Result = "Success",
                        ErrorMessage = "Showing last Content date from relevant Course"
                    };
                }

                var course = _context.DBCourses.SingleOrDefault(c => c.Id == request.CourseId);

                if (course == null)
                {
                    return new GetLastContentUpdateDateResponse
                    {
                        Result = "Failure",
                        ErrorMessage = "Course with relevant Id not found"
                    };
                }

                return new GetLastContentUpdateDateResponse
                {
                    LastContentUpdateDate = course.CreatedDate.Value,
                    Result = "Success",
                    ErrorMessage = "Course has no content, showing Course creation date"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("GetLastContentUpdateDate Service error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new GetLastContentUpdateDateResponse
                {
                    Result = "Failure",
                    ErrorMessage = "GetLastContentUpdateDate Service error: " + ex.Message
                };
            }
        }


        public async Task<ActionResult<CourseActionResponse>> CreateCourse(CourseRequest course)
        {
            try
            {
                if (_validators.IsUserGenuine((int)course.UserId) == false)
                {
                    _logger.LogInformation("CreateCourse failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + course.UserId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(course.UserId) == false && _validators.IsUserSchoolManager(course.UserId) == false
                    && _validators.IsUserMentor(course.UserId) == false)
                {
                    _logger.LogInformation("CreateCourse failed, Only Admins, School Managers and Mentors can create Courses - AdminUserId: " + course.UserId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers and Mentors can create Courses"
                    };
                }

                if (IsSchoolWithGivenIdExists(course.SchoolId) == false)
                {
                    _logger.LogInformation("CreateCourse failed, School with given id not found - AdminUserId: " + course.UserId + " SchoolId: " + course.SchoolId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (IsTopicWithGivenIdExists(course.TopicId) == false)
                {
                    _logger.LogInformation("CreateCourse failed, Topic with given id not found - AdminUserId: " + course.UserId + " SchoolId: " + course.TopicId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Topic with given id not found"
                    };
                }

                if (course.SchoolClass < 0)
                {
                    _logger.LogInformation("CreateCourse failed, School class can't be less then zero - AdminUserId: " + course.UserId + " SchoolId: " + course.TopicId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School class can't be less then zero"
                    };
                }

                DBSchool dbschool = _context.DBSchools.SingleOrDefault(s => s.Id == course.SchoolId);
                School school = _mapper.Map<School>(dbschool);

                DBTopic dbtopic = _context.DBTopics.SingleOrDefault(s => s.Id == course.TopicId);
                Topic topic = _mapper.Map<Topic>(dbtopic);

                DBUser dbuser = _context.DBUsers.SingleOrDefault(s => s.Id == course.HomeroomTeacherId);
                User homeroomTeacher = _mapper.Map<User>(dbuser);

                DBCourse newCourse = new DBCourse()
                {                    
                    SchoolId = school.Id,
                    TopicId = topic.Id,
                    Name = course.Name,
                    SchoolClass = course.SchoolClass,
                    SchoolClassLetter = course.SchoolClassLetter,
                    HomeroomTeacherId = homeroomTeacher.Id,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate= DateTime.UtcNow
                };

                _context.DBCourses.Add(newCourse);
                _context.SaveChanges();

                if (_validators.IsUserMentor(course.UserId) == true)
                {
                    DBCourseUser courseUser = new DBCourseUser()
                    {
                        CourseId = newCourse.Id,
                        UserId = course.UserId,
                        CreatedDate= DateTime.UtcNow,
                        ModifiedDate= DateTime.UtcNow
                    };

                    _context.DBCourseUsers.Add(courseUser);

                    _context.SaveChanges();
                }

                _logger.LogInformation("Course created successfully - UserId: " + course.UserId + " CourseId: " + newCourse.Id);

                return new CourseActionResponse()
                {
                    SchoolResultId = newCourse.Id,
                    Result = "Success",
                    ErrorMessage = "Course created successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateCourse Service error: " + ex.Message + " UserId: " + course.UserId);

                return new CourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "CreateCourse Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<CourseActionResponse>> UpdateCourse(CourseRequest course)
        {
            try
            {
                if (_validators.IsUserGenuine((int)course.UserId) == false)
                {
                    _logger.LogInformation("UpdateCourse failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + course.UserId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(course.UserId) == false && _validators.IsUserSchoolManager(course.UserId) == false
                    && _validators.IsUserMentor(course.UserId) == false)
                {
                    _logger.LogInformation("UpdateCourse failed, Only Admins, School Managers and Mentors can edit Courses - AdminUserId: " + course.UserId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers and Mentors can edit Courses"
                    };
                }

                if (IsCourseWithGivenIdExists(course.CourseId) == false)
                {
                    _logger.LogInformation("UpdateCourse failed, Course with given id not found - AdminUserId: " + course.UserId + " CourseId: " + course.CourseId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Course with given id not found"
                    };
                }

                if (IsSchoolWithGivenIdExists(course.SchoolId) == false)
                {
                    _logger.LogInformation("UpdateCourse failed, Course with given id not found - AdminUserId: " + course.UserId + " SchoolId: " + course.CourseId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (IsTopicWithGivenIdExists(course.TopicId) == false)
                {
                    _logger.LogInformation("UpdateCourse failed, Topic with given id not found - AdminUserId: " + course.UserId + " TopicId: " + course.TopicId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Topic with given id not found"
                    };
                }

                if (IsUserAssignedToCourse((int)course.CourseId, (int)course.UserId) == false)
                {
                    _logger.LogInformation("UpdateCourse failed, Provided user is not assigned to desired course - AdminUserId: " + course.UserId + " CourseId: " + course.CourseId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided user is not assigned to desired course"
                    };
                }

                if (course.SchoolClass < 0)
                {
                    _logger.LogInformation("CreateCourse failed, School class can't be less then zero - AdminUserId: " + course.UserId + " SchoolId: " + course.TopicId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School class can't be less then zero"
                    };
                }

                DBCourse dbcourse = _context.DBCourses.SingleOrDefault(s => s.Id == course.CourseId);

                dbcourse.SchoolId = course.SchoolId;
                dbcourse.TopicId = course.TopicId;
                dbcourse.Name = course.Name;
                dbcourse.ModifiedDate= DateTime.UtcNow;
                dbcourse.SchoolClass = course.SchoolClass;

                _context.SaveChanges();

                _logger.LogInformation("Course updated successfully - UserId: " + course.UserId + " CourseId: " + course.CourseId);

                return new CourseActionResponse()
                {
                    SchoolResultId = dbcourse.Id,
                    Result = "Success",
                    ErrorMessage = "Course updated successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateCourse Service error: " + ex.Message + " UserId: " + course.UserId);

                return new CourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateCourse Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<ActionAllowedResponse>> IsDeleteCourseAllowed(CourseRequest course)
        {
            try
            {
                if (_validators.IsUserGenuine((int)course.UserId) == false)
                {
                    _logger.LogInformation("IsDeleteCourseAllowed failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + course.UserId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Logged in UserId is not same as provided UserId (possible hack attempt!"
                    };
                }


                if (IsCourseHasStudents(course.CourseId) == true)
                {
                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Can't delete course that has Students"
                    };
                }

                if (IsCourseHasNoContent(course.CourseId) == false)
                {
                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Course can't be deleted, it has content - delete this content first"
                    };
                }

                if ((_validators.IsUserAdmin(course.UserId) == false && _validators.IsUserSchoolManager(course.UserId) == false)
                    && (_validators.IsUserMentor(course.UserId) == true && IsUserAssignedToCourse(course.CourseId, course.UserId) == false))
                {
                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Mentors can only delete their own courses. Only Admins and School Managers can delete any Courses."
                    };
                }

                return new ActionAllowedResponse()
                {
                    IsAllowed = true
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("IsDeleteCourseAllowed Service error: " + ex.Message + " UserId: " + course.UserId + " CourseId: " + course.CourseId);

                return new ActionAllowedResponse()
                {
                    IsAllowed = false,
                    Message = "IsDeleteCourseAllowed Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<CourseActionResponse>> DeleteCourse(CourseRequest course)
        {
            try
            {
                if (_validators.IsUserGenuine((int)course.UserId) == false)
                {
                    _logger.LogInformation("DeleteCourse failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + course.UserId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if ((_validators.IsUserAdmin(course.UserId) == false && _validators.IsUserSchoolManager(course.UserId) == false)
                    && (_validators.IsUserMentor(course.UserId) == true && IsUserAssignedToCourse(course.CourseId, course.UserId) == false))
                {
                    _logger.LogInformation("DeleteCourse failed, Mentors can only delete their own courses. Only Admins and School Managers can delete any Courses - AdminUserId: " + course.UserId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentors can only delete their own courses. Only Admins and School Managers can delete any Courses"
                    };
                }

                if (IsCourseWithGivenIdExists(course.CourseId) == false)
                {
                    _logger.LogInformation("DeleteCourse failed, Course with given id not found - AdminUserId: " + course.UserId + " CourseId: " + course.CourseId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Course with given id not found"
                    };
                }

                if (IsUserAssignedToCourse((int)course.CourseId, (int)course.UserId) == false)
                {
                    _logger.LogInformation("DeleteCourse failed, Provided user is not assigned to desired course - AdminUserId: " + course.UserId + " CourseId: " + course.CourseId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided user is not assigned to desired course"
                    };
                }

                DBCourse dbcourse = _context.DBCourses.SingleOrDefault(s => s.Id == course.CourseId);

                if (IsCourseHasStudents(course.CourseId) == true)
                {
                    _logger.LogInformation("DeleteCourse failed, Can't delete course that has Students - AdminUserId: " + course.UserId + " CourseId: " + course.CourseId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Can't delete course that has Students"
                    };
                }

                if (IsCourseHasNoContent(course.CourseId) == false)
                {
                    _logger.LogInformation("DeleteCourse failed, Course can't be deleted, it has content - delete this content first: " + course.UserId + " CourseId: " + dbcourse.Id);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Course can't be deleted, it has content - delete this content first"
                    };
                }

                _context.DBCourses.Remove(dbcourse);

                _context.SaveChanges();

                bool unAssign = UnAssignAllUsersFromCourse(course.UserId, course.CourseId);

                if (unAssign == false)
                {
                    _logger.LogInformation("DeleteCourse failed, Failed to unassign all users from deleted course - AdminUserId: " + course.UserId + " CourseId: " + course.CourseId);

                    return new CourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Failed to unassign all users from deleted course"
                    };
                }

                _logger.LogInformation("Course deleted successfully - UserId: " + course.UserId + " CourseId: " + course.CourseId);

                return new CourseActionResponse()
                {
                    Result = "Success",
                    ErrorMessage = "Course deleted successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("DeleteCourse Service error: " + ex.Message + " UserId: " + course.UserId);

                return new CourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "DeleteCourse Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<ActionAllowedResponse>> IsAssignOrUnassignUserToCourseAllowed(AssignUserToCourseRequest request)
        {
            try
            {
                await _validators.CleanupDuplicateLinksAsync("course");

                if (_validators.IsUserGenuine((int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToCourseAllowed failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.ManagerUserId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(request.ManagerUserId) == false && _validators.IsUserSchoolManager(request.AddedUserId) == true)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToCourseAllowed failed, Only Admins can assign School Managers to Course - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Only Admins can assign School Managers to Course"
                    };
                }

                if (_validators.IsUserSchoolManager(request.ManagerUserId) == true &&
                    _validators.IsUserMentor(request.AddedUserId) == false && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToCourseAllowed failed, School Managers can only assign Mentors and Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "School Managers can only assign Mentors and Students"
                    };
                }

                if (_validators.IsUserMentor(request.ManagerUserId) == true && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToCourseAllowed failed, Mentors can only assign Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Mentors can only assign Students"
                    };
                }


                if (_validators.IsUserMentor(request.AddedUserId) == true && IsCourseHasStudents(request.CourseId) == true)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToCourseAllowed failed, Can't unassign Mentor from course that has Students - AdminUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Can't unassign Mentor from course that has Students"
                    };
                }

                if (_validators.IsUserMentor(request.AddedUserId) == true && IsCourseHasNoContent(request.CourseId) == false
                    && request.IsAssign == false)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToCourseAllowed failed, Can't unassign Mentor from course that has content - delete this content first: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Can't unassign Mentor from course that has content - delete this content first"
                    };
                }

                return new ActionAllowedResponse()
                {
                    IsAllowed = true
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("IsAssignOrUnassignUserToCourseAllowed Service error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                return new ActionAllowedResponse()
                {
                    IsAllowed = false,
                    Message = "IsAssignOrUnassignUserToCourseAllowed Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<AssignUserToCourseActionResponse>> AssignUserToCourse(AssignUserToCourseRequest request)
        {
            try
            {
                await _validators.CleanupDuplicateLinksAsync("course");

                if (_validators.IsUserGenuine((int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("DeleteCourse failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.ManagerUserId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsCourseWithGivenIdExists(request.CourseId) == false)
                {
                    _logger.LogInformation("AssignUserToCourse failed, Course with given id not found - AdminUserId: " + request.ManagerUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Course with given id not found"
                    };
                }

                if (IsUserAssignedToSchool(request.SchoolId, request.AddedUserId) == false)
                {
                    _logger.LogInformation("AssignUserToCourse failed, User is not assigned to School that owns Course - AdminUserId: " + request.ManagerUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "User is not assigned to School that owns Course"
                    };
                }

                if (IsUserAssignedToCourse((int)request.CourseId, (int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("AssignUserToCourse failed, Provided manager user is not assigned to desired course - AdminUserId: " + request.ManagerUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided manager user is not assigned to desired course"
                    };
                }

                if (_validators.IsUserStudent(request.AddedUserId) == false && _validators.IsUserMentor(request.AddedUserId) == false
                    && _validators.IsUserSchoolManager(request.AddedUserId) == false)
                {
                    _logger.LogInformation("AssignUserToCourse failed, Only Students, Mentors or School Managers can be assigned to Course - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Students, Mentors or School Managers can be assigned to Course"
                    };
                }

                if (_validators.IsUserAdmin(request.ManagerUserId) == false && _validators.IsUserSchoolManager(request.ManagerUserId) == false
                    && _validators.IsUserMentor(request.ManagerUserId) == false)
                {
                    _logger.LogInformation("AssignUserToCourse failed, Only Admins, School Managers and Mentors can assign users to Course - AdminUserId: " + request.ManagerUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers and Mentors can assign users to Course"
                    };
                }

                if (_validators.IsUserAdmin(request.ManagerUserId) == false && _validators.IsUserSchoolManager(request.AddedUserId) == true)
                {
                    _logger.LogInformation("AssignUserToCourse failed, Only Admins can assign School Managers to Course - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins can assign School Managers to Course"
                    };
                }

                if (_validators.IsUserSchoolManager(request.ManagerUserId) == true &&
                    _validators.IsUserMentor(request.AddedUserId) == false && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("AssignUserToCourse failed, School Managers can only assign Mentors and Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School Managers can only assign Mentors and Students"
                    };
                }

                if (_validators.IsUserMentor(request.ManagerUserId) == true && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("AssignUserToCourse failed, Mentors can only assign Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentors can only assign Students"
                    };
                }

                DBCourseUser courseUser = new DBCourseUser()
                {
                    CourseId = request.CourseId,
                    UserId = request.AddedUserId,                    
                    CreatedDate= DateTime.UtcNow,
                    ModifiedDate= DateTime.UtcNow
                };

                _context.DBCourseUsers.Add(courseUser);

                _context.SaveChanges();

                _logger.LogInformation("User assigned to course successfully - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                return new AssignUserToCourseActionResponse()
                {
                    CourseId = request.CourseId,
                    ManagerUserId = request.ManagerUserId,
                    AddedUserId = request.AddedUserId,
                    Result = "Success",
                    ErrorMessage = "User assigned to course successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("AssignUserToCourse Service error: " + ex.Message + " UserId: " + request.ManagerUserId);

                return new AssignUserToCourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "AssignUserToCourse Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<AssignUserToCourseActionResponse>> UnAssignUserFromCourse(AssignUserToCourseRequest request)
        {
            try
            {
                await _validators.CleanupDuplicateLinksAsync("course");

                if (_validators.IsUserGenuine((int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserToCourse failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.ManagerUserId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsCourseWithGivenIdExists(request.CourseId) == false)
                {
                    _logger.LogInformation("UnAssignUserToCourse failed, Course with given id not found - AdminUserId: " + request.ManagerUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (IsUserAssignedToCourse((int)request.CourseId, (int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserToCourse failed, Provided manager user is not assigned to desired course - AdminUserId: " + request.ManagerUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided manager user is not assigned to desired course"
                    };
                }

                if (_validators.IsUserStudent(request.AddedUserId) == false && _validators.IsUserMentor(request.AddedUserId) == false
                    && _validators.IsUserSchoolManager(request.AddedUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromCourse failed, Only Students, Mentors or School Managers can be unassigned from Course - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Students, Mentors or School Managers can be unassigned from course"
                    };
                }

                if (_validators.IsUserAdmin(request.ManagerUserId) == false && _validators.IsUserSchoolManager(request.AddedUserId) == true)
                {
                    _logger.LogInformation("UnAssignUserFromCourse failed, Only Admins can unassign School Managers from Course - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins can unassign School Managers to Course"
                    };
                }

                if (_validators.IsUserSchoolManager(request.ManagerUserId) == true &&
                    _validators.IsUserMentor(request.AddedUserId) == false && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromCourse failed, School Managers can only unassign Mentors and Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School Managers can only unassign Mentors and Students"
                    };
                }

                if (_validators.IsUserMentor(request.ManagerUserId) == true && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromCourse failed, Mentors can only assign Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentors can only unassign Students"
                    };
                }


                if (_validators.IsUserMentor(request.AddedUserId) == true && IsCourseHasStudents(request.CourseId) == true)
                {
                    _logger.LogInformation("UnAssignUserFromCourse failed, Can't unassign Mentor from course that has Students - AdminUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Can't unassign Mentor from course that has Students"
                    };
                }

                if (_validators.IsUserMentor(request.AddedUserId) == true && IsCourseHasNoContent(request.CourseId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromCourse failed, Can't unassign Mentor from course that has content - delete this content first: " + request.AddedUserId + " CourseId: " + request.CourseId);

                    return new AssignUserToCourseActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Can't unassign Mentor from course that has content - delete this content first"
                    };
                }

                DBCourseUser updatedCourseUser = _context.DBCourseUsers.SingleOrDefault(s => s.CourseId == request.CourseId && s.UserId == request.AddedUserId);
                _context.DBCourseUsers.Remove(updatedCourseUser);

                _context.SaveChanges();

                _logger.LogInformation("User unassigned from course successfully - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                return new AssignUserToCourseActionResponse()
                {
                    CourseId = request.CourseId,
                    ManagerUserId = request.ManagerUserId,
                    AddedUserId = request.AddedUserId,
                    Result = "Success",
                    ErrorMessage = "User unassigned from school successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("UnAssignUserFromCourse Service error: " + ex.Message + " UserId: " + request.ManagerUserId);

                return new AssignUserToCourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UnAssignUserFromSchool Service error: " + ex.Message
                };
            }
        }

        private bool UnAssignAllUsersFromCourse(int userId, int courseId)
        {
            try
            {
                List<DBCourseUser> updatedCourseUsers = _context.DBCourseUsers.Where(s => s.CourseId == courseId).ToList();

                foreach (DBCourseUser updatedCourseUser in updatedCourseUsers)
                {
                    _context.DBCourseUsers.Remove(updatedCourseUser);
                    _context.SaveChanges();
                }

                return true;
            }

            catch (Exception ex)
            {
                _logger.LogError("UnAssignAllUsersFromCourse failed, Error: " + ex.Message + " UserId: " + userId);

                return false;
            }
        }

        private bool IsCourseWithGivenIdExists(int courseId)
        {
            List<DBCourse> courses = _context.DBCourses.Where(s => s.Id == courseId).ToList();

            return courses.Count == 1;
        }

        private bool IsCourseHasStudents(int courseId)
        {
            List<DBCourseUser> courseUsers = _context.DBCourseUsers.Where(c => c.CourseId == courseId).ToList();

            List<int> userIds = courseUsers.ConvertAll(x => x.UserId);

            List<DBUser> students = _context.DBUsers.Where(u => u.UserTypeId == 1 && userIds.Contains(u.Id)).ToList();

            return students.Count > 0;
        }

        private bool IsSchoolWithGivenIdExists(int schoolId)
        {
            List<DBSchool> schools = _context.DBSchools.Where(s => s.Id == schoolId).ToList();

            return schools.Count == 1;
        }

        private bool IsTopicWithGivenIdExists(int topicId)
        {
            List<DBTopic> topics = _context.DBTopics.Where(s => s.Id == topicId).ToList();

            return topics.Count == 1;
        }

        private bool IsUserAssignedToCourse(int courseId, int userId)
        {
            if (_validators.IsUserAdmin(userId))
                return true;

            List<DBCourseUser> courseUsers = _context.DBCourseUsers.Where(s => s.CourseId == courseId && s.UserId == userId).ToList();

            return courseUsers.Count == 1;
        }

        private bool IsUserAssignedToSchool(int schoolId, int userId)
        {
            if (_validators.IsUserAdmin(userId))
                return true;

            List<DBSchoolUser> schoolUsers = _context.DBSchoolUsers.Where(s => s.SchoolId == schoolId && s.UserId == userId).ToList();

            return schoolUsers.Count == 1;
        }

        private bool IsCourseHasNoContent(int courseId)
        {
            List<DBContent> tasks = _context.DBContents.Where(c => c.CourseId == courseId).ToList();

            return tasks.Count == 0;
        }

        private List<User> GetCourseMentors(int courseId)
        {
            List<DBUser> users = new List<DBUser>();
            
            List<DBCourseUser> courseUsers = _context.DBCourseUsers.Where(c => c.CourseId == courseId).ToList();

            List<int> userIds = courseUsers.ConvertAll(x => x.UserId);

            users = _context.DBUsers.Where(u => u.UserTypeId == 0 && userIds.Contains(u.Id)).ToList();
            
            return _mapper.Map<List<User>>(users);
        }

        private int GetCourseStudentsCount(int courseId)
        {
            List<DBUser> users = new List<DBUser>();

            List<DBCourseUser> courseUsers = _context.DBCourseUsers.Where(c => c.CourseId == courseId).ToList();

            List<int> userIds = courseUsers.ConvertAll(x => x.UserId);

            users = _context.DBUsers.Where(u => u.UserTypeId == 1 && userIds.Contains(u.Id)).ToList();

            return users.Count;
        }
    }
}
