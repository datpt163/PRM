using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Projects.QueryHandle
{
    public class GetReportTaskQueryHandler : IRequestHandler<GetReportTaskQuery, ResponseReportTask>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetReportTaskQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseReportTask> Handle(GetReportTaskQuery request, CancellationToken cancellationToken)
        {
            var project = await _unitOfWork.Projects.GetQueryNoTracking()
                .Include(x=> x.Lead)
                .Include(x => x.UserProjects)
                .ThenInclude(up => up.User)
                .Include(x => x.Statuses)
                .ThenInclude(x => x.Issues)
                .ThenInclude(x => x.Assignee)
                .Where(x => x.Id == request.ProjectId)
                .FirstOrDefaultAsync();

            if (project == null)
            {
                throw new Exception("Project does not exist!");
            }

            var issues = project.Statuses
                .SelectMany(status => status.Issues)
                .Where(issue => (!request.StartDate.HasValue || issue.StartDate >= request.StartDate) &&
                                (!request.EndDate.HasValue || issue.DueDate <= request.EndDate)&&
                                issue.Assignee != null)
                .ToList();

            var statuses = project.Statuses.Select(status => new StatusReport
            {
                Id = status.Id,
                StatusName = status.Name
            }).ToList();

            var userProjects = project.UserProjects.Select(x=> x.User).ToList();
            if(project.Lead!=null) userProjects.Add(project.Lead);

            var userReports = userProjects
                .Where(user => user != null)
                .Select(user => new UserReport
                {
                    UserId = user!.Id,
                    FullName = user.FullName,
                    UserTaskStatuses = issues
                        .Where(issue => issue.Assignee != null && issue.Assignee.Id == user.Id)
                        .GroupBy(issue => issue.Status)
                        .Select(statusGroup => new UserTaskStatus
                        {
                            StatusId = statusGroup.Key!.Id,
                            Total = statusGroup.Count()
                        }).ToList()
                })
                .ToList();

            return new ResponseReportTask
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Users = userReports,
                Statuses = statuses
            };
        }
    }

}
