using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Mvc;
using static MentorsAndStudents.UsersController;

namespace MentorsAndStudents
{
    public interface ITasksService
    {
        Task<ActionResult<ContentActionResponse>> UploadContent(ContentRequest task);
        Task<ActionResult<ContentViewResponse>> ViewContents(ContentsViewRequest view);
        Task<ActionResult<ContentActionResponse>> UpdateContent(ContentRequest task);
        Task<FileStreamResult> DownloadContent(ContentRequest task);
        Task<ActionResult<ContentActionResponse>> DeleteContent(DeleteContentRequest task);
    }
}
