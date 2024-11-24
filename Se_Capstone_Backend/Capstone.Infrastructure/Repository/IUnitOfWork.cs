using Capstone.Domain.Entities;

namespace Capstone.Infrastructure.Repository
{
    public interface IUnitOfWork
    {
        public IRepository<Applicant> Applicants { get; }
        public IRepository<Attendance> Attendances { get; }
        public IRepository<Issue> Issues { get; }
        public IRepository<Job> Jobs { get; }
        public IRepository<Skill> Skills { get; }
        public IRepository<Label> Labels { get; }
        public IRepository<Article> Articles { get; }
        public IRepository<Permission> Permissions { get; }
        public IRepository<Project> Projects { get; }
        public IRepository<Role> Roles { get; }
        public IRepository<Status> Statuses { get; }
        public IRepository<User> Users { get; }
        public IRepository<Position> Positions { get; }
        public IRepository<Comment> Comments { get; }
        public IRepository<Phase> Phases { get; }
        public IRepository<UserProject> UserProjects { get; }
        public IRepository<GroupPermission> GroupPermissions { get; }
        public IRepository<Notification> Notifications { get; }
        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        Task<int> SaveChangesAsync();

        Task Dispose(bool disposing);
    }
}
