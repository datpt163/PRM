namespace Capstone.Api.Module.Projects.Request
{
    public class TaskOverviewRequest
    {
        public Guid ProjectId { get; set; }
        public Guid? PhaseId { get; set; }
    }
}
