namespace MentorsAndStudents
{
    public class ViewMentorSchoolTopicConnectionsResponse
    {
        public int MentorId { get; set; }
        public int SchoolId { get; set; }
        public List<Topic> AssignedTopics { get; set; }
        public List<Topic> NotAssignedTopics { get; set; }
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
    }
}
