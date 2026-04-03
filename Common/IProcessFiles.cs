using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public interface IProcessFiles
    {
        Task<bool> UploadFile(string fileName, IFormFile File, string directory);
        Task<FileStreamResult> DownloadFile(string fileName, string directory);
        Task<bool> DeleteFile(string fileName, string directory);
    }
}
