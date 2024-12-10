using Capstone.Application.Resources;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Projects.Query
{
    public class GetTaskOverviewHandler : IRequestHandler<GetTaskOverviewQuery, TaskOverviewResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTaskOverviewHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TaskOverviewResponse> Handle(GetTaskOverviewQuery request, CancellationToken cancellationToken)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            {
                throw new Exception(Messages.end_date_greater_than_start_date);
            }

            var project = await _unitOfWork.Projects
                           .GetQueryNoTracking()
                           .Include(s => s.Statuses)
                           .ThenInclude(x => x.Issues)
                           .Include(x => x.Phases
                               .Where(phase => phase.ProjectId == request.ProjectId &&
                                               (request.PhaseId == null || phase.Id == request.PhaseId)))
                           .ThenInclude(phase => phase.Issues)
                           .ThenInclude(s => s.Status)
                           .FirstOrDefaultAsync(x=> x.Id == request.ProjectId,cancellationToken);

            if (project == null)
            {
                throw new Exception(Messages.project_not_found);
            }

            var ongoingTasks = 0;
            var totalTasks = 0;
            var doneTasks = 0;
            var taskCompletionRate = new List<TaskCompletionRate>();

            var allIssues = request.PhaseId == null
                ? project.Statuses.SelectMany(status => status.Issues).ToList()
                : project.Phases.SelectMany(phase => phase.Issues).ToList();

            var totalEffort = project.TotalEffort ?? 0;
            float estimateEffort = 0;
            float actualEffort = 0;
            foreach(var issue in allIssues)
            {
                actualEffort = totalEffort - (issue.ActualTime ?? 0);

                estimateEffort = totalEffort - (issue.EstimatedTime ?? 0);
            }
            
            allIssues = allIssues.Where(issue => (!request.StartDate.HasValue || issue.StartDate >= request.StartDate) &&
                                (!request.EndDate.HasValue || issue.DueDate <= request.EndDate)).ToList();

            totalTasks = allIssues.Count;

            foreach (var issue in allIssues)
            {
                if (issue.Status?.IsDone == true)
                {
                    doneTasks++;
                }
                else if (issue.Status?.IsDone == false || issue?.Status?.IsDone == null)
                {
                    ongoingTasks++;
                }
            }

            double remainingPercentage = 100;
            int statusesCount = project.Statuses.Count;

            for (int i = 0; i < statusesCount; i++)
            {
                var status = project.Statuses.ElementAt(i);
                int statusTaskCount = allIssues.Count(issue => issue.Status?.Id == status.Id);

                if (i == statusesCount - 1)
                {
                    taskCompletionRate.Add(new TaskCompletionRate
                    {
                        Status = status.Name,
                        Percentage = Math.Round(remainingPercentage, 2)
                    });
                }
                else
                {
                    double percentage = totalTasks > 0
                        ? Math.Round((double)statusTaskCount * 100 / totalTasks, 2)
                        : 0;

                    remainingPercentage -= percentage;
                    taskCompletionRate.Add(new TaskCompletionRate
                    {
                        Status = status.Name,
                        Percentage = percentage
                    });
                }
            }

            var overallCompletionRate = totalTasks > 0
                ? Math.Round((double)doneTasks * 100 / totalTasks, 2)
                : 0;

            return new TaskOverviewResponse
            {
                OngoingTasks = ongoingTasks,
                TotalTasks = totalTasks,
                DoneTasks = doneTasks,
                TaskCompletionRate = taskCompletionRate,
                OverallCompletionRate = overallCompletionRate,
                TotalEffort = totalEffort,
                EstimateEffort = estimateEffort,
            };
        }
    }
}
