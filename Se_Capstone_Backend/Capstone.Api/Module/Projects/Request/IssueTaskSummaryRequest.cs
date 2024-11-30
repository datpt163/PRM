namespace Capstone.Api.Module.Projects.Request
{
    public class IssueTaskSummaryRequest
    {
        public Guid ProjectId { get; set; }
        public Guid? PhaseId { get; set; }
        public Guid UserId { get; set; }
    }

}
