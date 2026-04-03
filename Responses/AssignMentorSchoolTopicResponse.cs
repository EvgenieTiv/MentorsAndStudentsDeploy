namespace MentorsAndStudents
{
    public class AssignMentorSchoolTopicResponse
    {
        public int MentorId { get; set; }
        public int SchoolId { get; set; }
        public int TopicId { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
