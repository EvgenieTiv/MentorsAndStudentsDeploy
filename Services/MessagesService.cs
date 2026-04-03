using AutoMapper;
using Azure.Core;
using MentorsAndStudents.Common;
using MentorsAndStudents.Context;
using MentorsAndStudents.Requests;
using MentorsAndStudents.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Threading.Tasks;

namespace MentorsAndStudents
{
    public class MessagesService : IMessagesService
    {
        private readonly MentorsAndStudentsContext _context;
        private readonly IValidators _validators;
        private readonly IMapper _mapper;
        private readonly ILogger<MessagesService> _logger;

        public MessagesService(MentorsAndStudentsContext context, IMapper mapper, IValidators validators, ILogger<MessagesService> logger)
        {
            _context = context;
            _validators = validators;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ActionResult<MessagesViewResponse>> ViewMessages(ViewMessagesRequest view)
        {
            try
            {
                List<DBMessage> dbmessages = new List<DBMessage>();
                List<MessageView> messagesViews = new List<MessageView>();

                if (_validators.IsUserGenuine((int)view.UserId) == false)
                {
                    _logger.LogInformation("ViewMessages failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + view.UserId);

                    return new MessagesViewResponse()
                    {
                        MessagesViews = new List<MessageView>(),
                        Result = "Failure",
                        ErrorMessage = "ViewMessages failed - Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin((int)view.UserId) == false && _validators.IsUserSchoolManager((int)view.UserId) == false
                    && _validators.IsUserMentor((int)view.UserId) == false && _validators.IsUserStudent((int)view.UserId) == false)
                {
                    _logger.LogInformation("ViewMessages failed - Only Admins, School Managers, Mentors and Students can view Messages: " + " UserId: " + view.UserId);

                    return new MessagesViewResponse()
                    {
                        MessagesViews = new List<MessageView>(),
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers, Mentors and Student can view Solutions"
                    };
                }

                if (view.ContentId != 0 && view.SolutionId != 0)
                {
                    _logger.LogInformation("ViewMessages failed - Can't search by both Task and Solution: " + " UserId: " + view.UserId);

                    return new MessagesViewResponse()
                    {
                        MessagesViews = new List<MessageView>(),
                        Result = "Failure",
                        ErrorMessage = "Can't search by both Task and Solution"
                    };
                }

                if (view.ContentId == 0 && view.SolutionId == 0)
                {
                    _logger.LogInformation("ViewMessages failed - Must provide either ContentId or SolutionId: " + " UserId: " + view.UserId);

                    return new MessagesViewResponse()
                    {
                        MessagesViews = new List<MessageView>(),
                        Result = "Failure",
                        ErrorMessage = "Must provide either ContentId or SolutionId"
                    };
                }

                if (_validators.IsUserAdmin((int)view.UserId) == true)
                {
                    if (view.ContentId != 0)
                    {
                        dbmessages = _context
                            .DBMessages
                            .Include(m => m.User)
                            .Include(m => m.Content)
                            .Include(m => m.Solution)
                            .Where(m => m.ContentId == view.ContentId).ToList();
                    }

                    else if (view.SolutionId != 0)
                    {
                        dbmessages = _context
                            .DBMessages
                            .Include(m => m.User)
                            .Include(m => m.Content)
                            .Include(m => m.Solution)
                            .Where(m => m.SolutionId == view.SolutionId).ToList();
                    }
                }

                if (_validators.IsUserSchoolManager((int)view.UserId) == true)
                {
                    if (view.ContentId != 0)
                    {
                        if (IsUserAssociatedWithCourse((int)view.UserId, (int)view.ContentId, (int)view.SolutionId) == true)
                        {
                            dbmessages = _context
                                .DBMessages
                                .Include(m => m.User)
                                .Include(m => m.Content)
                                .Include(m => m.Solution)
                                .Where(m => m.ContentId == view.ContentId).ToList();
                        }

                        else
                        {
                            _logger.LogInformation("ViewMessages failed - Provided School Manager is not associated with Course of provided Task: " + " UserId: " + view.UserId + " ContentId: " + view.ContentId);
                        }
                    }

                    else if (view.SolutionId != 0)
                    {
                        if (IsUserAssociatedWithCourse((int)view.UserId, (int)view.ContentId, (int)view.SolutionId) == true)
                        {
                            dbmessages = _context
                                .DBMessages
                                .Include(m => m.User)
                                .Include(m => m.Content)
                                .Include(m => m.Solution)
                                .Where(m => m.SolutionId == view.SolutionId).ToList();
                        }

                        else
                        {
                            _logger.LogInformation("ViewMessages failed - Provided School Manager is not associated with Course of provided Solution: " + " UserId: " + view.UserId + " SolutionId: " + view.SolutionId);
                        }
                    }
                }

                else if (_validators.IsUserMentor((int)view.UserId) == true)
                {
                    if (view.ContentId != 0)
                    {
                        if (IsUserOwnsTask((int)view.UserId, (int)view.ContentId, (int)view.SolutionId) == true)
                        {
                            dbmessages = _context
                                .DBMessages
                                .Include(m => m.User)
                                .Include(m => m.Content)
                                .Include(m => m.Solution)
                                .Where(m => m.ContentId == view.ContentId && m.UserId == view.UserId).ToList();
                        }

                        else
                        {
                            _logger.LogInformation("ViewMessages failed - Provided Mentor is not associated with the provided Task: " + " UserId: " + view.UserId + " ContentId: " + view.ContentId);
                        }
                    }

                    else if (view.SolutionId != 0)
                    {
                        if (IsUserOwnsTask((int)view.UserId, (int)view.ContentId, (int)view.SolutionId) == true)
                        {
                            dbmessages = _context
                                .DBMessages
                                .Include(m => m.User)
                                .Include(m => m.Content)
                                .Include(m => m.Solution)
                                .Where(m => m.SolutionId == view.SolutionId && m.UserId == view.UserId).ToList();
                        }

                        else
                        {
                            _logger.LogInformation("ViewMessages failed - Provided Mentor is not associated with the Task of provided Solution: " + " UserId: " + view.UserId + " SolutionId: " + view.SolutionId);
                        }
                    }
                }

                else if (_validators.IsUserStudent((int)view.UserId) == true)
                {
                    if (view.ContentId != 0)
                    {
                        if (IsUserAssociatedWithCourse((int)view.UserId, (int)view.ContentId, (int)view.SolutionId) == true)
                        {
                            dbmessages = _context
                                .DBMessages
                                .Include(m => m.User)
                                .Include(m => m.Content)
                                .Include(m => m.Solution)
                                .Where(m => m.ContentId == view.ContentId).ToList();
                        }

                        else
                        {
                            _logger.LogInformation("ViewMessages failed - Provided Student is not associated with Course of provided Task: " + " UserId: " + view.UserId + " ContentId: " + view.ContentId);
                        }
                    }

                    else if (view.SolutionId != 0)
                    {
                        if (IsUserOwnsSolution((int)view.UserId, (int)view.SolutionId) == true)
                        {
                            dbmessages = _context
                                .DBMessages
                                .Include(m => m.User)
                                .Include(m => m.Content)
                                .Include(m => m.Solution)
                                .Where(m => m.SolutionId == view.SolutionId && m.UserId == view.UserId).ToList();
                        }

                        else
                        {
                            _logger.LogInformation("ViewMessages failed - Provided Mentor is not associated with the Solution: " + " UserId: " + view.UserId + " SolutionId: " + view.SolutionId);
                        }
                    }
                }

                foreach (DBMessage dbmessage in dbmessages)
                {
                    MessageView messageView = new MessageView
                    {
                        Id = dbmessage.Id,
                        Text = dbmessage.Text,
                        UserFullName = dbmessage.User.FirstName + " " + dbmessage.User.LastName,
                        UserTypeId = dbmessage.User.UserTypeId
                    };

                    messagesViews.Add(messageView);
                }

                return new MessagesViewResponse()
                {
                    MessagesViews = messagesViews,
                    ContentId = (int) view.ContentId,
                    SolutionId = (int) view.SolutionId,
                    UserId = (int) view.UserId,
                    Result = "Success",
                    ErrorMessage = "ViewMessages Messages shown"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("ViewMessages Service error: " + ex.Message);

                return new MessagesViewResponse()
                {
                    MessagesViews = new List<MessageView>(),
                    Result = "Failure",
                    ErrorMessage = "ViewMessages Service error: " + ex.Message
                };
            }
        }

        public async Task<ActionResult<MessageActionResponse>> CreateMessage(MessageRequest message)
        {
            try
            {
                if (_validators.IsUserGenuine((int)message.UserId) == false)
                {
                    _logger.LogInformation("CreateMessage failed - Logged in UserId is not same as provided UserId (possible hack attempt!)" + " Provided UserId: " + message.UserId);

                    return new MessageActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "ViewMessages failed - Logged in UserId is not same as provided UserId (possible hack attempt!)"
                    };
                }

                if (_validators.IsUserAdmin((int)message.UserId) == false && _validators.IsUserSchoolManager((int)message.UserId) == false
                    && _validators.IsUserMentor((int)message.UserId) == false && _validators.IsUserStudent((int)message.UserId) == false)
                {
                    _logger.LogInformation("CreateMessage failed - Only Admins, School Managers, Mentors and Students can post Messages: " + " UserId: " + message.UserId);

                    return new MessageActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Only Admins, School Managers, Mentors and Students can post Messages"
                    };
                }

                if (message.ContentId != 0 && message.SolutionId != 0)
                {
                    _logger.LogInformation("CreateMessage failed - Can't post message by both Task and Solution: " + " UserId: " + message.UserId);

                    return new MessageActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Can't search by both Task and Solution"
                    };
                }

                if (message.ContentId == 0 && message.SolutionId == 0)
                {
                    _logger.LogInformation("ViewContents failed - Must provide either ContentId or SolutionId: " + " UserId: " + message.UserId);

                    return new MessageActionResponse()
                    {
                        Result = "Failure",
                        ErrorMessage = "Must provide either ContentId or SolutionId"
                    };
                }

                if (_validators.IsUserAdmin((int)message.UserId) == true)
                {
                    int messageId = PostMessage(message);

                    if (messageId > 0)
                    {
                        _logger.LogInformation("Message created successfully " + " MessageId: " + messageId);

                        return new MessageActionResponse()
                        {
                            MessageResultId = messageId,
                            Result = "Success",
                            ErrorMessage = "Message created successfully"
                        };
                    }

                    else
                    {
                        _logger.LogInformation("CreateMessage failed - Unexpected error with Admin User: " + " UserId: " + message.UserId);

                        return new MessageActionResponse()
                        {
                            Result = "Failure",
                            ErrorMessage = "Unexpected error with Admin User"
                        };
                    }
                }

                else if (_validators.IsUserSchoolManager((int)message.UserId) == true)
                {
                    if (IsUserAssociatedWithCourse(message.UserId, message.ContentId, message.SolutionId) == true)
                    {
                        int messageId = PostMessage(message);

                        if (messageId > 0)
                        {
                            _logger.LogInformation("Message created successfully " + " MessageId: " + messageId);

                            return new MessageActionResponse()
                            {
                                MessageResultId = messageId,
                                Result = "Success",
                                ErrorMessage = "Message created successfully"
                            };
                        }

                        else
                        {
                            _logger.LogInformation("CreateMessage failed - Unexpected error with School Manager: " + " UserId: " + message.UserId);

                            return new MessageActionResponse()
                            {
                                Result = "Failure",
                                ErrorMessage = "Unexpected error with School Manager"
                            };
                        }
                    }

                    else
                    {
                        _logger.LogInformation("CreateMessage failed - Provided School Manager is not associated with Course of provided Task or Solution: " + " UserId: " + message.UserId + " ContentId: " + message.ContentId + " SolutionId: " + message.SolutionId);

                        return new MessageActionResponse()
                        {
                            Result = "Failure",
                            ErrorMessage = "Provided School Manager is not associated with Course of provided Task or Solution"
                        };
                    }
                }

                else if (_validators.IsUserMentor((int)message.UserId) == true)
                {
                    if (IsUserOwnsTask(message.UserId, message.ContentId, message.SolutionId) == true)
                    {
                        int messageId = PostMessage(message);

                        if (messageId > 0)
                        {
                            _logger.LogInformation("Message created successfully " + " MessageId: " + messageId);

                            return new MessageActionResponse()
                            {
                                MessageResultId = messageId,
                                Result = "Success",
                                ErrorMessage = "Message created successfully"
                            };
                        }

                        else
                        {
                            _logger.LogInformation("CreateMessage failed - Unexpected error with Mentor: " + " UserId: " + message.UserId);

                            return new MessageActionResponse()
                            {
                                Result = "Failure",
                                ErrorMessage = "Unexpected error with Mentor"
                            };
                        }
                    }

                    else
                    {
                        _logger.LogInformation("CreateMessage failed - Provided Mentor is not associated with the provided Task or Task of the provided Solution: " + " UserId: " + message.UserId + " ContentId: " + message.ContentId + " SolutionId: " + message.SolutionId);

                        return new MessageActionResponse()
                        {
                            Result = "Failure",
                            ErrorMessage = "Provided Mentor is not associated with the provided Task or Task of the provided Solution"
                        };
                    }
                }

                else if (_validators.IsUserStudent((int)message.UserId) == true)
                {
                    if (message.ContentId != 0)
                    {
                        if (IsUserAssociatedWithCourse(message.UserId, message.ContentId, message.SolutionId) == true)
                        {
                            int messageId = PostMessage(message);

                            if (messageId > 0)
                            {
                                _logger.LogInformation("Message created successfully " + " MessageId: " + messageId);

                                return new MessageActionResponse()
                                {
                                    MessageResultId = messageId,
                                    Result = "Success",
                                    ErrorMessage = "Message created successfully"
                                };
                            }

                            else
                            {
                                _logger.LogInformation("CreateMessage failed - Unexpected error with Student: " + " UserId: " + message.UserId);

                                return new MessageActionResponse()
                                {
                                    Result = "Failure",
                                    ErrorMessage = "Unexpected error with Student"
                                };
                            }
                        }

                        else
                        {
                            _logger.LogInformation("CreateMessage failed - Provided Student is not associated with Course of provided Task" + " UserId: " + message.UserId + " ContentId: " + message.ContentId);

                            return new MessageActionResponse()
                            {
                                Result = "Failure",
                                ErrorMessage = "Provided Student is not associated with Course of provided Task or Solution"
                            };
                        }
                    }

                    if (message.SolutionId != 0)
                    {
                        if (IsUserOwnsSolution(message.UserId, message.SolutionId) == true)
                        {
                            int messageId = PostMessage(message);

                            if (messageId > 0)
                            {
                                _logger.LogInformation("Message created successfully " + " MessageId: " + messageId);

                                return new MessageActionResponse()
                                {
                                    MessageResultId = messageId,
                                    Result = "Success",
                                    ErrorMessage = "Message created successfully"
                                };
                            }

                            else
                            {
                                _logger.LogInformation("CreateMessage failed - Unexpected error with Student: " + " UserId: " + message.UserId);

                                return new MessageActionResponse()
                                {
                                    Result = "Failure",
                                    ErrorMessage = "Unexpected error with Student"
                                };
                            }
                        }

                        else
                        {
                            _logger.LogInformation("CreateMessage failed - Provided Student is not associated with the Task of the provided Solution: " + " UserId: " + message.UserId + " SolutionId: " + message.SolutionId);

                            return new MessageActionResponse()
                            {
                                Result = "Failure",
                                ErrorMessage = "Provided Student is not associated with the provided Task of the provided Solution"
                            };
                        }
                    }
                }

                _logger.LogInformation("CreateMessage failed - Unexpected error: " + " UserId: " + message.UserId + " ContentId: " + message.ContentId + " SolutionId: " + message.SolutionId);

                return new MessageActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "Unexpected error"
                };
            }

            catch (Exception ex)
            {
                _logger.LogError("CreateMessage Service error: " + ex.Message + " UserId: " + message.UserId);

                return new MessageActionResponse()
                {
                    Result = "Failure",
                    ErrorMessage = "CreateMessage Service error: " + ex.Message
                };
            }
        }

        private int PostMessage(MessageRequest message)
        {
            try
            {
                DBMessage newMessage = new DBMessage()
                {
                    UserId = message.UserId,
                    ContentId = message.ContentId != 0 ? message.ContentId : null,
                    SolutionId = message.SolutionId != 0 ? message.SolutionId : null,
                    Text = message.Text,
                    CreatedDate= DateTime.UtcNow,
                    ModifiedDate= DateTime.UtcNow,
                };               

                _context.DBMessages.Add(newMessage);
                _context.SaveChanges();

                return newMessage.Id;
            }

            catch (Exception ex)
            {
                return 0;
            }
        }

        private bool IsUserAssociatedWithCourse(int userId, int ContentId, int solutionId)
        {
            if (ContentId != 0)
            {
                DBContent task = _context
                    .DBContents
                    .Include(t => t.Course)
                    .SingleOrDefault(t => t.Id == ContentId);

                DBCourseUser dbCourseUser = _context.DBCourseUsers.SingleOrDefault(c => c.UserId == userId && c.CourseId == task.Course.Id);

                return dbCourseUser != null;
            }

            else if (solutionId != 0)
            {
                DBSolution solution = _context
                    .DBSolutions
                    .Include(s => s.Content)
                    .ThenInclude(s => s.Course)
                    .SingleOrDefault(s => s.Id == solutionId);

                DBCourseUser dbCourseUser = _context.DBCourseUsers.SingleOrDefault(c => c.UserId == userId && c.Id == solution.Content.Course.Id);

                return dbCourseUser != null;
            }

            return false;
        }

        private bool IsUserOwnsTask(int userId, int ContentId, int solutionId)
        {
            if (ContentId != 0)
            {
                List<DBContent> tasks = _context
                    .DBContents
                    .Include(t => t.Mentor)
                    .Where(t => t.Mentor.Id == userId && t.Id == ContentId).ToList();

                return tasks.Count == 1;
            }

            else if (solutionId != 0)
            {
                DBSolution solution = _context
                    .DBSolutions
                    .Include(s => s.Content)
                    .SingleOrDefault(s => s.Id == solutionId);

                return IsUserOwnsTask(userId, solution.ContentId, 0);
            }

            return false;
        }

        private bool IsUserOwnsSolution(int userId, int solutionId)
        {
            if (solutionId != 0)
            {
                List<DBSolution> solutions = _context
                    .DBSolutions
                    .Include(s => s.Student)
                    .Where(s => s.Student.Id == userId && s.Id == solutionId).ToList();

                return solutions.Count == 1;
            }

            return false;
        }
    }
}