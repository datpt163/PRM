using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.DbContexts
{
    public partial class SeCapstoneContext : IdentityDbContext<User, Role, Guid>
    {
        public SeCapstoneContext()
        {
        }

        public SeCapstoneContext(DbContextOptions<SeCapstoneContext> options)
    : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=ec2-52-221-213-11.ap-southeast-1.compute.amazonaws.com:5432;Database=SeCapstone_db;Username=postgres;Password=headnshoulder");
            }
        }

        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<GroupPermission> GroupPermissions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
            .HasMany(u => u.Skills)
            .WithMany(s => s.Users);

            modelBuilder.Entity<Applicant>()
                .Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Notification>()
               .Property(e => e.Id)
               .HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<Permission>()
              .Property(e => e.Id)
              .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Attendance>()
               .Property(e => e.Id)
               .HasDefaultValueSql("gen_random_uuid()");
            modelBuilder.Entity<Comment>()
              .Property(e => e.Id)
              .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Issue>()
               .Property(e => e.Id)
               .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Job>()
               .Property(e => e.Id)
               .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Label>()
               .Property(e => e.Id)
               .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Position>()
              .Property(e => e.Id)
              .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Article>()
               .Property(e => e.Id)
               .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<Project>()
               .Property(e => e.Id)
               .HasDefaultValueSql("gen_random_uuid()");


            modelBuilder.Entity<Status>()
              .Property(e => e.Id)
              .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<GroupPermission>()
             .Property(e => e.Id)
             .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<User>()
             .Property(e => e.Gender)
             .HasColumnType("smallint")
              .HasConversion<int>()
            .HasDefaultValue(Gender.Male);

            modelBuilder.Entity<User>()
            .Property(e => e.Status)
            .HasColumnType("smallint")
            .HasConversion<int>()
            .HasDefaultValue(UserStatus.Active);

            modelBuilder.Entity<Issue>()
           .Property(e => e.Priority)
           .HasColumnType("smallint")
            .HasConversion<int>()
            .HasDefaultValue(Priority.Low);

            modelBuilder.Entity<Project>()
          .Property(e => e.Status)
          .HasColumnType("smallint")
           .HasConversion<int>()
           .HasDefaultValue(ProjectStatus.InProgress);

            modelBuilder.Entity<Attendance>()
            .HasOne(a => a.User)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Issue>()
              .HasOne(a => a.Label)
               .WithMany(s => s.Issues)
               .HasForeignKey(a => a.LabelId);

            modelBuilder.Entity<Comment>()
            .HasOne(a => a.User)
             .WithMany(s => s.Comments)
             .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Label>()
          .HasOne(a => a.Project)
           .WithMany(s => s.Labels)
           .HasForeignKey(a => a.ProjectId);


            modelBuilder.Entity<Notification>()
          .HasOne(a => a.User)
           .WithMany(s => s.Notifications)
           .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Phase>()
        .HasOne(a => a.Project)
         .WithMany(s => s.Phases)
         .HasForeignKey(a => a.ProjectId);

            modelBuilder.Entity<Issue>()
      .HasOne(a => a.Phase)
       .WithMany(s => s.Issues)
       .HasForeignKey(a => a.PhaseId);

            modelBuilder.Entity<Status>()
        .HasOne(a => a.Project)
         .WithMany(s => s.Statuses)
         .HasForeignKey(a => a.ProjectId);


            modelBuilder.Entity<Comment>()
      .HasOne(a => a.Issue)
      .WithMany(s => s.Comments)
      .HasForeignKey(a => a.IssueId)
      .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Issue>()
            .HasOne(a => a.ParentIssue)
            .WithMany(s => s.SubIssues)
            .HasForeignKey(a => a.ParentIssueId);

            modelBuilder.Entity<Issue>()
           .HasOne(a => a.LastUpdateBy)
           .WithMany(s => s.IssuesUpdate)
           .HasForeignKey(a => a.LastUpdateById);



            modelBuilder.Entity<Issue>()
             .HasOne(a => a.Status)
              .WithMany(s => s.Issues)
              .HasForeignKey(a => a.StatusId);


            modelBuilder.Entity<Project>()
                  .HasOne(a => a.Lead)
                   .WithMany(s => s.LeadProjects)
                   .HasForeignKey(a => a.LeadId);

            modelBuilder.Entity<Permission>()
              .HasOne(a => a.GroupPermission)
               .WithMany(s => s.Permissions)
               .HasForeignKey(a => a.GroupPermissionId);

            modelBuilder.Entity<Applicant>()
           .HasOne(a => a.MainJob)
           .WithMany()
           .HasForeignKey(a => a.MainJobId)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Issue>()
        .HasOne(a => a.Reporter)
        .WithMany(a => a.ReportIssues)
        .HasForeignKey(a => a.ReporterId)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Issue>()
     .HasOne(a => a.Assignee)
     .WithMany(a => a.AssinedIssues)
     .HasForeignKey(a => a.AssigneeId)
     .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserProject>()
            .HasOne(a => a.Position)
            .WithMany(a => a.UserProjects)
            .HasForeignKey(a => a.PositionId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserProject>()
            .HasOne(a => a.Project)
            .WithMany(a => a.UserProjects)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProject>()
          .HasOne(a => a.User)
          .WithMany(a => a.UserProjects)
          .HasForeignKey(a => a.UserId)
          .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProject>()
            .HasOne(a => a.Position)
            .WithMany(a => a.UserProjects)
            .HasForeignKey(a => a.PositionId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserProject>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.ProjectId }); 

                entity.Property(e => e.IsProjectConfigurator).HasDefaultValue(false);
                entity.Property(e => e.IsIssueConfigurator).HasDefaultValue(false);
            });

            modelBuilder.Entity<Applicant>()
              .HasMany(a => a.Jobs)
              .WithMany(j => j.Applicants)
              .UsingEntity<Dictionary<string, object>>(
                  "applicantJobs",
                  j => j.HasOne<Job>().WithMany().HasForeignKey("JobId"),
                  a => a.HasOne<Applicant>().WithMany().HasForeignKey("ApplicantId"));

            modelBuilder.Entity<Permission>()
          .HasMany(a => a.Roles)
          .WithMany(j => j.Permissions)
          .UsingEntity<Dictionary<string, object>>(
           "rolePermissions",
              j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
              a => a.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"));


            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName != null && tableName.StartsWith("AspNet"))
                {
                    string tableNameSub = tableName.Substring(6);
                    if (tableNameSub.Length > 0)
                    {
                        tableNameSub = char.ToLower(tableNameSub[0]) + tableNameSub.Substring(1);
                    }
                    entityType.SetTableName(tableNameSub);
                }
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var columnName = char.ToLower(property.Name[0]) + property.Name.Substring(1);
                    property.SetColumnName(columnName);
                }
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
