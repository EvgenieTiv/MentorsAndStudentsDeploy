namespace MentorsAndStudents.Common
{
    public interface IValidators
    {
        bool IsUserMentor(int userId);
        bool IsUserAdmin(int userId);
        bool IsUserSchoolManager(int userId);
        bool IsUserStudent(int userId);
        bool IsUserGenuine(int userId);
        string? ValidatePassword(string password, string email, string firstName, string lastName);
        bool IsValidGrade(string grade, GradingSystem gradingSystem, out string? error);
        Task CleanupDuplicateLinksAsync(string tableType);
    }
}
