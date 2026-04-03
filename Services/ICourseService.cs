using MentorsAndStudents.Requests;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public interface ICourseService
    {
        Task<ActionResult<CoursesViewResponse>> ViewCourses(CoursesViewRequest view);
        Task<ActionResult<CoursePropsResponse>> ViewSingleCourseProps(CoursesViewRequest request);
        Task<ActionResult<UsersViewResponse>> ViewUsersAssignedOrNotToCourse(UsersViewRequest request);
        Task<ActionResult<SchoolOrCourseUsersByTypeAndCoursesCountResponse>> GetAllCourseUsersByTypeCount(UsersViewRequest request);
        Task<ActionResult<GetLastContentUpdateDateResponse>> GetLastContentUpdateDate(UsersViewRequest request);
        Task<ActionResult<CourseActionResponse>> CreateCourse(CourseRequest course);
        Task<ActionResult<CourseActionResponse>> UpdateCourse(CourseRequest course);
        Task<ActionResult<ActionAllowedResponse>> IsDeleteCourseAllowed(CourseRequest course);
        Task<ActionResult<CourseActionResponse>> DeleteCourse(CourseRequest course);
        Task<ActionResult<ActionAllowedResponse>> IsAssignOrUnassignUserToCourseAllowed(AssignUserToCourseRequest request);
        Task<ActionResult<AssignUserToCourseActionResponse>> AssignUserToCourse(AssignUserToCourseRequest request);
        Task<ActionResult<AssignUserToCourseActionResponse>> UnAssignUserFromCourse(AssignUserToCourseRequest request);
    }
}
