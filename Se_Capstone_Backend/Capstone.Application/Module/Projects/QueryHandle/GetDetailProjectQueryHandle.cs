using AutoMapper;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using CloudinaryDotNet.Core;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Projects.QueryHandle
{
    public class GetDetailProjectQueryHandle : IRequestHandler<GetDetailProjectQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _jwtService;


        public GetDetailProjectQueryHandle(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ResponseMediator> Handle(GetDetailProjectQuery request, CancellationToken cancellationToken)
        {
            var project = await _unitOfWork.Projects.Find(x => x.Id == request.Id).Include(c => c.Lead).Include(c => c.UserProjects).ThenInclude(c => c.User).FirstOrDefaultAsync();

            if (project == null)
                return new ResponseMediator("Project not found", null, 404);


            var projectMapper = _mapper.Map<ProjectDetailResponse>(project);
            projectMapper.Members = _mapper.Map<List<UserForProjectDetailDTO>>(project.UserProjects.Select(x => x.User));
            foreach (var p in projectMapper.Members)
            {
                User user = _unitOfWork.Users.FindOne(x => x.Id == p.Id);
                if (user != null)
                    p.RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                var userProject = _unitOfWork.UserProjects.Find(x => x.UserId == p.Id && x.ProjectId == request.Id).Include(x => x.Position).FirstOrDefault();
                if(userProject != null)
                {
                    p.IsIssueConfigurator = userProject.IsIssueConfigurator;
                    p.IsProjectConfigurator = userProject.IsProjectConfigurator;
                    p.PositionName = userProject.Position?.Title;
                }
            }

            var myUser = await _jwtService.VerifyTokenAsync(request.Token);
            if (myUser != null)
            {

                var roles = await _userManager.GetRolesAsync(myUser);
                var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name == (roles.FirstOrDefault() == null ? "" : roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();

                if (role != null && role.Name != null && role.Permissions.Select(x => x.Name).Contains("SETTING_DETAIL_ALL_PROJECTS"))
                {
                    projectMapper.MyPermissions = new List<string>() {"IsMemberConfigurator", "IsProjectConfigurator", "IsIssueConfigurator" };
                }else if(project.LeadId == myUser.Id)
                {
                    projectMapper.MyPermissions = new List<string>() { "IsMemberConfigurator", "IsProjectConfigurator", "IsIssueConfigurator" };
                }
                else
                {
                    var userProject = _unitOfWork.UserProjects.Find(x => x.ProjectId == request.Id && x.UserId == myUser.Id).FirstOrDefault();
                    if (userProject != null)
                    {
                        if (userProject.IsIssueConfigurator == true)
                            projectMapper.MyPermissions.Add("IsIssueConfigurator");
                        if (userProject.IsProjectConfigurator == true)
                            projectMapper.MyPermissions.Add("IsProjectConfigurator");
                    }
                }
            }

            foreach(var r in projectMapper.Members)
            {
                var userQuery = _unitOfWork.Users.FindOne(x => x.Id == r.Id);
                if(userQuery != null)
                {
                    var roles = await _userManager.GetRolesAsync(userQuery);
                    var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name == (roles.FirstOrDefault() == null ? "" : roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();

                    if (role != null && role.Name != null && role.Permissions.Select(x => x.Name).Contains("SETTING_DETAIL_ALL_PROJECTS"))
                    {
                       r.IsConfigurator = true;
                    }
                }

            }

            return new ResponseMediator("", projectMapper);
        }
    }
}
