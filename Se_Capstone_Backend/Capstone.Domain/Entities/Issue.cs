using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Capstone.Domain.Entities.Common;
using Capstone.Domain.Enums;
namespace Capstone.Domain.Entities
{
    [Table("issues")]
    public class Issue : BaseEntity
    {
        public Guid Id { get; set; }
        public int Index {  get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int Percentage { get; set; } = 0;
        public Priority? Priority { get; set; }
        public float? EstimatedTime { get; set; } 
        public float? ActualTime { get; set; } 
        public int Position { get; set; }
        public Guid? ParentIssueId { get; set; }
        public Guid ReporterId { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid? LastUpdateById { get; set; }
        public Guid StatusId { get; set; }
        public Guid? LabelId { get; set; }
        public Guid? PhaseId { get; set; }
        public Phase? Phase { get; set; }
        public Label? Label { get; set; }
        public Status Status { get; set; } = null!;
        public User? LastUpdateBy { get; set; }
        public Issue? ParentIssue { get; set; }
        public User Reporter { get; set; } = null!;
        public User? Assignee { get; set; } = null!;
        public List<Issue> SubIssues { get; set; } = new List<Issue>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
