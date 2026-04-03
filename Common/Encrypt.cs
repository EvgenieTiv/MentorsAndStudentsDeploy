using System.Security.Cryptography;
using System.Text;

namespace MentorsAndStudents
{
    public class Encrypt
    {
        public static string Sha256(string text)
        {
            var sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                var result = hash.ComputeHash(Encoding.UTF8.GetBytes(text));
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < result.Length; i++)
                    sb.Append(result[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
