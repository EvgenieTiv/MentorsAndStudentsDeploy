using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public interface ITopicsService
    {
        Task<ActionResult<TopicsViewResponse>> ViewTopics(TopicRequest topic);
        Task<ActionResult<TopicActionResponse>> CreateTopic(TopicRequest topic);
        Task<ActionResult<TopicActionResponse>> UpdateTopic(TopicRequest topic);
        // Task<ActionResult<TopicActionResponse>> DeleteTopic(TopicRequest topic);
    }
}
