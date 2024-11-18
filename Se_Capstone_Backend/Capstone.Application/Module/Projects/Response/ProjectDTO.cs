using Capstone.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Capstone.Application.Module.Projects.Response
{
    public class ProjectDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsVisible { get; set; } = false;
        public Guid? LeadId { get; set; }
        public string? LeadName { get; set; }
    }

    public class UserDTO
    {
        public Guid Id;
        public string Name = string.Empty;
    }

    public class IssueDTO
    {
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int Percentage { get; set; } = 0;
        public Priority Priority { get; set; }
        public int? EstimatedTime { get; set; }
        public int PercentDone { get; set; }
    }
}
