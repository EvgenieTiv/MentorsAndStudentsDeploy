using AutoMapper;
using Azure.Core;
using MentorsAndStudents.Common;
using MentorsAndStudents.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static MentorsAndStudents.UsersController;

namespace MentorsAndStudents
{
    public class UsersService: IUsersService
    {
        private readonly IConfiguration _configuration;
        private readonly MentorsAndStudentsContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IValidators _validators;

        public UsersService(IConfiguration configuration, MentorsAndStudentsContext context, IMapper mapper, ILogger<UsersService> logger, IHttpContextAccessor httpContextAccessor, IValidators validators)
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _validators = validators;
        }

        public async Task<ActionResult<UsersViewResponse>> ViewUsers()
        {
            try
            {
                List<DBUser> dbusers = _context.DBUsers.ToList();

                return new UsersViewResponse()
                {
                    UsersViews = _mapper.Map<List<User>>(dbusers).ToList(),
                    Result = "Success",
                    ErrorMessage = "Users shown"
                };
            }
            
            catch (Exception ex)
            {
                _logger.LogError("ViewUsers Service error: " + ex.Message);

                return new UsersViewResponse()
                {
                    UsersViews = new List<User>(),
                    Result = "Failure",
                    ErrorMessage = "ViewUsers Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<UserActionResponse>> CreateUser(UserRequest user)
        {
            try
            { 
                if (IsEmailUniquePerUserType(user.Email, user.UserTypeId) == false)
                {
                    _logger.LogInformation("Create user failed, Provided Email already exists for selected User Type - Email: " + user.Email + " User Type: " + user.UserTypeId);

                    return new UserActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided Email already exists for selected User Type"
                    };
                }

                if (IsEmailValid(user.Email) == false)
                {
                    _logger.LogInformation("Create user failed, Provided Email is invalid - Email: " + user.Email + " User Type: " + user.UserTypeId);

                    return new UserActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided Email is invalid"
                    };
                }

                string? passwordValidationError = _validators.ValidatePassword(user.Password, user.Email, user.FirstName, user.LastName);

                if (passwordValidationError != null)
                {
                    _logger.LogInformation("Create user failed, "+ passwordValidationError + user.Email + " User Type: " + user.UserTypeId);

                    return new UserActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = passwordValidationError
                    };
                }

                if (user.Password != user.ConfirmPassword)
                {
                    _logger.LogInformation("Create user failed, Passwords don't match: " + user.Email + " User Type: " + user.UserTypeId);

                    return new UserActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Passwords don't match"
                    };
                }

                if (user.UserTypeId != 0 && user.UserTypeId != 1 && user.AdminCode != _configuration["AdminCodeTemp:AdminCode"])
                {
                    _logger.LogInformation("Login failed: For any User Type except Mentor and Student must provide a valid code, the provided code is invalid - Email: " + user.Email + " User Type: " + user.UserTypeId);

                    return new UserActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "For any User Type except Mentor and Student must provide a valid code, the provided code is invalid"
                    };
                }

                if (user.Country == null || user.Country == "")
                {
                    _logger.LogInformation("Login failed: Country field can't be empty - Email: " + user.Email);

                    return new UserActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Country field can't be empty"
                    };
                }

                if (user.City == null || user.City == "")
                {
                    _logger.LogInformation("Login failed: City field can't be empty - Email: " + user.Email);

                    return new UserActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "City field can't be empty"
                    };
                }

                string hashPassword = Encrypt.Sha256(user.Password);

                DBUser newUser = new DBUser()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserTypeId = user.UserTypeId,
                    Country = user.Country,
                    City = user.City,
                    Password = hashPassword
                };

                _context.DBUsers.Add(newUser);
                _context.SaveChanges();

                _logger.LogInformation("Create user successful - Email: " + user.Email + " User Type: " + user.UserTypeId);

                return new UserActionResponse()
                {
                    ResultId = newUser.Id,
                    Result = "Success",
                    ErrorMessage = "User created successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateUser Service error: " + ex.Message + " Email: " + user.Email + " UserTypeId: " + user.UserTypeId);

                return new UserActionResponse()
                {
                    ResultId = -1,
                    Result = "Failure",
                    ErrorMessage = "CreateUser Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<LoginActionResponse>> Login(LoginRequest request)
        {
            try
             {
                (int, string, string, string) userIdAndToken = ValidatePasswordAndCreateToken(request);

                int userId = userIdAndToken.Item1;
                string token = userIdAndToken.Item2;
                string firstName = userIdAndToken.Item3;
                string lastName = userIdAndToken.Item4;

                if (token == "Error")
                {
                    _logger.LogInformation("Login failed wrong credentials - Email: "+ request.Email +" User Type: "+request.UserTypeId);

                    return new LoginActionResponse()
                    {
                        Email = request.Email,
                        Token = token,
                        Result = "Failure",
                        ErrorMessage = "Wrong credentials"
                    };
                }
                
                _logger.LogInformation("Login successful - Email: " + request.Email + " User Type: " + request.UserTypeId);

                return new LoginActionResponse()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    UserId = userId,
                    Email = request.Email,
                    UserTypeId = request.UserTypeId,
                    Token = token,
                    Result = "Success",
                    ErrorMessage = "Login successful"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("Login Service error: " + ex.Message + " Email: " + request.Email + " UserTypeId: " + request.UserTypeId);

                return new LoginActionResponse()
                {
                    Email = request.Email,
                    Token = "",
                    Result = "Failure",
                    ErrorMessage = "Login Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken()
        {
            try
            {
                string? refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogInformation("RefreshToken failed - No refresh token provided");

                    return new RefreshTokenResponse
                    {
                        Token = "",
                        Result = "Failure",
                        ErrorMessage = "No refresh token provided"
                    };
                }

                DBUser user = _context.DBUsers.SingleOrDefault(u => u.RefreshToken == refreshToken);

                if (user == null)
                {
                    _logger.LogInformation("RefreshToken failed - Invalid refresh token");

                    return new RefreshTokenResponse
                    {
                        Token = "",
                        Result = "Failure",
                        ErrorMessage = "Invalid refresh token"
                    };
                }


                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = CreateTokenDescriptor(user.Id, user.Email, user.UserTypeId);
                var token = tokenHandler.CreateToken(tokenDescriptor);
                string newAccessToken = tokenHandler.WriteToken(token);

                return new RefreshTokenResponse
                {
                    Token = newAccessToken,
                    Result = "Success",
                    ErrorMessage = "Access token refreshed"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("RefreshToken Service error: " + ex.Message);

                return new RefreshTokenResponse()
                {
                    Token = "",
                    Result = "Failure",
                    ErrorMessage = "RefreshToken Service error: " + ex.Message
                };
            }
        }

        private bool IsEmailUniquePerUserType(string email, int userTypeId)
        {
            List<DBUser> emails = _context.DBUsers.Where(u => u.Email == email && u.UserTypeId == userTypeId).ToList();

            return emails.Count == 0;
        }

        private bool IsEmailValid(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                _logger.LogError("Email provided for login/sign up is invalid: " + email);

                return false;
            }
        }

        private (int, string, string, string) ValidatePasswordAndCreateToken(LoginRequest request)
        {
            try
            {
                string answer = "Error";

                List<User> emails = _mapper.Map<List<User>>(_context.DBUsers.Where(u => u.Email == request.Email && u.UserTypeId == request.UserTypeId));

                int userId = 0;

                if (emails.Count == 1)
                {
                    string hashPassword = Encrypt.Sha256(request.Password);

                    if (emails[0].Password == hashPassword)
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var tokenDescriptor = CreateTokenDescriptor(emails[0].Id, request.Email, request.UserTypeId);
                        userId = emails[0].Id;
                        GenerateAndSaveRefreshToken(userId);
                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        string userToken = tokenHandler.WriteToken(token);
                        answer = userToken;  
                    }
                }

                _httpContextAccessor.HttpContext.Session.SetInt32("UserId", userId);
                return (userId, answer, emails[0].FirstName, emails[0].LastName);
            }

            catch (Exception ex)
            {
                _logger.LogError("ValidatePasswordAndCreateToken failed with error: " + ex.Message);

                return (0, "Error", "", "");
            }
        }

        private SecurityTokenDescriptor CreateTokenDescriptor(int userId, string email, int userTypeId)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, email),
                    new Claim("UserTypeId", userTypeId.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenDescriptor;
        }

        private string GenerateAndSaveRefreshToken(int userId)
        {
            string refreshToken = Guid.NewGuid().ToString();

            var user = _context.DBUsers.SingleOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                _context.SaveChanges();
            }

            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                "refreshToken",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Только HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

            return refreshToken;
        }
    }
}
