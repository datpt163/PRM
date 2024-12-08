using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
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
    public class GetListMemberQueryHandle : IRequestHandler<GetListMemberQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetListMemberQueryHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(GetListMemberQuery request, CancellationToken cancellationToken)
        {
            var member = new List<User?>();
            var project = await _unitOfWork.Projects.Find(x => x.Id == request.ProjectId)
                                              .Include(x => x.Lead).Include(x => x.UserProjects).ThenInclude(x => x.User)
                                              .Include(x => x.Statuses).ThenInclude(c => c.Issues).ThenInclude(c => c.Assignee)
                                              .Include(x => x.Statuses).ThenInclude(c => c.Issues).ThenInclude(c => c.Reporter)
                                              .FirstOrDefaultAsync();
            if (project == null)
                return new ResponseMediator(Messages.project_not_found, null, 404);

            var assinees = project.Statuses.SelectMany(x => x.Issues.Where(x => x.Assignee != null)).Select(x => x.Assignee).ToList();
            var reporters = project.Statuses.SelectMany(x => x.Issues.Where(x => x.Reporter != null)).Select(x => x.Reporter).ToList();
            if (assinees != null)
                member.AddRange(assinees);
            if (reporters != null)
                member.AddRange(reporters);
            member.Add(project.Lead);
            member.AddRange(project.UserProjects.Select(x => x.User));
            member = member.DistinctBy(x => x?.Id).ToList();
            return new ResponseMediator("", member.Where(x => x != null).Select(x => new
            {
                Id = x?.Id,
                UserName = x?.UserName,
                Avatar = x?.Avatar,
            }));
        }
    }
}
