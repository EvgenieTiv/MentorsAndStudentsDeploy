using Azure.Core;
using MentorsAndStudents.Requests;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MentorsAndStudents
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class SchoolController
    {
        private readonly ISchoolsService _schoolsService;
        private readonly ILogger<SchoolController> _logger;

        public SchoolController(ISchoolsService schoolsService, ILogger<SchoolController> logger)
        {
            _schoolsService = schoolsService;
            _logger = logger;
        }

        [HttpPost("view_schools")]
        public async Task<ActionResult<SchoolsViewResponse>> ViewSchools(SchoolsViewRequest view)
        {
            try
            {
                return await _schoolsService.ViewSchools(view);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewSchools Controller error: " + ex.Message + " UserId: " + view.UserId);

                return new SchoolsViewResponse()
                {
                    SchoolsViews = new List<School>(),
                    Result = "Failure",
                    ErrorMessage = "ViewSchools Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("view_single_school_props")]
        public async Task<ActionResult<SchoolPropsResponse>> ViewSingleSchoolProps(SchoolsViewRequest request)
        {
            try
            {
                return await _schoolsService.ViewSingleSchoolProps(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewSingleSchoolProps Controller error: " + ex.Message + " UserId: " + request.UserId);

                return new SchoolPropsResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "ViewSingleSchoolProps Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("view_users_assigned_or_not_to_school")]
        public async Task<ActionResult<UsersViewResponse>> ViewUsersAssignedOrNotToSchool(UsersViewRequest request)
        {
            try
            {
                return await _schoolsService.ViewUsersAssignedOrNotToSchool(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewUsersFromSchool Controller error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new UsersViewResponse()
                {
                    UsersViews = new List<User>(),
                    Result = "Failure",
                    ErrorMessage = "ViewUsers Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("get_all_school_users_by_type_and_courses_count")]
        public async Task<ActionResult<SchoolOrCourseUsersByTypeAndCoursesCountResponse>> GetAllSchoolUsersByTypeAndCoursesCount(UsersViewRequest request)
        {
            try
            {
                return await _schoolsService.GetAllSchoolUsersByTypeAndCoursesCount(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("GetAllSchoolUsersByTypeAndCoursesCount Controller error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "GetAllSchoolUsersByTypeAndCoursesCount Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("create_school")]
        public async Task<ActionResult<SchoolActionResponse>> CreateSchool(SchoolRequest school)
        {
            try
            {
                return await _schoolsService.CreateSchool(school);
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateSchool Controller error: " + ex.Message + " UserId: " + school.UserId);

                return new SchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "CreateSchool Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("update_school")]
        public async Task<ActionResult<SchoolActionResponse>> UpdateSchool(SchoolRequest school)
        {
            try
            {
                return await _schoolsService.UpdateSchool(school);
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateSchool Controller error: " + ex.Message + " UserId: " + school.UserId + " SchoolId: " + school.SchoolId);

                return new SchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateSchool Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("is_delete_school_allowed")]
        public async Task<ActionResult<ActionAllowedResponse>> IsDeleteSchoolAllowed(SchoolRequest school)
        {
            try
            {
                return await _schoolsService.IsDeleteSchoolAllowed(school);
            }

            catch (Exception ex)
            {
                _logger.LogError("IsDeleteSchoolAllowed Controller error: " + ex.Message + " UserId: " + school.UserId + " SchoolId: " + school.SchoolId);

                return new ActionAllowedResponse()
                {
                    IsAllowed = false,
                    Message = "IsDeleteSchoolAllowed Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("delete_school")]
        public async Task<ActionResult<SchoolActionResponse>> DeleteSchool(SchoolRequest school)
        {
            try
            {
                return await _schoolsService.DeleteSchool(school);
            }

            catch (Exception ex)
            {
                _logger.LogError("DeleteSchool Controller error: " + ex.Message + " UserId: " + school.UserId + " CourseId: " + school.SchoolId);

                return new SchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "DeleteSchool Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("is_assign_or_unassign_user_to_school_allowed")]
        public async Task<ActionResult<ActionAllowedResponse>> IsAssignOrUnassignUserToSchoolAllowed(AssignUserToSchoolRequest request)
        {
            try
            {
                return await _schoolsService.IsAssignOrUnassignUserToSchoolAllowed(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("IsAssignUserToSchoolAllowed Controller error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " SchoolId: " + request.SchoolId);

                return new ActionAllowedResponse()
                {
                    IsAllowed = false,
                    Message = "IsAssignUserToSchoolAllowed Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("assign_user_to_school")]
        public async Task<ActionResult<AssignUserToSchoolActionResponse>> AssignUserToSchool(AssignUserToSchoolRequest request)
        {
            try
            {
                return await _schoolsService.AssignUserToSchool(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("AssignUserToSchool Controller error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                return new AssignUserToSchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "AssignUserToSchool Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("unassign_user_from_school")]
        public async Task<ActionResult<AssignUserToSchoolActionResponse>> UnAssignUserFromSchool(AssignUserToSchoolRequest request)
        {
            try
            {
                return await _schoolsService.UnAssignUserFromSchool(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("UnAssignUserFromSchool Controller error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                return new AssignUserToSchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UnAssignUserFromSchool Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("view_mentor_school_topic_connections")]
        public async Task<ActionResult<ViewMentorSchoolTopicConnectionsResponse>> ViewMentorSchoolTopicConnections(MentorSchoolTopicRequest request)
        {
            try
            {
                return await _schoolsService.ViewMentorSchoolTopicConnections(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewMentorSchoolTopicConnections Controller error: " + ex.Message + " AdminId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId);

                return new ViewMentorSchoolTopicConnectionsResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "ViewMentorSchoolTopicConnections Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("assign_mentor_school_topic")]
        public async Task<ActionResult<AssignMentorSchoolTopicResponse>> AssignMentorSchoolTopic(MentorSchoolTopicRequest request)
        {
            try
            {
                return await _schoolsService.AssignMentorSchoolTopic(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("AssignMentorSchoolTopic Controller error: " + ex.Message + " AdminId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId + " TopicId: " + request.TopicId);

                return new AssignMentorSchoolTopicResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "AssignMentorSchoolTopic Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("unassign_mentor_school_topic")]
        public async Task<ActionResult<AssignMentorSchoolTopicResponse>> UnAssignMentorSchoolTopic(MentorSchoolTopicRequest request)
        {
            try
            {
                return await _schoolsService.UnAssignMentorSchoolTopic(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("UnAssignMentorSchoolTopic Controller error: " + ex.Message + " AdminId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId + " TopicId: " + request.TopicId);

                return new AssignMentorSchoolTopicResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UnAssignMentorSchoolTopic Controller error: " + ex.Message
                };
            }
        }
    }
}
