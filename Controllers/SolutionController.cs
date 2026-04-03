using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MentorsAndStudents.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class SolutionController
    {
        private readonly ISolutionsService _solutionsService;
        private readonly ILogger<SolutionController> _logger;

        public SolutionController(ISolutionsService solutionsService, ILogger<SolutionController> logger)
        {
            _solutionsService = solutionsService;
            _logger = logger;
        }

        [HttpPost("view_solutions")]
        public async Task<ActionResult<SolutionsViewResponse>> ViewSolutions(SolutionsViewRequest view)
        {
            try
            {
                return await _solutionsService.ViewSolutions(view);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewSolutions Controller error: " + ex.Message + " UserId: " + view.StudentId);

                return new SolutionsViewResponse()
                {
                    SolutionsViews = new List<SolutionView>(),
                    Result = "Failure",
                    ErrorMessage = "ViewSolutions Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("upload_solution")]
        public async Task<ActionResult<SolutionActionResponse>> UploadSolution(SolutionRequest solution)
        {
            try
            {
                return await _solutionsService.UploadSolution(solution);
            }

            catch (Exception ex)
            {
                _logger.LogError("UploadSolution Controller error: " + ex.Message + " UserId: " + solution.StudentId);

                return new SolutionActionResponse()
                {
                    FileName = "Error",
                    Result = "Failure",
                    ErrorMessage = "UploadSolution Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("update_solution")]
        public async Task<ActionResult<SolutionActionResponse>> UpdateSolution(SolutionRequest solution)
        {
            try
            {
                return await _solutionsService.UpdateSolution(solution);
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateSolution Controller error: " + ex.Message + " UserId: " + solution.StudentId);

                return new SolutionActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateSolution Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("download_solution")]
        public async Task<FileStreamResult> DownloadSolution([FromBody] SolutionRequest solution)
        {
            try
            {
                return await _solutionsService.DownloadSolution(solution);
            }

            catch (Exception ex)
            {
                _logger.LogError("DownloadSolution Controller error: " + ex.Message + " UserId: " + solution.StudentId);

                throw new Exception("DownloadSolution Controller exception");
            }
        }

        [HttpPost("delete_solution")]
        public async Task<ActionResult<SolutionActionResponse>> DeleteSolution(SolutionRequest solution)
        {
            try
            {
                return await _solutionsService.DeleteSolution(solution);
            }

            catch (Exception ex)
            {
                _logger.LogError("DeleteSolution Controller error: " + ex.Message + " UserId: " + solution.StudentId);

                return new SolutionActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "DeleteSolution Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("set_grade_solution")]
        public async Task<ActionResult<SolutionSetGradeResponse>> SetGradeSolution(SolutionSetGradeRequest solutionGradeModel)
        {
            try
            {
                return await _solutionsService.SetGradeSolution(solutionGradeModel);
            }

            catch (Exception ex)
            {
                _logger.LogError("SetGradeSolution Controller error: " + ex.Message + " UserId: " + solutionGradeModel.MentorId);

                return new SolutionSetGradeResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "SetGradeSolution Controller error: " + ex.Message
                };
            }
        }
    }
}
