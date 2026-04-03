using MentorsAndStudents.Requests;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MentorsAndStudents
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class CourseController
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService courseService, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        [HttpPost("view_courses")]
        public async Task<ActionResult<CoursesViewResponse>> ViewCourses(CoursesViewRequest view)
        {
            try
            {
                return await _courseService.ViewCourses(view);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewCourses Controller error: " + ex.Message + " UserId: " + view.UserId);

                return new CoursesViewResponse()
                {
                    CoursesViews = new List<Course>(),
                    Result = "Failure",
                    ErrorMessage = "ViewCourses Controller error: " + ex.Message
                };
            }
        }


        [HttpPost("view_single_course_props")]
        public async Task<ActionResult<CoursePropsResponse>> ViewSingleCourseProps(CoursesViewRequest request)
        {
            try
            {
                return await _courseService.ViewSingleCourseProps(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewSingleCourseProps Controller error: " + ex.Message + " UserId: " + request.UserId);

                return new CoursePropsResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "ViewSingleCourseProps Controller error: " + ex.Message
                };
            }
        }
        [HttpPost("view_users_assigned_or_not_to_course")]
        public async Task<ActionResult<UsersViewResponse>> ViewUsersAssignedOrNotToCourse(UsersViewRequest request)
        {
            try
            {
                return await _courseService.ViewUsersAssignedOrNotToCourse(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewUsersAssignedOrNotToCourse Controller error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new UsersViewResponse()
                {
                    UsersViews = new List<User>(),
                    Result = "Failure",
                    ErrorMessage = "ViewUsersAssignedOrNotToCourse Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("get_all_course_users_by_type_count")]
        public async Task<ActionResult<SchoolOrCourseUsersByTypeAndCoursesCountResponse>> GetAllCourseUsersByTypeCount(UsersViewRequest request)
        {
            try
            {
                return await _courseService.GetAllCourseUsersByTypeCount(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("GetAllCourseUsersByTypeCount Controller error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "GetAllCourseUsersByTypeCount Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("get_last_content_update_date")]
        public async Task<ActionResult<GetLastContentUpdateDateResponse>> GetLastContentUpdateDate(UsersViewRequest request)
        {
            try
            {
                return await _courseService.GetLastContentUpdateDate(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("GetLastContentUpdateDate Controller error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new GetLastContentUpdateDateResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "GetLastContentUpdateDate Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("create_course")]
        public async Task<ActionResult<CourseActionResponse>> CreateCourse(CourseRequest course)
        {
            try
            {
                return await _courseService.CreateCourse(course);
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateCourse Controller error: " + ex.Message + " UserId: " + course.UserId);

                return new CourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "CreateCourse Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("update_course")]
        public async Task<ActionResult<CourseActionResponse>> UpdateCourse(CourseRequest course)
        {
            try
            {
                return await _courseService.UpdateCourse(course);
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateCourse Controller error: " + ex.Message + " UserId: " + course.UserId + " CourseId: " + course.CourseId);

                return new CourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateCourse Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("is_delete_course_allowed")]
        public async Task<ActionResult<ActionAllowedResponse>> IsDeleteCourseAllowed(CourseRequest course)
        {
            try
            {
                return await _courseService.IsDeleteCourseAllowed(course);
            }

            catch (Exception ex)
            {
                _logger.LogError("IsDeleteCourseAllowed Controller error: " + ex.Message + " UserId: " + course.UserId + " CourseId: " + course.CourseId);

                return new ActionAllowedResponse()
                {
                    IsAllowed = false,
                    Message = "IsDeleteCourseAllowed Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("delete_course")]
        public async Task<ActionResult<CourseActionResponse>> DeleteCourse(CourseRequest course)
        {
            try
            {
                return await _courseService.DeleteCourse(course);
            }

            catch (Exception ex)
            {
                _logger.LogError("DeleteCourse Controller error: " + ex.Message + " UserId: " + course.UserId + " CourseId: " + course.CourseId);

                return new CourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "DeleteCourse Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("is_assign_or_unassign_user_to_course_allowed")]
        public async Task<ActionResult<ActionAllowedResponse>> IsAssignOrUnassignUserToCourseAllowed(AssignUserToCourseRequest request)
        {
            try
            {
                return await _courseService.IsAssignOrUnassignUserToCourseAllowed(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("IsAssignOrUnassignUserToCourseAllowed Controller error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                return new ActionAllowedResponse()
                {
                    IsAllowed = false,
                    Message = "IsAssignOrUnassignUserToCourseAllowed Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("assign_user_to_course")]
        public async Task<ActionResult<AssignUserToCourseActionResponse>> AssignUserToCourse(AssignUserToCourseRequest request)
        {
            try
            {
                return await _courseService.AssignUserToCourse(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("AssignUserToCourse Controller error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                return new AssignUserToCourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "AssignUserToCourse Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("unassign_user_from_course")]
        public async Task<ActionResult<AssignUserToCourseActionResponse>> UnAssignUserFromCourse(AssignUserToCourseRequest request)
        {
            try
            {
                return await _courseService.UnAssignUserFromCourse(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("UnAssignUserFromCourse Controller error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.CourseId);

                return new AssignUserToCourseActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UnAssignUserFromCourse Controller error: " + ex.Message
                };
            }
        }
    }
}
