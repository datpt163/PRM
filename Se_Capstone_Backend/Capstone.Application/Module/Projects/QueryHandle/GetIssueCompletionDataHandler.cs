using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Application.Resources;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.QueryHandle
{
    public class GetIssueCompletionDataHandler : IRequestHandler<GetIssueCompletionDataQuery, List<IssueCompletionData>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetIssueCompletionDataHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<IssueCompletionData>> Handle(GetIssueCompletionDataQuery request, CancellationToken cancellationToken)
        {
            if (request.StartDate > request.EndDate)
                throw new Exception(Messages.end_date_greater_than_start_date);

            var project = await _unitOfWork.Projects
                .GetQueryNoTracking()
                .Include(p => p.Statuses)
                .ThenInclude(status => status.Issues)
                .Include(p => p.Phases.Where(phase =>
                    request.PhaseId == null || phase.Id == request.PhaseId))
                .ThenInclude(phase => phase.Issues)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

            if (project == null)
                throw new Exception(Messages.project_not_found);

            var relevantIssues = request.PhaseId == null
                ? project.Statuses.SelectMany(status => status.Issues).ToList()
                : project.Phases.SelectMany(phase => phase.Issues).ToList();

            if (request.UserId.HasValue)
            {
                relevantIssues = relevantIssues.Where(issue => issue.AssigneeId == request.UserId).ToList();
            }

            var completedIssues = relevantIssues
                .Where(issue => issue.ActualDate.HasValue &&
                                issue.ActualDate >= request.StartDate &&
                                issue.ActualDate <= request.EndDate)
                .ToList();

            var dueIssues = relevantIssues
                .Where(issue => issue.DueDate.HasValue &&
                                issue.DueDate >= request.StartDate &&
                                issue.DueDate <= request.EndDate)
                .ToList();

            var groupedData = new Dictionary<string, IssueCompletionData>();

            foreach (var issue in completedIssues)
            {
                var key = issue.ActualDate!.Value.ToString("dd/MM/yyyy");
                if (!groupedData.ContainsKey(key))
                    groupedData[key] = new IssueCompletionData { Period = key };
                groupedData[key].CompletedTasks++;
            }

            foreach (var issue in dueIssues)
            {
                var key = issue.DueDate!.Value.ToString("dd/MM/yyyy");
                if (!groupedData.ContainsKey(key))
                    groupedData[key] = new IssueCompletionData { Period = key };
                groupedData[key].DueTasks++;
            }

            var allDays = Enumerable.Range(0, (request.EndDate - request.StartDate).Days + 1)
                .Select(offset => request.StartDate.AddDays(offset).ToString("dd/MM/yyyy"))
                .ToList();

            var result = allDays.Select(day => new IssueCompletionData
            {
                Period = day,
                CompletedTasks = groupedData.ContainsKey(day) ? groupedData[day].CompletedTasks : 0,
                DueTasks = groupedData.ContainsKey(day) ? groupedData[day].DueTasks : 0
            }).ToList();

            return result;
        }
    }
}
