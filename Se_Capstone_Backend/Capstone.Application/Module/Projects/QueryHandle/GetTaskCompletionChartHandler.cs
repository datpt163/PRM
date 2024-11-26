using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.QueryHandle
{
    public class GetTaskCompletionChartHandler : IRequestHandler<GetTaskCompletionChartQuery, List<TaskCompletionPoint>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTaskCompletionChartHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<TaskCompletionPoint>> Handle(GetTaskCompletionChartQuery request, CancellationToken cancellationToken)
        {
            var project = await _unitOfWork.Projects
                .GetQueryNoTracking()
                .Include(p => p.Phases.Where(phase =>
                    request.PhaseId == null || phase.Id == request.PhaseId))
                .ThenInclude(phase => phase.Issues)
                .ThenInclude(issue => issue.Status)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

            if (project == null)
                throw new Exception("Project not found.");

            var completedIssues = project.Phases
                .SelectMany(phase => phase.Issues)
                .Where(issue => issue.Status.IsDone == true &&
                                issue.DueDate.HasValue &&
                                issue.DueDate >= request.StartDate &&
                                issue.DueDate <= request.EndDate)
                .ToList();

            var groupedData = completedIssues
                .GroupBy(issue => new
                {
                    Year = issue.DueDate!.Value.Year,
                    Month = issue.DueDate!.Value.Month
                })
                .Select(group => new TaskCompletionPoint
                {
                    Period = $"{group.Key.Month}/{group.Key.Year}",
                    CompletedTasks = group.Count()
                })
                .ToDictionary(x => x.Period, x => x.CompletedTasks);


            var allMonths = Enumerable.Range(0, ((request.EndDate.Year - request.StartDate.Year) * 12) + request.EndDate.Month - request.StartDate.Month + 1)
                .Select(offset => request.StartDate.AddMonths(offset))
                .Select(date => new TaskCompletionPoint
                {
                    Period = $"{date.Month}/{date.Year}",
                    CompletedTasks = groupedData.ContainsKey($"{date.Month}/{date.Year}") ? groupedData[$"{date.Month}/{date.Year}"] : 0
                })
                .ToList();

            return allMonths;
        }
    }

}
