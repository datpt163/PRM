namespace Capstone.Api.Module.Projects.Request
{
    public class TaskOverviewRequest
    {
        public Guid ProjectId { get; set; }
        public Guid? PhaseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
