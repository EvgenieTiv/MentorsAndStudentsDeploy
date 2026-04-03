using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public interface IUserTypesService
    {
        Task<ActionResult<UserTypesViewResponse>> ViewUserTypes();
    }
}
