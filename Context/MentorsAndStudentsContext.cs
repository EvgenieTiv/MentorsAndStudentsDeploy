using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MentorsAndStudents.Context
{
    public class MentorsAndStudentsContext: DbContext
    {
        public MentorsAndStudentsContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<DBCourse> DBCourses { get; set; }
        public DbSet<DBCourseUser> DBCourseUsers { get; set; }
        public DbSet<DBContent> DBContents { get; set; }
        public DbSet<DBSchool> DBSchools { get; set; }
        public DbSet<DBSchoolUser> DBSchoolUsers { get; set; }
        public DbSet<DBSolution> DBSolutions { get; set; }
        public DbSet<DBTopic> DBTopics { get; set; }
        public DbSet<DBUser> DBUsers { get; set; }
        public DbSet<DBMessage> DBMessages { get; set; }
        public DbSet<DBMentorSchoolTopic> DBMentorsSchoolsTopics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }        
    }
}
