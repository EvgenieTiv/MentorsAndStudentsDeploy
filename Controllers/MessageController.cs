using MentorsAndStudents.Requests;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MentorsAndStudents
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class MessageController
    {
        private readonly IMessagesService _messageService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMessagesService messageService, ILogger<MessageController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        [HttpPost("view_messages")]
        public async Task<ActionResult<MessagesViewResponse>> ViewMessages(ViewMessagesRequest view)
        {
            try
            {
                return await _messageService.ViewMessages(view);
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewMessages Controller error: " + ex.Message);

                return new MessagesViewResponse()
                {
                    MessagesViews = new List<MessageView>(),
                    Result = "Failure",
                    ErrorMessage = "ViewMessages Controller error: " + ex.Message
                };
            }
        }

        [HttpPost("create_message")]
        public async Task<ActionResult<MessageActionResponse>> CreateMessage(MessageRequest message)
        {
            try
            {
                return await _messageService.CreateMessage(message);
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateMessage Controller error: " + ex.Message + " UserId: " + message.UserId);

                return new MessageActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "CreateMessage Controller error: " + ex.Message
                };
            }
        }
    }
}
