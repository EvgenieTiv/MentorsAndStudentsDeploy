using AutoMapper;
using MentorsAndStudents.Models;

namespace MentorsAndStudents.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, DBUser>();
            CreateMap<DBUser, User>();
            CreateMap<Topic, DBTopic>();
            CreateMap<DBTopic, Topic>();
            CreateMap<School, DBSchool>();
            CreateMap<DBSchool, School>();
            CreateMap<SchoolUser, DBSchoolUser>();
            CreateMap<DBSchoolUser, SchoolUser>();
            CreateMap<Course, DBCourse>();
            CreateMap<DBCourse, Course>();
            CreateMap<CourseUser, DBCourseUser>();
            CreateMap<DBCourseUser, CourseUser>();
            CreateMap<Content, DBContent>();
            CreateMap<DBContent, Content>();
            CreateMap<Solution, DBSolution>();
            CreateMap<DBSolution, Solution>();
            CreateMap<Message, DBMessage>();
            CreateMap<DBMessage, Message>();
            CreateMap<MentorSchoolTopic, DBMentorSchoolTopic>();
            CreateMap<DBMentorSchoolTopic, MentorSchoolTopic>();
        }
    }
}
