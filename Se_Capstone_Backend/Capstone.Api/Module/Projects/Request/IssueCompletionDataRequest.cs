namespace Capstone.Api.Module.Projects.Request
{
    public class IssueCompletionDataRequest
    {
        public Guid ProjectId { get; set; }
        public Guid? PhaseId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
