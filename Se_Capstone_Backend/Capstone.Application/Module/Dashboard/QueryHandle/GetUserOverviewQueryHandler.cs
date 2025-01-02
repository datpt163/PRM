using Capstone.Application.Module.Dashboard.Query;
using Capstone.Application.Module.Dashboard.Response;
using Capstone.Application.Resources;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Dashboard.QueryHandle
{
    public class GetUserOverviewQueryHandler : IRequestHandler<GetUserOverviewQuery, UserOverviewResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserOverviewQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserOverviewResponse> Handle(GetUserOverviewQuery request, CancellationToken cancellationToken)
        {

            var user = await _unitOfWork.Users
                .GetQueryNoTracking()
                .Include(x => x.Skills)
                .Include(x => x.LeadProjects)
                .Include(x=> x.UserProjects)
                .ThenInclude(x=> x.Project)
                //.ThenInclude(x=> x.Status)
                .Include(x => x.AssinedIssues)
                .ThenInclude(x=> x.Status)
                .ThenInclude(x=> x.Project)
                .FirstOrDefaultAsync(x=> x.Id == request.UserId,cancellationToken);

            if(user == null) {
                throw new Exception(Messages.user_not_found);
            }
            var totalSkills = 0;
            if (user.Skills != null)
            {
                totalSkills = user.Skills.Where(x => !x.IsDeleted).Count();

            }
            var totalTasks = user.AssinedIssues.Where(x=> !x.IsDeleted).Count();
            var totalCurrentTasks = user.AssinedIssues.Where(x => !x.IsDeleted && (x.Status.IsDone == false || x.Status.IsDone == null)).Count();
            var totalTasksDone = user.AssinedIssues.Where(x => !x.IsDeleted && x.Status.IsDone == true ).Count();
            var totalProjects = user.LeadProjects.Count() + user.UserProjects.Count();
            var totalProjectsLead = user.LeadProjects.Count();
            var totalCurrentProjects = user.LeadProjects.Where(x=> x.Status != Domain.Enums.ProjectStatus.Finished).Count()
                                    + user.UserProjects.Where(x => x.Project.Status != Domain.Enums.ProjectStatus.Finished).Count();

            var overViewTasks = new List<OverViewTask>();
            if (totalCurrentTasks > 0)
            {
                var currentTask = user.AssinedIssues.Where(x => !x.IsDeleted && (x.Status.IsDone == false || x.Status.IsDone == null)).ToList();
                foreach (var task in currentTask)
                {
                    overViewTasks.Add(new OverViewTask
                    {
                        TaskId = task.Id,
                        TaskName = task.Title,
                        ProjectId = task.Status.ProjectId,
                        ProjectName = task.Status.Project.Name,
                        UserId = request.UserId,
                        UserName = user.FullName,
                        StatusName = task.Status.Name,
                        Color = task.Status.Color,
                        Index = task.Index
                    });
                }
            }
            overViewTasks = overViewTasks.OrderBy(x => x.Index).ToList();

            return new UserOverviewResponse
            {
                TotalSkills = totalSkills,
                TotalTasks = totalTasks,
                TotalCurrentTasks = totalCurrentTasks,
                TotalTasksDone = totalTasksDone,
                TotalProjects = totalProjects,
                TotalProjectsLead = totalProjectsLead,
                TotalCurrentProjects = totalCurrentProjects,
                OverViewTasks = overViewTasks
            };

        }
    }
}
