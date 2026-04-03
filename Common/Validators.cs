using AutoMapper;
using MentorsAndStudents.Common;
using MentorsAndStudents.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MentorsAndStudents
{
    public class Validators: IValidators
    {
        private readonly MentorsAndStudentsContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly HashSet<string> CommonPasswords = new()
        {
            "12345678", "123456789", "123123123", "abcdefgh", "password", "qwertyui",
            "iloveyou", "letmein1", "welcome1", "football", "baseball", "dragon12",
            "monkey12", "admin123", "password1", "qwerty123", "asdfghjk", "11111111",
            "00000000", "abc12345"
        };

        public Validators(MentorsAndStudentsContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsUserMentor(int userId)
        {
            if (IsUserIdUnique(userId) == false)
                return false;

            DBUser dbUser = _context.DBUsers.Where(u => u.Id == userId).ToList()[0];
            return dbUser.UserTypeId == 0;
        }

        public bool IsUserStudent(int userId)
        {
            if (IsUserIdUnique(userId) == false)
                return false;

            DBUser dbUser = _context.DBUsers.Where(u => u.Id == userId).ToList()[0];
            return dbUser.UserTypeId == 1;
        }

        public bool IsUserAdmin(int userId)
        {
            if (IsUserIdUnique(userId) == false)
                return false;

            DBUser dbUser = _context.DBUsers.Where(u => u.Id == userId).ToList()[0];
            return dbUser.UserTypeId == 2;
        }

        public bool IsUserSchoolManager(int userId)
        {
            if (IsUserIdUnique(userId) == false)
                return false;

            DBUser dbUser = _context.DBUsers.Where(u => u.Id == userId).ToList()[0];
            return dbUser.UserTypeId == 3;
        }

        public bool IsUserGenuine(int userId)
        {
            if (_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                string storedUserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return storedUserId == userId.ToString();
            }

            return false;
        }

        public string? ValidatePassword(string password, string email, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password is required.";

            if (password.Length < 8)
                return "Password must be at least 8 characters long.";

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return "Password must contain at least one uppercase letter.";

            if (!Regex.IsMatch(password, @"[a-z]"))
                return "Password must contain at least one lowercase letter.";

            if (!Regex.IsMatch(password, @"[0-9]"))
                return "Password must contain at least one digit.";

            if (!Regex.IsMatch(password, @"[\W_]")) // спецсимвол или подчёркивание
                return "Password must contain at least one special character.";

            // Только латиница
            if (!Regex.IsMatch(password, @"^[\x00-\x7F]+$"))
                return "Password must contain only Latin characters.";

            string lower = password.ToLower();

            // Точное совпадение со слабым паролем
            if (CommonPasswords.Contains(lower))
                return "Password is too common.";

            // Частичное вхождение слабых паролей
            if (CommonPasswords.Any(p => lower.Contains(p.ToLower())))
                return "Password contains parts of a common password.";

            // Проверка на совпадение с личной информацией
            if (!string.IsNullOrEmpty(email) && lower.Contains(email.Split('@')[0].ToLower()))
                return "Password should not contain your email username.";

            if (!string.IsNullOrEmpty(firstName) && lower.Contains(firstName.ToLower()))
                return "Password should not contain your first name.";

            if (!string.IsNullOrEmpty(lastName) && lower.Contains(lastName.ToLower()))
                return "Password should not contain your last name.";

            return null; // valid
        }

        public bool IsValidGrade(string grade, GradingSystem gradingSystem, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(grade) || grade == "Not set")
            {
                error = "Grade is not set";
                return false;
            }

            bool isNumeric = int.TryParse(grade, out _);
            bool shouldBeNumeric = NumericGradingSystems.Contains(gradingSystem);

            if (shouldBeNumeric && !isNumeric)
            {
                error = "Grade must be a number";
                return false;
            }

            if (!shouldBeNumeric && isNumeric)
            {
                error = "Grade must be a letter or descriptive value";
                return false;
            }

            return true;
        }

        public readonly HashSet<GradingSystem> NumericGradingSystems = new()
        {
            GradingSystem.OneToFive,
            GradingSystem.OneToSix,
            GradingSystem.OneToTen,
            GradingSystem.ZeroToHundred
        };

        public async Task CleanupDuplicateLinksAsync(string tableType)
        {
            if (tableType == "school")
            {
                var allLinks = await _context.DBSchoolUsers.ToListAsync();

                var duplicates = allLinks
                    .GroupBy(x => new { x.UserId, x.SchoolId })
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g.OrderByDescending(x => x.Id).Skip(1))
                    .ToList();

                if (duplicates.Any())
                {
                    _context.DBSchoolUsers.RemoveRange(duplicates);
                    await _context.SaveChangesAsync();
                }
            }
            else if (tableType == "course")
            {
                var allLinks = await _context.DBCourseUsers.ToListAsync();

                var duplicates = allLinks
                    .GroupBy(x => new { x.UserId, x.CourseId })
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g.OrderByDescending(x => x.Id).Skip(1))
                    .ToList();

                if (duplicates.Any())
                {
                    _context.DBCourseUsers.RemoveRange(duplicates);
                    await _context.SaveChangesAsync();
                }
            }
        }


        private bool IsUserIdUnique(int userId)
        {
            List<DBUser> users = _context.DBUsers.Where(u => u.Id == userId).ToList();

            return users.Count == 1;
        }
    }
}
