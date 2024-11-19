using Capstone.Application.Common.ResponseMediator;
using Capstone.Domain.Enums;
using MediatR;

namespace Capstone.Application.Module.Status.Query
{
    public class GetListStatusQuery : IRequest<ResponseMediator>
    {
        public Guid? projectId { get; set; }
    }
    public class GetListStatusKanbanQuery : IRequest<ResponseMediator>
    {
        public Guid? projectId { get; set; }
        public int? Index { get; set; }
        public string? Title { get; set; }
        public Priority? Priority { get; set; }
        public List<Guid>? AssigneeId { get; set; }
        public Guid? ReporterId { get; set; }
        public List<Guid>? StatusId { get; set; }
        public List<Guid>? LabelId { get; set; }
        public List<Guid>? PhaseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

    }
}
