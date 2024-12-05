using Capstone.Application.Common.Jwt;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Common.ProjectAuthorize
{
    public interface IManagePermissionProject
    {
        public Task<bool> IsAuthorizedAsync(string token, Project project, string typeAuthorize);
    }
    public class ManagePermissionProject : IManagePermissionProject
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        public ManagePermissionProject(IUnitOfWork unitOfWork, IJwtService jwtService, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _userManager = userManager;
        }

        public async Task<bool> IsAuthorizedAsync(string token, Project project, string typeAuthorize)
        {
            var permissions = await GetPermissionAsync(token, project);

            if(permissions.Contains(typeAuthorize)) 
                return true;
            return false;
        }

        public async Task<List<string>> GetPermissionAsync(string token, Project project)
        {
            var permissions = new List<string>();

            var myUser = await _jwtService.VerifyTokenAsync(token);
            if (myUser != null)
            {

                var roles = await _userManager.GetRolesAsync(myUser);
                var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name == (roles.FirstOrDefault() == null ? "" : roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();

                if (role != null && role.Name != null && role.Permissions.Select(x => x.Name).Contains("SETTING_DETAIL_ALL_PROJECTS"))
                {
                    permissions = new List<string>() { "IsMemberConfigurator", "IsProjectConfigurator", "IsIssueConfigurator", "IsCommentConfigurator" };
                }
                else
                {
                    if (project.LeadId == myUser.Id)
                    {
                        permissions = new List<string>() { "IsMemberConfigurator", "IsProjectConfigurator", "IsIssueConfigurator", "IsCommentConfigurator" };
                    }
                    else
                    {
                        var userProject = _unitOfWork.UserProjects.Find(x => x.ProjectId == project.Id && x.UserId == myUser.Id).FirstOrDefault();
                        if (userProject != null)
                        {
                            if (userProject.IsIssueConfigurator == true)
                                permissions.Add("IsIssueConfigurator");
                            if (userProject.IsProjectConfigurator == true)
                                permissions.Add("IsProjectConfigurator");
                            if (userProject.IsCommentConfigurator == true)
                                permissions.Add("IsCommentConfigurator");
                        }
                    }
                }
            }
            return permissions;
        }
    }
}
