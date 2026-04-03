using Microsoft.AspNetCore.Mvc;
using static MentorsAndStudents.UsersController;

namespace MentorsAndStudents
{
    public interface IUsersService
    {
        Task<ActionResult<UsersViewResponse>> ViewUsers();
        Task<ActionResult<UserActionResponse>> CreateUser(UserRequest user);
        Task<ActionResult<LoginActionResponse>> Login(LoginRequest request);
        Task<ActionResult<RefreshTokenResponse>> RefreshToken();
    }
}
