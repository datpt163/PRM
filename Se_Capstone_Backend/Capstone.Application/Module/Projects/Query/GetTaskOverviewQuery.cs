using MediatR;

namespace Capstone.Application.Module.Projects.Query
{
    public class GetTaskOverviewQuery : IRequest<TaskOverviewResponse>
    {
        public Guid ProjectId { get; set; }
        public Guid? PhaseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
