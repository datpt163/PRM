using Capstone.Application.Module.Dashboard.Query;
using Capstone.Application.Module.Dashboard.Response;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Dashboard.QueryHandle
{
    public class GetProjectStatsByMonthQueryHandler : IRequestHandler<GetProjectStatsByMonthQuery, List<ProjectStatsResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProjectStatsByMonthQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProjectStatsResponse>> Handle(GetProjectStatsByMonthQuery request, CancellationToken cancellationToken)
        {
            var startOfMonth = new DateTime(request.Date.Year, request.Date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var projectStats = await _unitOfWork.Projects.GetQueryNoTracking()
                .Where(p => (p.StartDate.HasValue && p.StartDate.Value >= startOfMonth && p.StartDate.Value <= endOfMonth)
                            || (p.EndDate.HasValue && p.EndDate.Value >= startOfMonth && p.EndDate.Value <= endOfMonth))
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
