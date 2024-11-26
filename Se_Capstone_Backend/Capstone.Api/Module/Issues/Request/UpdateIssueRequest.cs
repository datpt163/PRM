using Capstone.Domain.Enums;

namespace Capstone.Api.Module.Issues.Request
{
    public class UpdateIssueRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int Percentage { get; set; }
        public Priority? Priority { get; set; }
        public float? EstimatedTime { get; set; }
        public Guid? ParentIssueId { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid StatusId { get; set; }
        public Guid? LabelId { get; set; }
        public Guid? PhaseId { get; set; }
        public float? ActualTime { get; set; } = 0;
    }
}
