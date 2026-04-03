using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MentorsAndStudents
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class ContentController
    {
        private readonly ITasksService _tasksService;
        private readonly ILogger<ContentController> _logger;

        public ContentController(ITasksService tasksService, ILogger<ContentController> logger)
        {
            _tasksService = tasksService;
            _logger = logger;
        }

        [HttpPost("view_contents")]
        public async Task<ActionResult<ContentViewResponse>> ViewContents(ContentsViewRequest view)
        {
            try
            {
                return await _tasksService.ViewContents(view);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewContents Controller error: " + ex.Message + " UserId: " + view.MentorId);

                return new ContentViewResponse()
                {
                    TasksViews = new List<TaskView>(),
                    Result = "Failure",
                    ErrorMessage = "ViewContents Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("upload_content")]
        public async Task<ActionResult<ContentActionResponse>> UploadContent(ContentRequest task)
        {
            try
            {
                return await _tasksService.UploadContent(task);
            }
            catch (Exception ex)
            {
                _logger.LogError("UploadContent Controller error: " + ex.Message + " UserId: " + task.MentorId);

                return new ContentActionResponse()
                {
                    FileName = "Error",
                    Result = "Failure",
                    ErrorMessage = "UploadContent Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("update_content")]
        public async Task<ActionResult<ContentActionResponse>> UpdateContent(ContentRequest task)
        {
            try
            {
                return await _tasksService.UpdateContent(task);
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateContent Controller error: " + ex.Message + " UserId: " + task.MentorId);

                return new ContentActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateContent Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("download_content")]
        public async Task<FileStreamResult> DownloadContent([FromBody] ContentRequest task)
        {
            try
            {
                return await _tasksService.DownloadContent(task);
            }

            catch (Exception ex)
            {
                _logger.LogError("DownloadContent Controller error: " + ex.Message + " UserId: " + task.MentorId);

                throw new Exception("DownloadContent Controller exception");
            }
        }

        [HttpPost("delete_content")]
        public async Task<ActionResult<ContentActionResponse>> DeleteContent(DeleteContentRequest task)
        {
            try
            {
                return await _tasksService.DeleteContent(task);
            }

            catch (Exception ex)
            {
                _logger.LogError("DeleteContent Controller error: " + ex.Message + " UserId: " + task.MentorId);

                return new ContentActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "DeleteContent Controller error: " + ex.Message
                };
            }
        }
    }
}
