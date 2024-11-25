using Capstone.Application.Module.Dashboard.Query;
using Capstone.Application.Module.Dashboard.Response;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Dashboard.QueryHandle
{
    public class GetDashboardOverviewQueryHandler : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDashboardOverviewQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardOverviewResponse> Handle(GetDashboardOverviewQuery request, CancellationToken cancellationToken)
        {
            var totalTasks = await _unitOfWork.Issues.GetQueryNoTracking().CountAsync(cancellationToken);

            var ongoingTasks = await _unitOfWork.Issues.GetQueryNoTracking()
                .Include(x => x.Status).CountAsync(i => i.Status != null && i.Status.IsDone == false, cancellationToken);


            var pausedTasks = await _unitOfWork.Issues.GetQueryNoTracking().Include(x => x.Status).CountAsync(i => i.Status != null && i.Status.IsDone == true, cancellationToken);


            var unfinishedProjects = await _unitOfWork.Projects.GetQueryNoTracking()
                .CountAsync(p => p.Status != ProjectStatus.Finished, cancellationToken);


            var projectsDueThisMonth = await _unitOfWork.Projects.GetQueryNoTracking()
                .CountAsync(p => p.EndDate.HasValue &&
                                 p.EndDate.Value.Month == DateTime.Now.Month &&
                                 p.EndDate.Value.Year == DateTime.Now.Year, cancellationToken);

            var taskCompletionRate = await _unitOfWork.Issues.GetQueryNoTracking()
                                     .Include(x => x.Status)
                                     .GroupBy(i => i.Status.Name)
                                     .Select(group => new TaskCompletionRate
                                     {
                                         Status = group.Key ?? "Unknown",
                                         Percentage = (double)group.Count() * 100 / totalTasks
                                     })
                                     .ToListAsync(cancellationToken);


            return new DashboardOverviewResponse
            {
                OngoingTasks = ongoingTasks,
                TotalTasks = totalTasks,
                UnfinishedProjects = unfinishedProjects,
                ProjectsDueThisMonth = projectsDueThisMonth,
                PausedTasks = pausedTasks,
                TaskCompletionRate = taskCompletionRate
            };
        }
    }
}
