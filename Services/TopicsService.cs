using AutoMapper;
using MentorsAndStudents.Common;
using MentorsAndStudents.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace MentorsAndStudents
{
    public class TopicsService : ITopicsService
    {
        private readonly MentorsAndStudentsContext _context;
        private readonly IMapper _mapper;
        private readonly IValidators _validators;
        private readonly ILogger<TopicsService> _logger;

        public TopicsService(MentorsAndStudentsContext context, IMapper mapper, IValidators validators, ILogger<TopicsService> logger)
        {
            _context = context;
            _mapper = mapper;
            _validators = validators;
            _logger = logger;
        }

        public async Task<ActionResult<TopicsViewResponse>> ViewTopics(TopicRequest topic)
        {
            try
            {
                List<DBTopic> dbtopics = _context.DBTopics.ToList();
                List<Topic> topics = _mapper.Map<List<Topic>>(dbtopics);

                if (_validators.IsUserGenuine((int)topic.UserId) == false)
                {
                    _logger.LogInformation("CreateTopic failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + topic.UserId);

                    return new TopicsViewResponse()
                    {
                        TopicsViews = new List<Topic>(),
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                return new TopicsViewResponse()
                {
                    TopicsViews = topics,
                    Result = "Success",
                    ErrorMessage = "User Types shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewTopics Service error: " + ex.Message);

                return new TopicsViewResponse()
                {
                    TopicsViews = new List<Topic>(),
                    Result = "Failure",
                    ErrorMessage = "ViewTopics Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<TopicActionResponse>> CreateTopic(TopicRequest topic)
        {
            try
            {
                if (_validators.IsUserGenuine((int)topic.UserId) == false)
                {
                    _logger.LogInformation("CreateTopic failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + topic.UserId);

                    return new TopicActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(topic.UserId) == false)
                {
                    _logger.LogInformation("CreateTopic failed - Only Admins can alter Topics: " + " UserId: " + topic.UserId);

                    return new TopicActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins can alter Topics"
                    };
                }

                if (topic.Name == "" || topic.Name == null)
                {
                    _logger.LogInformation("CreateTopic failed - No Name for the topic was provided: " + " UserId: " + topic.UserId);

                    return new TopicActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "No Name for the topic was provided"
                    };
                }                

                DBTopic newTopic = new DBTopic()
                {                    
                    Name = topic.Name,
                    CreatedDate= DateTime.UtcNow,
                    ModifiedDate= DateTime.UtcNow
                };

                _context.DBTopics.Add(newTopic);
                _context.SaveChanges();

                _logger.LogInformation("Topic created successfully" + " UserId: " + topic.UserId + " TopicId: " + newTopic.Id);

                return new TopicActionResponse()
                {
                    TopicResultId = newTopic.Id,
                    Result = "Success",
                    ErrorMessage = "Topic created successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateTopic Service error: " + ex.Message + " UserId: " + topic.UserId);

                return new TopicActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "CreateTopic Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<TopicActionResponse>> UpdateTopic(TopicRequest topic)
        {
            try
            {
                if (_validators.IsUserGenuine((int)topic.UserId) == false)
                {
                    _logger.LogInformation("UpdateTopic failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + topic.UserId);

                    return new TopicActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin(topic.UserId) == false)
                {
                    _logger.LogInformation("UpdateTopic failed - Only Admins can alter Topics: " + " UserId: " + topic.UserId);

                    return new TopicActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins can alter Topics"
                    };
                }

                if (IsTopicWithGivenIdExists(topic.TopicId) == false)
                {
                    _logger.LogInformation("UpdateTopic failed - Topic with given id not found: " + " UserId: " + topic.UserId + " TopicId: " + topic.TopicId);

                    return new TopicActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Topic with given id not found"
                    };
                }

                if (topic.Name == "" || topic.Name == null)
                {
                    _logger.LogInformation("UpdateTopic failed - No Name for the topic was provided: " + " UserId: " + topic.UserId);

                    return new TopicActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "No Name for the topic was provided"
                    };
                }

                DBTopic updatedTopic = _context.DBTopics.SingleOrDefault(t => t.Id == topic.TopicId);

                updatedTopic.Name = topic.Name;
                updatedTopic.ModifiedDate= DateTime.UtcNow;

                _context.SaveChanges();

                _logger.LogInformation("Topic updated successfully" + " UserId: " + topic.UserId + " TopicId: " + updatedTopic.Id);

                return new TopicActionResponse()
                {
                    TopicResultId = updatedTopic.Id,
                    Result = "Success",
                    ErrorMessage = "Topic updated successfully"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("UpdateTopic Service error: " + ex.Message + " UserId: " + topic.UserId);

                return new TopicActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "UpdateTopic Service error: " + ex.Message
                };
            }
        }

        //public async Task<ActionResult<TopicActionResponse>> DeleteTopic(TopicRequest topic)
        //{
        //    try
        //    {
        //        if (_validators.IsUserAdmin(topic.UserId) == false)
        //        {
        //            _logger.LogInformation("DeleteTopic failed - Only Admins can alter Topics: " + " UserId: " + topic.UserId);

        //            return new TopicActionResponse()
        //            {
        //                Result = "Failure",
        //                ErrorMessage = "Only Admins can alter Topics"
        //            };
        //        }

        //        if (IsTopicWithGivenIdExists(topic.TopicId) == false)
        //        {
        //            _logger.LogInformation("DeleteTopic failed - Topic with given id not found: " + " UserId: " + topic.UserId + " TopicId: " + topic.TopicId);

        //            return new TopicActionResponse()
        //            {
        //                Result = "Failure",
        //                ErrorMessage = "Topic with given id not found"
        //            };
        //        }

        //        DBTopic updatedTopic = _context.DBTopics.FirstOrDefault(t => t.Id == topic.TopicId);
        //        _context.DBTopics.Remove(updatedTopic);

        //        _context.SaveChanges();

        //        _logger.LogInformation("Topic updated successfully" + " UserId: " + topic.UserId);

        //        return new TopicActionResponse()
        //        {
        //            Result = "Success",
        //            ErrorMessage = "Topic deleted successfully"
        //        };
        //    }

        //    catch (Exception ex)
        //    {
        //        _logger.LogError("DeleteTopic Service error: " + ex.Message + " UserId: " + topic.UserId);

        //        return new TopicActionResponse()
        //        {
        //            Result = "Failure",
        //            ErrorMessage = "DeleteTopic Service error: " + ex.Message
        //        };
        //    }
        //}

        private bool IsTopicWithGivenIdExists(int topicId)
        {
            List<DBTopic> topics = _context.DBTopics.Where(s => s.Id == topicId).ToList();

            return topics.Count == 1;
        }
    }
}
