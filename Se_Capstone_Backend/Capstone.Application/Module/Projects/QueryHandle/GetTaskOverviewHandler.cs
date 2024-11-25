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

            var project = await _unitOfWork.Projects
                           .GetQueryNoTracking()
                           .Include(s=> s.Statuses)
                           .Include(x => x.Phases
                               .Where(phase => phase.ProjectId == request.ProjectId &&
                                               (request.PhaseId == null || phase.Id == request.PhaseId)))
                           .ThenInclude(phase => phase.Issues)
                           .ThenInclude(s=> s.Status)
                           .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
            {
                throw new Exception("Project is not exist!");
            }
            var ongoingTasks = 0;
            var totalTasks = 0;
            var doneTasks = 0;
            var taskCompletionRate = new List<TaskCompletionRate> ();

            if (!project.Phases.Any())
            {
                return new TaskOverviewResponse
                {
                    OngoingTasks = ongoingTasks,
                    TotalTasks = totalTasks,
                    DoneTasks = doneTasks,
                    TaskCompletionRate = taskCompletionRate
                };
            }

            foreach (var phase in project.Phases)
            {

                totalTasks += phase.Issues.Count();
                
            }


            foreach (var status in project.Statuses)
            {
                int statusTaskCount = 0;

                foreach (var phase in project.Phases)
                {
                    foreach (var issue in phase.Issues)
                    {

                        if (issue.Status?.Id == status.Id)
                        {
                            statusTaskCount++;

                            if (issue.Status?.IsDone == true)
                            {
                                doneTasks++;
                            }
                            else if (issue.Status?.IsDone == false)
                            {
                                ongoingTasks++;
                            }
                        }
                    }
                }

                if (statusTaskCount > 0)
                {
                    taskCompletionRate.Add(new TaskCompletionRate
                    {
                        Status = status.Name,
                        Percentage = (double)statusTaskCount * 100 / totalTasks
                    });
                }
                else
                {
                    taskCompletionRate.Add(new TaskCompletionRate
                    {
                        Status = status.Name,
                        Percentage = 0
                    });
                }
            }

            var overallCompletionRate = totalTasks > 0
                ? (double)doneTasks * 100 / totalTasks
                : 0;

            return new TaskOverviewResponse
            {
                OngoingTasks = ongoingTasks,
                TotalTasks = totalTasks,
                DoneTasks = doneTasks,
                TaskCompletionRate = taskCompletionRate
            };
        }
    }
}
