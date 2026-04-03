using MentorsAndStudents.Requests;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public interface ISchoolsService
    {
        Task<ActionResult<SchoolsViewResponse>> ViewSchools(SchoolsViewRequest view);
        Task<ActionResult<SchoolPropsResponse>> ViewSingleSchoolProps(SchoolsViewRequest request);
        Task<ActionResult<UsersViewResponse>> ViewUsersAssignedOrNotToSchool(UsersViewRequest request);
        Task<ActionResult<SchoolOrCourseUsersByTypeAndCoursesCountResponse>> GetAllSchoolUsersByTypeAndCoursesCount(UsersViewRequest request);
        Task<ActionResult<SchoolActionResponse>> CreateSchool(SchoolRequest school);
        Task<ActionResult<SchoolActionResponse>> UpdateSchool(SchoolRequest school);
        Task<ActionResult<ActionAllowedResponse>> IsDeleteSchoolAllowed(SchoolRequest school);
        Task<ActionResult<SchoolActionResponse>> DeleteSchool(SchoolRequest school);
        Task<ActionResult<ActionAllowedResponse>> IsAssignOrUnassignUserToSchoolAllowed(AssignUserToSchoolRequest request);
        Task<ActionResult<AssignUserToSchoolActionResponse>> AssignUserToSchool(AssignUserToSchoolRequest request);
        Task<ActionResult<AssignUserToSchoolActionResponse>> UnAssignUserFromSchool(AssignUserToSchoolRequest request);
        Task<ActionResult<ViewMentorSchoolTopicConnectionsResponse>> ViewMentorSchoolTopicConnections(MentorSchoolTopicRequest request);
        Task<ActionResult<AssignMentorSchoolTopicResponse>> AssignMentorSchoolTopic(MentorSchoolTopicRequest request);
        Task<ActionResult<AssignMentorSchoolTopicResponse>> UnAssignMentorSchoolTopic(MentorSchoolTopicRequest request);
    }
}
