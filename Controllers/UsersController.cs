using MentorsAndStudents.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController
    {
        private readonly IUsersService _usersService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUsersService usersService, ILogger<UsersController> logger)
        {
            _usersService = usersService;
            _logger = logger;
        }

        [HttpPost("view_users")]
        public async Task<ActionResult<UsersViewResponse>> ViewUsers()
        {
            try
            {
                return await _usersService.ViewUsers();
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewUsers Controller error: " + ex.Message);

                return new UsersViewResponse()
                {
                    UsersViews = new List<User>(),
                    Result = "Failure",
                    ErrorMessage = "ViewUsers Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("create_user")]
        public async Task<ActionResult<UserActionResponse>> CreateUser(UserRequest user)
        {
            try
            {
                return await _usersService.CreateUser(user);
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateUser Controller error: " + ex.Message + " Email: " + user.Email + " UserTypeId: " + user.UserTypeId);

                return new UserActionResponse()
                {
                    ResultId = -1,
                    Result = "Failure",
                    ErrorMessage = "CreateUser Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginActionResponse>> Login(LoginRequest request)
        {
            try
            {
                return await _usersService.Login(request);
            }

            catch (Exception ex)
            {
                _logger.LogError("Login Controller error: " + ex.Message + " Email: " + request.Email + " UserTypeId: " + request.UserTypeId);

                return new LoginActionResponse()
                {
                    Email = request.Email,
                    Token = "Error",
                    Result = "Failure",
                    ErrorMessage = "Login Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("refresh_token")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken()
        {
            try
            {
                return await _usersService.RefreshToken();
            }

            catch (Exception ex)
            {
                _logger.LogError("RefreshToken Controller error: " + ex.Message);

                return new RefreshTokenResponse()
                {
                    Token = "",
                    Result = "Failure",
                    ErrorMessage = "RefreshToken Controller error: " + ex.Message
                };
            }
        }
    } 
}
