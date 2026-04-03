using AutoMapper;
using Azure.Core;
using MentorsAndStudents.Common;
using MentorsAndStudents.Context;
using MentorsAndStudents.Requests;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MentorsAndStudents
{
    public class SchoolsService : ISchoolsService
    {
        private readonly MentorsAndStudentsContext _context;
        private readonly IValidators _validators;
        private readonly IMapper _mapper;
        private readonly ILogger<SchoolsService> _logger;

        public SchoolsService(MentorsAndStudentsContext context, IMapper mapper, IValidators validators, ILogger<SchoolsService> logger)
        {
            _context = context;
            _validators = validators;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ActionResult<SchoolsViewResponse>> ViewSchools(SchoolsViewRequest view)
        {
            try
            {
                List<School> schools = new List<School>();

                if (_validators.IsUserGenuine((int)view.UserId) == false)
                {
                    _logger.LogInformation("ViewSchools failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + view.UserId);

                    return new SchoolsViewResponse()
                    {
                        SchoolsViews = new List<School>(),
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(view.UserId) == true)
                {
                    List<DBSchool> dbschools = _context.DBSchools.ToList();
                    schools = _mapper.Map<List<School>>(dbschools);
                }

                else if (_validators.IsUserSchoolManager(view.UserId) == true || _validators.IsUserMentor(view.UserId) == true 
                    || _validators.IsUserStudent(view.UserId) == true)
                {
                    List<DBSchoolUser> schoolUsers = _context.DBSchoolUsers.Where(s => s.UserId == view.UserId).ToList();
                    List<int> schoolIds = schoolUsers.ConvertAll(x => x.SchoolId);

                    List<DBSchool> dbschools = _context.DBSchools.Where(s => schoolIds.Contains(s.Id)).ToList();

                    schools = _mapper.Map<List<School>>(dbschools);

                    schools = schools.OrderBy(s => s.Country)
                                     .ThenBy(s => s.City)        
                                     .ThenBy(s => s.Name)
                                     .ToList();
                }

                else
                {
                    _logger.LogError("ViewSchools failed: Only Admins, School Managers, Mentors and Students can view schools" + " UserId: " + view.UserId);

                    return new SchoolsViewResponse()
                    {
                        SchoolsViews = new List<School>(),
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers, Mentors and Students can view schools"
                    };
                }

                    return new SchoolsViewResponse()
                    {
                        SchoolsViews = schools,
                        Result = "Success",
                        ErrorMessage = "Schools shown"
                    };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewSchools Service error: " + ex.Message + " UserId: " + view.UserId);

                return new SchoolsViewResponse()
                {
                    SchoolsViews = new List<School>(),
                    Result = "Failure",
                    ErrorMessage = "ViewSchools Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<SchoolPropsResponse>> ViewSingleSchoolProps(SchoolsViewRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine((int)request.UserId) == false)
                {
                    _logger.LogInformation("ViewSingleSchoolProps failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.UserId);

                    return new SchoolPropsResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsSchoolWithGivenIdExists((int)request.SchoolId) == false)
                {
                    _logger.LogInformation("ViewSingleSchoolProps failed, School with given id not found - AdminUserId: " + request.UserId + " SchoolId: " + request.SchoolId);

                    return new SchoolPropsResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                DBSchool dbschool = _context.DBSchools.SingleOrDefault(s => s.Id == request.SchoolId);


                return new SchoolPropsResponse()
                {
                    Id = dbschool.Id,
                    Name = dbschool.Name,
                    City = dbschool.City,
                    Country = dbschool.Country,
                    GradingSystemInt = (int)dbschool.GradingSystem,
                    Result = "Success",
                    ErrorMessage = "School props shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewSingleSchoolProps Service error: " + ex.Message + " UserId: " + request.UserId);

                return new SchoolPropsResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "ViewSingleSchoolProps Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<UsersViewResponse>> ViewUsersAssignedOrNotToSchool(UsersViewRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine((int)request.AdminUserId) == false)
                {
                    _logger.LogInformation("ViewUsersFromSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.AdminUserId);

                    return new UsersViewResponse()
                    {
                        UsersViews = new List<User>(),
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsSchoolWithGivenIdExists((int)request.SchoolId) == false)
                {
                    _logger.LogInformation("ViewUsersFromSchool failed, School with given id not found - AdminUserId: " + request.AdminUserId + " CourseId: " + request.SchoolId);

                    return new UsersViewResponse()
                    {
                        UsersViews = new List<User>(),
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (IsUserAssignedToSchool((int)request.SchoolId, (int)request.AdminUserId) == false)
                {
                    _logger.LogInformation("ViewUsersFromSchool failed, Provided user is not assigned to desired school - AdminUserId: " + request.AdminUserId + " SchoolId: " + request.SchoolId);

                    return new UsersViewResponse()
                    {
                        UsersViews = new List<User>(),
                        Result = "Failure",
                        ErrorMessage = "Provided user is not assigned to desired school"
                    };
                }

                if (_validators.IsUserAdmin((int)request.AdminUserId) == false && _validators.IsUserSchoolManager((int)request.AdminUserId) == false
                        && _validators.IsUserMentor((int)request.AdminUserId) == false)
                {
                    _logger.LogInformation("ViewUsersFromSchool failed, Only Admins, School Managers and Mentors can view users assigned to school - AdminUserId: " + request.AdminUserId + " SchoolId: " + request.SchoolId);

                    return new UsersViewResponse()
                    {
                        UsersViews = new List<User>(),
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers and Mentors can view users assigned to school"
                    };
                }

                List<DBSchoolUser> schoolUsers = _context.DBSchoolUsers.Where(s => s.SchoolId == request.SchoolId).ToList();

                List<int> userIds = schoolUsers.Select(x => x.UserId).ToList();

                List<DBUser> dbusers = request.ViewAssigned
                    ? _context.DBUsers.Where(u => userIds.Contains(u.Id)).ToList()
                    : _context.DBUsers.Where(u => !userIds.Contains(u.Id)).ToList();

                dbusers.RemoveAll(u => u.UserTypeId == 2);

                List<User>  users = _mapper.Map<List<User>>(dbusers);

                users.ForEach(u => u.Password = null);

                return new UsersViewResponse()
                {
                    UsersViews = users,
                    SchoolId = request.SchoolId,
                    IsShowingAssigned = request.ViewAssigned,
                    Result = "Success",
                    ErrorMessage = request.ViewAssigned ? "Users of relevant school shown" : "Users not assigned to relevant school shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewUsersFromSchool Service error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new UsersViewResponse()
                {
                    UsersViews = new List<User>(),
                    Result = "Failure",
                    ErrorMessage = "ViewUsers Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<SchoolOrCourseUsersByTypeAndCoursesCountResponse>> GetAllSchoolUsersByTypeAndCoursesCount(UsersViewRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine((int)request.AdminUserId) == false)
                {
                    _logger.LogInformation("ViewUsersFromSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.AdminUserId);

                    return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsSchoolWithGivenIdExists((int)request.SchoolId) == false)
                {
                    _logger.LogInformation("ViewUsersFromSchool failed, School with given id not found - AdminUserId: " + request.AdminUserId + " SchoolId: " + request.SchoolId);

                    return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (_validators.IsUserAdmin((int)request.AdminUserId) == false && _validators.IsUserSchoolManager((int)request.AdminUserId) == false)
                {
                    _logger.LogInformation("ViewUsersFromSchool failed, Only Admins and School Managers and Mentors can view information about school users - AdminUserId: " + request.AdminUserId + " SchoolId: " + request.SchoolId);

                    return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins and School Managers and Mentors can view information about school users"
                    };
                }

                List<DBSchoolUser> schoolUsers = _context.DBSchoolUsers.Where(s => s.SchoolId == request.SchoolId).ToList();

                List<int> userIds = schoolUsers.ConvertAll(x => x.UserId);

                List<DBUser> dbusers = _context.DBUsers.Where(u => userIds.Contains(u.Id)).ToList();

                int schoolManagersCount = dbusers.Where(u => u.UserTypeId == 3).ToList().Count;
                int mentorsCount = dbusers.Where(u => u.UserTypeId == 0).ToList().Count;
                int studentsCount = dbusers.Where(u => u.UserTypeId == 1).ToList().Count;

                List<DBCourse> dbcourses = _context.DBCourses.Where(c => c.SchoolId == request.SchoolId).ToList();

                int coursesCount = dbcourses.Count;

                return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                {
                    CoursesCount = coursesCount,
                    SchoolManagersCount = schoolManagersCount,
                    MentorsCount = mentorsCount,
                    StudentsCount = studentsCount,
                    Result = "Success",
                    ErrorMessage = "Information about count of school users and courses shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("GetAllSchoolUsersByTypeAndCoursesCount Service error: " + ex.Message + " UserId: " + request.AdminUserId);

                return new SchoolOrCourseUsersByTypeAndCoursesCountResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "GetAllSchoolUsersByTypeAndCoursesCount Service error: " + ex.Message
                };
            }
        }


        public async Task<ActionResult<SchoolActionResponse>> CreateSchool(SchoolRequest school)
        {
            try
            {
                if (_validators.IsUserGenuine((int)school.UserId) == false)
                {
                    _logger.LogInformation("CreateSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserGenuine((int)school.UserId) == false)
                {
                    _logger.LogInformation("CreateSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(school.UserId) == false)
                {
                    _logger.LogInformation("CreateSchool failed, Only user with Admin priveleges can alter Schools - AdminUserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only user with Admin priveleges can alter Schools"
                    };
                }

                if (IsSchoolNameUniquePerCountryAndCity(school.Name, school.Country, school.City) == true)
                {
                    _logger.LogInformation("CreateSchool failed, School with given name already exists in provided Country and City - AdminUserId: " + school.UserId + " School Name: " + school.Name);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given name already exists in provided Country and City"
                    };
                }

                if (school.Country == null || school.Country == "")
                {
                    _logger.LogInformation("CreateSchool failed, Country field can't be empty - AdminUserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Country field can't be empty"
                    };
                }

                if (school.City == null || school.City == "")
                {
                    _logger.LogInformation("CreateSchool failed, City field can't be empty - AdminUserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "City field can't be empty"
                    };
                }

                DBSchool newSchool = new DBSchool()
                {
                    Name = school.Name,
                    CreatedDate= DateTime.UtcNow,
                    ModifiedDate= DateTime.UtcNow,
                    City = school.City,
                    Country = school.Country,
                    GradingSystem = (GradingSystem) school.GradingSystem
                };

                _context.DBSchools.Add(newSchool);
                _context.SaveChanges();

                _logger.LogInformation("School created successfully - UserId: " + school.UserId + " SchoolId: " + newSchool.Id);

                return new SchoolActionResponse()
                {
                    SchoolResultId = newSchool.Id,
                    Result = "Success",
                    ErrorMessage = "School created successfully"                    
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateSchool Service error: " + ex.Message + " UserId: " + school.UserId);

                return new SchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "CreateSchool Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<SchoolActionResponse>> UpdateSchool(SchoolRequest school)
        {
            try
            {
                if (_validators.IsUserGenuine((int)school.UserId) == false)
                {
                    _logger.LogInformation("UpdateSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserGenuine((int)school.UserId) == false)
                {
                    _logger.LogInformation("UpdateSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(school.UserId) == false)
                {
                    _logger.LogInformation("UpdateSchool failed, Only user with Admin priveleges can alter Schools - AdminUserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only user with Admin priveleges can alter Schools"
                    };
                }

                if (IsUserAssignedToSchool((int)school.SchoolId, (int)school.UserId) == false)
                {
                    _logger.LogInformation("UpdateSchool failed, Provided user is not assigned to desired school - AdminUserId: " + school.UserId + " SchoolId: " + school.SchoolId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided user is not assigned to desired school"
                    };
                }

                if (IsSchoolWithGivenIdExists(school.SchoolId) == false)
                {
                    _logger.LogInformation("UpdateSchool failed, School with given id not found - AdminUserId: " + school.UserId + " SchoolId: " + school.SchoolId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (IsSchoolNameUniquePerCountryAndCity(school.Name, school.Country, school.City, school.SchoolId) == true)
                {
                    _logger.LogInformation("CreateSchool failed, School with given name already exists in provided Country and City - AdminUserId: " + school.UserId + " School Name: " + school.Name);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given name already exists in provided Country and City"
                    };
                }

                if (school.Country == null || school.Country == "")
                {
                    _logger.LogInformation("CreateSchool failed, Country field can't be empty - AdminUserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Country field can't be empty"
                    };
                }

                if (school.City == null || school.City == "")
                {
                    _logger.LogInformation("CreateSchool failed, City field can't be empty - AdminUserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "City field can't be empty"
                    };
                }

                DBSchool updatedSchool = _context.DBSchools.Where(s => s.Id == school.SchoolId).SingleOrDefault();

                updatedSchool.Name = school.Name;
                updatedSchool.ModifiedDate= DateTime.UtcNow;
                updatedSchool.Country = school.Country;
                updatedSchool.City = school.City;
                updatedSchool.GradingSystem = (GradingSystem) school.GradingSystem;

                _context.SaveChanges();

                _logger.LogInformation("School updated successfully - UserId: " + school.UserId + " CourseId: " + school.SchoolId);

                return new SchoolActionResponse()
                {
                    SchoolResultId = updatedSchool.Id,
                    Result = "Success",
                    ErrorMessage = "School updated successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateSchool Service error: " + ex.Message + " UserId: " + school.UserId + " CourseId: " + school.SchoolId);

                return new SchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateSchool Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<ActionAllowedResponse>> IsDeleteSchoolAllowed(SchoolRequest school)
        {
            try
            {
                if (_validators.IsUserGenuine((int)school.UserId) == false)
                {
                    _logger.LogInformation("IsDeleteSchoolAllowed failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + school.UserId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsSchoolHasNoCourses(school.SchoolId) == false)
                {                    
                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "School can't be deleted, it has courses - delete those courses first"
                    };
                }

                return new ActionAllowedResponse()
                {
                    IsAllowed = true                    
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("IsDeleteSchoolAllowed Service error: " + ex.Message + " UserId: " + school.UserId + " SchoolId: " + school.SchoolId);

                return new ActionAllowedResponse()
                {
                    IsAllowed = false,
                    Message = "IsDeleteSchoolAllowed Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<SchoolActionResponse>> DeleteSchool(SchoolRequest school)
        {
            try
            {
                if (_validators.IsUserGenuine((int)school.UserId) == false)
                {
                    _logger.LogInformation("DeleteSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserGenuine((int)school.UserId) == false)
                {
                    _logger.LogInformation("DeleteSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(school.UserId) == false)
                {
                    _logger.LogInformation("DeleteSchool failed, Only user with Admin priveleges can alter Schools - AdminUserId: " + school.UserId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only user with Admin priveleges can alter Schools"
                    };
                }

                if (IsUserAssignedToSchool((int)school.SchoolId, (int)school.UserId) == false)
                {
                    _logger.LogInformation("DeleteSchool failed, Provided user is not assigned to desired school - AdminUserId: " + school.UserId + " SchoolId: " + school.SchoolId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided user is not assigned to desired school"
                    };
                }

                if (IsSchoolWithGivenIdExists(school.SchoolId) == false)
                {
                    _logger.LogInformation("DeleteSchool failed, School with given id not found - AdminUserId: " + school.UserId + " SchoolId: " + school.SchoolId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                DBSchool updatedSchool = _context.DBSchools.SingleOrDefault(s => s.Id == school.SchoolId);

                if (IsSchoolHasNoCourses(school.SchoolId) == false)
                {
                    _logger.LogInformation("DeleteSchool failed, School can't be deleted, it has courses - delete those courses first: " + school.UserId + " SchoolId: " + updatedSchool.Id);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School can't be deleted, it has courses - delete those courses first"
                    };
                }

                _context.DBSchools.Remove(updatedSchool);

                _context.SaveChanges();

                bool unAssign = UnAssignAllUsersFromSchool(school.UserId, school.SchoolId);

                if (unAssign == false)
                {
                    _logger.LogInformation("DeleteSchool failed, Failed to unassign all users from deleted school - AdminUserId: " + school.UserId + " SchoolId: " + school.SchoolId);

                    return new SchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Failed to unassign all users from deleted school"
                    };
                }

                _logger.LogInformation("School deleted successfully - UserId: " + school.UserId + " CourseId: " + school.SchoolId);

                return new SchoolActionResponse()
                {
                    Result = "Success",
                    ErrorMessage = "School deleted successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("DeleteSchool Service error: " + ex.Message + " UserId: " + school.UserId + " CourseId: " + school.SchoolId);

                return new SchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "DeleteSchool Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<ActionAllowedResponse>> IsAssignOrUnassignUserToSchoolAllowed(AssignUserToSchoolRequest request)
        {
            try
            {
                await _validators.CleanupDuplicateLinksAsync("school");

                if (_validators.IsUserGenuine((int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToSchoolAllowed failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.ManagerUserId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(request.ManagerUserId) == false && _validators.IsUserSchoolManager(request.AddedUserId) == true)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToSchoolAllowed failed, Only Admins can assign School Managers to School - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " SchoolId: " + request.SchoolId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Only Admins can assign School Managers to School"
                    };
                }

                if (_validators.IsUserSchoolManager(request.ManagerUserId) == true &&
                    _validators.IsUserMentor(request.AddedUserId) == false && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToSchoolAllowed failed, School Managers can only assign Mentors and Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " SchoolId: " + request.SchoolId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "School Managers can only assign Mentors and Students"
                    };
                }

                if (_validators.IsUserMentor(request.ManagerUserId) == true && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToSchoolAllowed failed, Mentors can only assign Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " SchoolId: " + request.SchoolId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Mentors can only assign Students"
                    };
                }

                if (IsSchoolHasNoCoursesOfMentorOrStudent(request.SchoolId, request.AddedUserId, 0) == false && request.IsAssign == false)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToSchoolAllowed failed, Can't unassign Mentor from school that has courses - delete those courses first - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " SchoolId: " + request.SchoolId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Can't unassign Mentor from school that has courses - delete those courses first"
                    };
                }

                if (IsSchoolHasNoCoursesOfMentorOrStudent(request.SchoolId, request.AddedUserId, 1) == false && request.IsAssign == false)
                {
                    _logger.LogInformation("IsAssignOrUnassignUserToSchoolAllowed failed, Can't unassign Student from school if he has assigned Courses in this School - unassign this student from those Courses first - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " SchoolId: " + request.SchoolId);

                    return new ActionAllowedResponse()
                    {
                        IsAllowed = false,
                        Message = "Can't unassign Student from school if he has assigned Courses in this School - unassign this student from those Courses first"
                    };
                }

                return new ActionAllowedResponse()
                {
                    IsAllowed = true
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("IsAssignOrUnassignUserToSchoolAllowed Service error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " SchoolId: " + request.SchoolId);

                return new ActionAllowedResponse()
                {
                    IsAllowed = false,
                    Message = "IsAssignOrUnassignUserToSchoolAllowed Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<AssignUserToSchoolActionResponse>> AssignUserToSchool(AssignUserToSchoolRequest request)
        {
            await _validators.CleanupDuplicateLinksAsync("school");

            try
            {
                if (_validators.IsUserGenuine((int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("AssignUserToSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.ManagerUserId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsSchoolWithGivenIdExists(request.SchoolId) == false)
                {
                    _logger.LogInformation("AssignUserToSchool failed, School with given id not found - AdminUserId: " + request.ManagerUserId + " SchoolId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (IsUserAssignedToSchool((int)request.SchoolId, (int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("AssignUserToSchool failed, Provided manager user is not assigned to desired school - AdminUserId: " + request.ManagerUserId + " SchoolId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided manager user is not assigned to desired school"
                    };
                }

                if (_validators.IsUserStudent(request.AddedUserId) == false && _validators.IsUserMentor(request.AddedUserId) == false
                    && _validators.IsUserSchoolManager(request.AddedUserId) == false)
                {
                    _logger.LogInformation("AssignUserToSchool failed, Only Students, Mentors or School Managers can be assigned to School - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Students, Mentors or School Managers can be assigned to school"
                    };
                }

                if (_validators.IsUserAdmin(request.ManagerUserId) == false && _validators.IsUserSchoolManager(request.AddedUserId) == true)
                {
                    _logger.LogInformation("AssignUserToSchool failed, Only Admins can assign School Managers to School - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins can assign School Managers to School"
                    };
                }

                if (_validators.IsUserSchoolManager(request.ManagerUserId) == true &&
                    _validators.IsUserMentor(request.AddedUserId) == false && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("AssignUserToCourse failed, School Managers can only assign Mentors and Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School Managers can only assign Mentors and Students"
                    };
                }

                if (_validators.IsUserMentor(request.ManagerUserId) == true && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("AssignUserToSchool failed, Mentors can only assign Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentors can only assign Students"
                    };
                }

                DBSchoolUser schoolUser = new DBSchoolUser()
                {
                    SchoolId = request.SchoolId,
                    UserId = request.AddedUserId,
                    CreatedDate= DateTime.UtcNow,
                    ModifiedDate= DateTime.UtcNow,
                };

                _context.DBSchoolUsers.Add(schoolUser);
                _context.SaveChanges();

                _logger.LogInformation("User assigned to course successfully - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                return new AssignUserToSchoolActionResponse()
                {
                    SchoolId = request.SchoolId,
                    ManagerUserId = request.ManagerUserId,
                    AddedUserId = request.AddedUserId,
                    Result = "Success",
                    ErrorMessage = "User assigned to school successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("AssignUserToSchool Service error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                return new AssignUserToSchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "AssignUserToSchool Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<AssignUserToSchoolActionResponse>> UnAssignUserFromSchool(AssignUserToSchoolRequest request)
        {
            try
            {
                await _validators.CleanupDuplicateLinksAsync("school");

                if (_validators.IsUserGenuine((int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.ManagerUserId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserGenuine((int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.ManagerUserId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsSchoolWithGivenIdExists(request.SchoolId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed, School with given id not found - AdminUserId: " + request.ManagerUserId + " SchoolId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (IsUserAssignedToSchool((int)request.SchoolId, (int)request.ManagerUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed, Provided manager user is not assigned to desired school - AdminUserId: " + request.ManagerUserId + " SchoolId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided manager user is not assigned to desired school"
                    };
                }

                if (_validators.IsUserStudent(request.AddedUserId) == false && _validators.IsUserMentor(request.AddedUserId) == false
                    && _validators.IsUserSchoolManager(request.AddedUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed, Only Students, Mentors or School Managers can be unassigned from School - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Students, Mentors or School Managers can be unassigned from school"
                    };
                }

                if (_validators.IsUserAdmin(request.ManagerUserId) == false && _validators.IsUserSchoolManager(request.AddedUserId) == true)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed, Only Admins can unassign School Managers from School - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins can unassign School Managers to School"
                    };
                }

                if (_validators.IsUserSchoolManager(request.ManagerUserId) == true &&
                    _validators.IsUserMentor(request.AddedUserId) == false && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed, School Managers can only unassign Mentors and Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School Managers can only unassign Mentors and Students"
                    };
                }

                if (_validators.IsUserMentor(request.ManagerUserId) == true && _validators.IsUserStudent(request.AddedUserId) == false)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed, Mentors can only unassign Students - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentors can only unassign Students"
                    };
                }

                if (IsSchoolHasNoCoursesOfMentorOrStudent(request.SchoolId, request.AddedUserId, 0) == false)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed, Can't unassign Mentor from school that has courses - delete those courses first - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " SchoolId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Can't unassign Mentor from school that has courses - delete those courses first"
                    };
                }

                if (IsSchoolHasNoCoursesOfMentorOrStudent(request.SchoolId, request.AddedUserId, 1) == false && request.IsAssign == false)
                {
                    _logger.LogInformation("UnAssignUserFromSchool failed, Can't unassign Student from school if he has assigned Courses in this School - unassign this student from those Courses first - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " SchoolId: " + request.SchoolId);

                    return new AssignUserToSchoolActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Can't unassign Student from school if he has assigned Courses in this School - unassign this student from those Courses first"
                    };
                }

                DBSchoolUser updatedSchoolUser = _context.DBSchoolUsers.SingleOrDefault(s => s.SchoolId == request.SchoolId && s.UserId == request.AddedUserId);
                _context.DBSchoolUsers.Remove(updatedSchoolUser);

                var connectionsToDelete = _context.DBMentorsSchoolsTopics
                    .Where(m => m.SchoolId == request.SchoolId && m.MentorId == request.AddedUserId)
                    .ToList();  

                _context.DBMentorsSchoolsTopics.RemoveRange(connectionsToDelete);

                _context.SaveChanges();

                _logger.LogInformation("User unassigned from course successfully - AdminUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                return new AssignUserToSchoolActionResponse()
                {
                    SchoolId = request.SchoolId,
                    ManagerUserId = request.ManagerUserId,
                    AddedUserId = request.AddedUserId,
                    Result = "Success",
                    ErrorMessage = "User unassigned from school successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("UnAssignUserFromSchool Service error: " + ex.Message + " ManagerUserId: " + request.ManagerUserId + " AddedUserId: " + request.AddedUserId + " CourseId: " + request.SchoolId);

                return new AssignUserToSchoolActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UnAssignUserToSchool Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<ViewMentorSchoolTopicConnectionsResponse>> ViewMentorSchoolTopicConnections(MentorSchoolTopicRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine((int)request.AdminId) == false)
                {
                    _logger.LogInformation("ViewMentorSchoolTopicConnections failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.AdminId);

                    return new ViewMentorSchoolTopicConnectionsResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsSchoolWithGivenIdExists(request.SchoolId) == false)
                {
                    _logger.LogInformation("ViewMentorSchoolTopicConnections failed, School with given id not found - AdminUserId: " + request.AdminId + " SchoolId: " + request.SchoolId);

                    return new ViewMentorSchoolTopicConnectionsResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (_validators.IsUserMentor(request.MentorId) == false)
                {
                    _logger.LogInformation("ViewMentorSchoolTopicConnections failed, Mentor with given id not found - AdminUserId: " + request.AdminId + " MentorId: " + request.MentorId);

                    return new ViewMentorSchoolTopicConnectionsResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentor with given id not found"
                    };
                }

                if (IsUserAssignedToSchool((int)request.SchoolId, (int)request.MentorId) == false)
                {
                    _logger.LogInformation("ViewMentorSchoolTopicConnections failed, Provided mentor user is not assigned to desired school - AdminUserId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId);

                    return new ViewMentorSchoolTopicConnectionsResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided mentor user is not assigned to desired school"
                    };
                }

                if (_validators.IsUserAdmin(request.AdminId) == false && _validators.IsUserSchoolManager(request.AdminId) == false
                    && _validators.IsUserMentor(request.AdminId) == false)
                {
                    _logger.LogInformation("ViewMentorSchoolTopicConnections failed, Only Admins, SchoolManagers and Mentors can view Mentor-School-Topic connections - AdminUserId: " + request.AdminId);

                    return new ViewMentorSchoolTopicConnectionsResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins, SchoolManagers and Mentors can view Mentor-School-Topic connections"
                    };
                }

                List<DBMentorSchoolTopic> connections = _context.DBMentorsSchoolsTopics.Where(m => m.MentorId == request.MentorId && m.SchoolId == request.SchoolId).ToList();
                List<int> topicIds = connections.ConvertAll(c => c.TopicId);

                List<DBTopic> assignedTopics = _context.DBTopics.Where(t => topicIds.Contains(t.Id)).ToList();
                List<DBTopic> unassignedTopics = _context.DBTopics.Where(t => !topicIds.Contains(t.Id)).ToList();

                return new ViewMentorSchoolTopicConnectionsResponse()
                {
                    MentorId = request.MentorId,
                    SchoolId = request.SchoolId,
                    AssignedTopics = _mapper.Map<List<Topic>>(assignedTopics),
                    NotAssignedTopics = _mapper.Map<List<Topic>>(unassignedTopics),
                    Result = "Success",
                    ErrorMessage = "Connections Mentor-School-Topic shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewMentorSchoolTopicConnections Service error: " + ex.Message + " AdminId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId);

                return new ViewMentorSchoolTopicConnectionsResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "ViewMentorSchoolTopicConnections Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<AssignMentorSchoolTopicResponse>> AssignMentorSchoolTopic(MentorSchoolTopicRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine((int)request.AdminId) == false)
                {
                    _logger.LogInformation("AssignMentorSchoolTopic failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.AdminId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsSchoolWithGivenIdExists(request.SchoolId) == false)
                {
                    _logger.LogInformation("AssignMentorSchoolTopic failed, School with given id not found - AdminUserId: " + request.AdminId + " SchoolId: " + request.SchoolId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (_validators.IsUserMentor(request.MentorId) == false)
                {
                    _logger.LogInformation("AssignMentorSchoolTopic failed, Mentor with given id not found - AdminUserId: " + request.AdminId + " MentorId: " + request.MentorId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentor with given id not found"
                    };
                }

                if (IsUserAssignedToSchool((int)request.SchoolId, (int)request.MentorId) == false)
                {
                    _logger.LogInformation("AssignMentorSchoolTopic failed, Provided mentor user is not assigned to desired school - AdminUserId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided mentor user is not assigned to desired school"
                    };
                }

                if (IsTopicWithGivenIdExists(request.TopicId) == false)
                {
                    _logger.LogInformation("AssignMentorSchoolTopic failed, Topic with given id not found - AdminUserId: " + request.AdminId + " TopicId: " + request.TopicId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Topic with given id not found"
                    };
                }

                if (_validators.IsUserAdmin(request.AdminId) == false && _validators.IsUserSchoolManager(request.AdminId) == false)
                {
                    _logger.LogInformation("AssignMentorSchoolTopic failed, Only Admins and SchoolManagers can assign Topics to Mentors - AdminUserId: " + request.AdminId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins and SchoolManagers can assign Topics to Mentors"
                    };
                }

                if (_validators.IsUserMentor(request.MentorId) == false)
                {
                    _logger.LogInformation("AssignMentorSchoolTopic failed, Selected user is not Mentor - AdminUserId: " + request.AdminId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Selected user is not Mentor"
                    };
                }

                if (IsMentorSchoolTopicConnectionExists(request.SchoolId, request.MentorId, request.TopicId) == true)
                {
                    _logger.LogInformation("AssignMentorSchoolTopic failed, Existing connection Mentor-School-Topic already exists - AdminUserId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId + " TopicId: " + request.TopicId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Existing connection Mentor-School-Topic already exists"
                    };
                }

                DBMentorSchoolTopic mentorSchoolTopic = new DBMentorSchoolTopic()
                {
                    MentorId = request.MentorId,
                    SchoolId = request.SchoolId,
                    TopicId = request.TopicId,
                    CreatedDate= DateTime.UtcNow,
                    ModifiedDate= DateTime.UtcNow,
                };

                _context.DBMentorsSchoolsTopics.Add(mentorSchoolTopic);
                _context.SaveChanges();

                _logger.LogInformation("Topic assigned to mentor by school successfully - AdminUserId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId + " TopicId: "+ request.TopicId);

                return new AssignMentorSchoolTopicResponse()
                {
                    MentorId = request.MentorId,
                    SchoolId = request.SchoolId,
                    TopicId = request.TopicId,
                    Result = "Success",
                    ErrorMessage = "Topic assigned to mentor by school successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("AssignMentorSchoolTopic Service error: " + ex.Message + " AdminId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId + " TopicId: " + request.TopicId);

                return new AssignMentorSchoolTopicResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "AssignMentorSchoolTopic Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<AssignMentorSchoolTopicResponse>> UnAssignMentorSchoolTopic(MentorSchoolTopicRequest request)
        {
            try
            {
                if (_validators.IsUserGenuine((int)request.AdminId) == false)
                {
                    _logger.LogInformation("UnAssignMentorSchoolTopic failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + request.AdminId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (IsSchoolWithGivenIdExists(request.SchoolId) == false)
                {
                    _logger.LogInformation("UnAssignMentorSchoolTopic failed, School with given id not found - AdminUserId: " + request.AdminId + " SchoolId: " + request.SchoolId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "School with given id not found"
                    };
                }

                if (_validators.IsUserMentor(request.MentorId) == false)
                {
                    _logger.LogInformation("UnAssignMentorSchoolTopic failed, Mentor with given id not found - AdminUserId: " + request.AdminId + " MentorId: " + request.MentorId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Mentor with given id not found"
                    };
                }

                if (IsUserAssignedToSchool((int)request.SchoolId, (int)request.MentorId) == false)
                {
                    _logger.LogInformation("UnAssignMentorSchoolTopic failed, Provided mentor user is not assigned to desired school - AdminUserId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Provided mentor user is not assigned to desired school"
                    };
                }

                if (IsTopicWithGivenIdExists(request.TopicId) == false)
                {
                    _logger.LogInformation("UnAssignMentorSchoolTopic failed, Topic with given id not found - AdminUserId: " + request.AdminId + " TopicId: " + request.TopicId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Topic with given id not found"
                    };
                }

                if (_validators.IsUserAdmin(request.AdminId) == false && _validators.IsUserSchoolManager(request.AdminId) == false)
                {
                    _logger.LogInformation("UnAssignMentorSchoolTopic failed, Only Admins and SchoolManagers can unassign Topics from Mentors - AdminUserId: " + request.AdminId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins and SchoolManagers can unassign Topics from Mentors"
                    };
                }

                if (_validators.IsUserMentor(request.MentorId) == false)
                {
                    _logger.LogInformation("UnAssignMentorSchoolTopic failed, Selected user is not Mentor - AdminUserId: " + request.AdminId);

                    return new AssignMentorSchoolTopicResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Selected user is not Mentor"
                    };
                }

                DBMentorSchoolTopic updatedMentorSchoolTopic = _context.DBMentorsSchoolsTopics.SingleOrDefault(s => s.MentorId == request.MentorId && s.SchoolId == request.SchoolId && s.TopicId == request.TopicId);
                _context.DBMentorsSchoolsTopics.Remove(updatedMentorSchoolTopic);

                _context.SaveChanges();


                _logger.LogInformation("Topic unassigned from mentor by school successfully - AdminUserId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId + " TopicId: " + request.TopicId);

                return new AssignMentorSchoolTopicResponse()
                {
                    MentorId = request.MentorId,
                    SchoolId = request.SchoolId,
                    TopicId = request.TopicId,
                    Result = "Success",
                    ErrorMessage = "Topic unassigned from mentor by school successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("UnAssignMentorSchoolTopic Service error: " + ex.Message + " AdminId: " + request.AdminId + " MentorId: " + request.MentorId + " SchoolId: " + request.SchoolId + " TopicId: " + request.TopicId);

                return new AssignMentorSchoolTopicResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UnAssignMentorSchoolTopic Service error: " + ex.Message
                };
            }
        }

        private bool UnAssignAllUsersFromSchool(int userId, int schoolId)
        {
            try
            {
                List<DBSchoolUser> updatedSchoolUsers = _context.DBSchoolUsers.Where(s => s.SchoolId == schoolId).ToList();

                foreach (DBSchoolUser updatedSchoolUser in updatedSchoolUsers)
                {
                    _context.DBSchoolUsers.Remove(updatedSchoolUser);

                    var connectionsToDelete = _context.DBMentorsSchoolsTopics
                        .Where(m => m.SchoolId == schoolId && m.MentorId == userId)
                        .ToList();

                    _context.DBMentorsSchoolsTopics.RemoveRange(connectionsToDelete);

                    _context.SaveChanges();
                }

                return true;
            }

            catch (Exception ex)
            {
                _logger.LogError("UnAssignAllUsersFromSchool failed, Error: " + ex.Message + " UserId: " + userId);

                return false;
            }
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

        private bool IsUserAssignedToSchool(int schoolId, int userId)
        {
            if (_validators.IsUserAdmin(userId))
                return true;

            List<DBSchoolUser> schoolUsers = _context.DBSchoolUsers.Where(s => s.SchoolId == schoolId && s.UserId == userId).ToList();

            return schoolUsers.Count == 1;
        }

        private bool IsMentorSchoolTopicConnectionExists(int schoolId, int mentorId, int topicId)
        {
            List<DBMentorSchoolTopic> connections = _context.DBMentorsSchoolsTopics.Where(m => m.SchoolId == schoolId && m.MentorId == mentorId && m.TopicId == topicId).ToList();

            return connections.Count > 0;
        }

        private bool IsSchoolNameUniquePerCountryAndCity(string schoolName, string country, string city, int existingId = 0)
        {
            List<DBSchool> schools = _context.DBSchools.Where(s => s.Name == schoolName
                && s.Country == country && s.City == city).ToList();

            if (existingId != 0)
                schools = schools.Where(s => s.Id != existingId).ToList();

            return schools.Count == 1;
        }

        private bool IsSchoolHasNoCourses(int schoolId)
        {
            List<DBCourse> courses = _context.DBCourses.Where(c => c.SchoolId == schoolId).ToList();

            return courses.Count == 0;
        }

        private bool IsSchoolHasNoCoursesOfMentorOrStudent(int schoolId, int mentorId, int userTypeId)
        {
            List<DBUser> users = _context.DBUsers.Where(u => u.Id == mentorId && u.UserTypeId == userTypeId).ToList();

            if (users.Count != 1)
                return true;

            List<DBCourse> courses = _context
                .DBCourses
                .Where(c => c.SchoolId == schoolId).ToList();

            List<int> courseIds = courses.ConvertAll(c => c.Id);            

            List<DBCourseUser> courseUsers = _context.DBCourseUsers.Where(c => c.UserId == mentorId && courseIds.Contains(c.CourseId)).ToList();            

            return courseUsers.Count == 0;
        }
    }
}
