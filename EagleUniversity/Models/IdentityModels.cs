using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EagleUniversity.Models
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ActivityType> ActivityTypes { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Assignments> Assignments { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<CourseDocument> CourseDocuments { get; set; }
        public DbSet<ModuleDocument> ModuleDocuments { get; set; }
        public DbSet<ActivityDocument> ActivityDocuments { get; set; }
        public DbSet<ExceptionLogger> ExceptionLoggers { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

      
    }
}