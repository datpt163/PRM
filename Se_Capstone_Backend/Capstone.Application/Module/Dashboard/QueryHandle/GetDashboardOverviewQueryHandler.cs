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
            var totalTasks = await _unitOfWork.Issues.GetQueryNoTracking().Where(x => !x.IsDeleted).CountAsync(cancellationToken);

            var ongoingTasks = await _unitOfWork.Issues.GetQueryNoTracking().Where(x=> !x.IsDeleted)
                .Include(x => x.Status).CountAsync(i => i.Status != null && i.Status.IsDone == false, cancellationToken);

            var totalProjects = await _unitOfWork.Projects.GetQueryNoTracking().Where(x=> !x.IsDeleted).CountAsync(cancellationToken);

            var currentYear = DateTime.Now.Year;

            var totalProjectsDone = await _unitOfWork.Projects.GetQueryNoTracking()
                .Where(p => !p.IsDeleted && p.Status == ProjectStatus.Finished && ((p.StartDate.HasValue && p.StartDate.Value.Year == currentYear)|| (p.EndDate.HasValue && p.EndDate.Value.Year == currentYear)))
                .CountAsync(cancellationToken);


            var totalSkillsEmployee = await _unitOfWork.Skills.GetQueryNoTracking().Where(x=> !x.IsDeleted).CountAsync(cancellationToken);
            var totalEmployee = await _unitOfWork.Users.GetQueryNoTracking().Where(x=> x.Status != UserStatus.Inacitve).CountAsync(cancellationToken);

            return new DashboardOverviewResponse
            {
                OngoingTasks = ongoingTasks,
                TotalTasks = totalTasks,
                TotalProjects = totalProjects,
                TotalProjectsDone = totalProjectsDone,
                TotalSkillsEmployee = totalSkillsEmployee,
                TotalEmployee = totalEmployee
            };
        }
    }
}
