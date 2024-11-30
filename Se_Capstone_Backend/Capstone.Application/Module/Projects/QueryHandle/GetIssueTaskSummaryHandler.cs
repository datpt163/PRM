using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.QueryHandle
{
    public class GetIssueTaskSummaryHandler : IRequestHandler<GetIssueTaskSummaryQuery, IssueTaskSummaryResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetIssueTaskSummaryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IssueTaskSummaryResponse> Handle(GetIssueTaskSummaryQuery request, CancellationToken cancellationToken)
        {
            var project = await _unitOfWork.Projects
                .GetQueryNoTracking()
                .Include(p => p.Statuses)
                .ThenInclude(status => status.Issues)
                .Include(p => p.Phases.Where(phase =>
                    request.PhaseId == null || phase.Id == request.PhaseId))
                .ThenInclude(phase => phase.Issues)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

            if (project == null)
                throw new Exception("Project not found.");

            var relevantIssues = request.PhaseId == null
                ? project.Statuses.SelectMany(status => status.Issues).ToList()
                : project.Phases.SelectMany(phase => phase.Issues).ToList();

            relevantIssues = relevantIssues.Where(issue => issue.AssigneeId == request.UserId).ToList();

            var totalIssues = relevantIssues.Count;

            var unfinishedTasks = relevantIssues.Count(issue => issue.Status.IsDone == false);

            return new IssueTaskSummaryResponse
            {
                TotalIssues = totalIssues,
                TotalUnfinishedTasks = unfinishedTasks
            };
        }
    }
}
