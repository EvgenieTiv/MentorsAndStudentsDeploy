using MentorsAndStudents.Common;

namespace MentorsAndStudents
{
    public class GenerateRandoms : IGenerateRandoms
    {
        public string GenerateRandomString()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "-");
            GuidString = GuidString.Replace("+", "-");
            GuidString = GuidString.Replace("/", "-");

            return GuidString;
        }
    }
}
