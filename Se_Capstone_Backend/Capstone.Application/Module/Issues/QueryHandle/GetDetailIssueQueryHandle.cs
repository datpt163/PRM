using AutoMapper;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Issues.DTO;
using Capstone.Application.Module.Issues.Query;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Issues.QueryHandle
{
    public class GetDetailIssueQueryHandle : IRequestHandler<GetDetailIssueQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        public GetDetailIssueQueryHandle(IUnitOfWork unitOfWork, IMapper mapper, IJwtService jwtService, UserManager<User> userManager)
        {
            _jwtService = jwtService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ResponseMediator> Handle(GetDetailIssueQuery request, CancellationToken cancellationToken)
        {
            var myUser = await _jwtService.VerifyTokenAsync(request.Token);
           
            var issue = await _unitOfWork.Issues.Find(x => x.Id == request.Id).Include(c => c.Phase).Include(c => c.ParentIssue).Include(c => c.Label).Include(c => c.Status).ThenInclude(c => c.Project).ThenInclude(c => c.UserProjects).Include(c => c.LastUpdateBy).Include(c => c.Reporter).Include(c => c.Assignee).Include(c => c.SubIssues).ThenInclude(c => c.Assignee).Include(c => c.SubIssues).ThenInclude(c => c.Status).Include(c => c.Comments).ThenInclude(c => c.User).FirstOrDefaultAsync();
            if(issue == null)
                return new ResponseMediator(Messages.parent_issue_not_found, null, 404);

            if (myUser != null)
            {

                var roles = await _userManager.GetRolesAsync(myUser);
                var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name == (roles.FirstOrDefault() == null ? "" : roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();
                var project = issue.Status.Project;
                if (!(role != null && role.Name != null && role.Permissions.Select(x => x.Name).Contains("SETTING_DETAIL_ALL_PROJECTS")))
                {
                    if (!(project.LeadId == myUser.Id || project.UserProjects.Select(x => x.UserId).Contains(myUser.Id)))
                        return new ResponseMediator("", null, 403);
                }
            }
            var response = _mapper.Map<IssueDTO2?>(issue);
            return new ResponseMediator("", response);

        }
    }
}
