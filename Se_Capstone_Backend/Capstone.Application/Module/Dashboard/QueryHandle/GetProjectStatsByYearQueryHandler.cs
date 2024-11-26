using Capstone.Application.Module.Dashboard.Response;
using Capstone.Infrastructure.Repository;
using Capstone.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Capstone.Application.Module.Dashboard.Query;

namespace Capstone.Application.Module.Dashboard.QueryHandle
{
    public class GetProjectStatsByYearQueryHandler : IRequestHandler<GetProjectStatsByYearQuery, List<ProjectStatsResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProjectStatsByYearQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProjectStatsResponse>> Handle(GetProjectStatsByYearQuery request, CancellationToken cancellationToken)
        {
            var startOfYear = new DateTime(request.Year, 1, 1);
            var endOfYear = new DateTime(request.Year, 12, 31);

            var projectStats = await _unitOfWork.Projects.GetQueryNoTracking()
                .Where(p => (p.StartDate.HasValue && p.StartDate.Value >= startOfYear && p.StartDate.Value <= endOfYear)
                            || (p.EndDate.HasValue && p.EndDate.Value >= startOfYear && p.EndDate.Value <= endOfYear))
                .GroupBy(p => p.Status)
                .Select(group => new
                {
                    Status = group.Key,
                    Count = group.Count()
                })
                .ToListAsync(cancellationToken);

            var allStatuses = Enum.GetValues(typeof(ProjectStatus)).Cast<ProjectStatus>();

            var result = allStatuses.Select(status => new ProjectStatsResponse
            {
                Status = status,
                Count = projectStats.FirstOrDefault(stat => stat.Status == status)?.Count ?? 0
            }).ToList();

            return result;
        }
    }
}
