using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MentorsAndStudents
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class TopicController
    {
        private readonly ITopicsService _topicService;
        private readonly ILogger<TopicController> _logger;

        public TopicController(ITopicsService topicService, ILogger<TopicController> logger)
        {
            _topicService = topicService;
            _logger = logger;
        }

        [HttpPost("view_topics")]
        public async Task<ActionResult<TopicsViewResponse>> ViewTopics(TopicRequest topic)
        {
            try
            {
                return await _topicService.ViewTopics(topic);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewTopics Controller error: " + ex.Message);

                return new TopicsViewResponse()
                {
                    TopicsViews = new List<Topic>(),
                    Result = "Failure",
                    ErrorMessage = "ViewTopics Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("create_topic")]
        public async Task<ActionResult<TopicActionResponse>> CreateTopic(TopicRequest topic)
        {
            try
            {
                return await _topicService.CreateTopic(topic);
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateTopic Controller error: " + ex.Message + " UserId: " + topic.UserId);

                return new TopicActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "CreateTopic Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("update_topic")]
        public async Task<ActionResult<TopicActionResponse>> UpdateTopic(TopicRequest topic)
        {
            try
            {
                return await _topicService.UpdateTopic(topic);
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateTopic Controller error: " + ex.Message + " UserId: " + topic.UserId);

                return new TopicActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateTopic Controller error: " + ex.Message
                };
            }
        }

        //[HttpPost("delete_topic")]
        //public async Task<ActionResult<TopicActionResponse>> DeleteTopic(TopicRequest topic)
        //{
        //    try
        //    {
        //        return await _topicService.DeleteTopic(topic);
        //    }

        //    catch (Exception ex)
        //    {
        //        _logger.LogError("DeleteTopic Controller error: " + ex.Message + " UserId: " + topic.UserId);

        //        return new TopicActionResponse()
        //        {
        //            Result = "Failure",
        //            ErrorMessage = "DeleteTopic Controller error: " + ex.Message
        //        };
        //    }
        //}
    }
}
