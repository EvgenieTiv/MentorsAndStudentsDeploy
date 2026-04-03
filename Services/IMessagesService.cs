using MentorsAndStudents.Requests;
using Microsoft.AspNetCore.Mvc;

namespace MentorsAndStudents
{
    public interface IMessagesService
    {
        Task<ActionResult<MessagesViewResponse>> ViewMessages(ViewMessagesRequest view);
        Task<ActionResult<MessageActionResponse>> CreateMessage(MessageRequest message);
    }
}
