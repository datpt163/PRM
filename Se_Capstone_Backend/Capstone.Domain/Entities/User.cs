using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Capstone.Domain.Enums;
namespace Capstone.Domain.Entities
{
    public partial class User : IdentityUser<Guid>
    {
        [MaxLength(100)]
        public string? Avatar { get; set; }
        [MaxLength(100)]
        public string? Address { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Active;
        [MaxLength(30)]
        public Gender Gender { get; set; } 
        public DateTime? Dob { get; set; }
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; } 
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? RefreshToken { get; set; }
        public User() { }

        public ICollection<Skill>? Skills { get; set; } = new List<Skill>();

        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<Project> LeadProjects { get; set; } = new List<Project>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Issue> IssuesUpdate { get; set; } = new List<Issue>();
        public ICollection<Issue> AssinedIssues { get; set; } = new List<Issue>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Issue> ReportIssues { get; set; } = new List<Issue>();
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
        public User(string email, string address, Gender gender, DateTime dob, string phone, string userName, string fullName)
        {
            Email = email;  
            Address = address;
            Gender = gender;
            Dob = dob;
            UserName = userName;
            PhoneNumber = phone;
            FullName = fullName;
            CreateDate = DateTime.Now;
        }
    }
}
