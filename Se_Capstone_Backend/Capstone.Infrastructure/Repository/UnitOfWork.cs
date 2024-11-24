using Capstone.Domain.Entities;
using Capstone.Infrastructure.DbContexts;

namespace Capstone.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SeCapstoneContext _context;
        private bool disposed = false;
        public UnitOfWork(SeCapstoneContext context)
        {
            _context = context;
        }
        public IRepository<User> users = null!;
        public IRepository<Applicant> applicants = null!;
        public IRepository<Attendance> attendances = null!;
        public IRepository<Issue> issues = null!;
        public IRepository<Job> jobs = null!;
        public IRepository<Skill> skills = null!;
        public IRepository<Label> labels = null!;
        public IRepository<Position> positions = null!;
        public IRepository<Article> articles = null!;
        public IRepository<Permission> permissions = null!;
        public IRepository<Project> projects = null!;
        public IRepository<Role> roles = null!;
        public IRepository<Status> statuses = null!;
        public IRepository<Comment> comments = null!;
        public IRepository<Phase> phases = null!;
        public IRepository<UserProject> userProjects = null!;
        public IRepository<GroupPermission> groupPermissions = null!;
        public IRepository<Notification> notifications = null!;
        public IRepository<User> Users => users ?? new Repository<User>(_context);
        public IRepository<Notification> Notifications => notifications ?? new Repository<Notification>(_context);
        public IRepository<Comment> Comments => comments ?? new Repository<Comment>(_context);
        public IRepository<UserProject> UserProjects => userProjects ?? new Repository<UserProject>(_context);
        public IRepository<Position> Positions => positions ?? new Repository<Position>(_context);
        public IRepository<Applicant> Applicants => applicants ?? new Repository<Applicant>(_context);
        public IRepository<Attendance> Attendances => attendances ?? new Repository<Attendance>(_context);
        public IRepository<Issue> Issues => issues ?? new Repository<Issue>(_context);
        public IRepository<Job> Jobs => jobs ?? new Repository<Job>(_context);
        public IRepository<Skill> Skills => skills ?? new Repository<Skill>(_context);
        public IRepository<Phase> Phases => phases ?? new Repository<Phase>(_context);
        public IRepository<Label> Labels => labels ?? new Repository<Label>(_context);
        public IRepository<Article> Articles => articles ?? new Repository<Article>(_context);
        public IRepository<Permission> Permissions => permissions ?? new Repository<Permission>(_context);
        public IRepository<Project> Projects => projects ?? new Repository<Project>(_context);
        public IRepository<Role> Roles => roles ?? new Repository<Role>(_context);
        public IRepository<Status> Statuses => statuses ?? new Repository<Status>(_context);
        public IRepository<GroupPermission> GroupPermissions => groupPermissions ?? new Repository<GroupPermission>(_context);


        public int SaveChanges()
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            return _context.SaveChanges();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        //public Task BulkSaveChangesAsync()
        //{
        //    return _context.BulkSaveChangesAsync();
        //}

        public IRepository<T> AsyncRepository<T>() where T : class
        {
            return new Repository<T>(_context);
        }

        public async Task Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    await _context.DisposeAsync();
                }
            }
            disposed = true;
        }
    }
}
