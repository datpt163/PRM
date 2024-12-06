using Capstone.Application.Common.Jwt;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
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
        public Task<(bool,int)> IsAuthorizedAsync(string token, string typeAuthorize, Guid? projectId = null, Guid? issueId = null, Guid? phaseId = null, Guid? labelId = null, Guid? statusId = null, Guid? commentId = null, string? option = null);
        public Task<(List<string>, int)> GetPermissionAsync(string token, Guid? projectId = null, Guid? issueId = null, Guid? phaseId = null, Guid? labelId = null, Guid? statusId = null, Guid? commentId = null, string? option = null);
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

        public async Task<(bool,int)> IsAuthorizedAsync(string token, string typeAuthorize, Guid? projectId = null, Guid? issueId = null, Guid? phaseId = null, Guid? labelId = null, Guid? statusId = null, Guid? commentId = null, string? option = null)
        {
            (List<string> permissions, int statusCode ) = await GetPermissionAsync(token, projectId: projectId, issueId: issueId, phaseId: phaseId, labelId: labelId, statusId: statusId, commentId: commentId, option: option);

            if (statusCode == 404)
                return (false, 404);
            else
            {
                if (permissions.Contains(typeAuthorize))
                    return (true,200);
                return (false,200);
            }
        }

        public async Task<(List<string>,int)> GetPermissionAsync(string token, Guid? projectId = null, Guid? issueId = null, Guid? phaseId = null, Guid? labelId = null, Guid? statusId = null, Guid? commentId = null, string? option = null)
        {
            var permissions = new List<string>();
            Project? project = new Project();
            if(projectId != null)
            {
                project = _unitOfWork.Projects.Find(x => x.Id == projectId).Include(c => c.UserProjects).FirstOrDefault();
                if (project == null)
                    return (permissions, 404);
            }

            if(issueId != null)
            {
                var issue = _unitOfWork.Issues.Find(x => x.Id == issueId).Include(c => c.Status).ThenInclude(c => c.Project).ThenInclude(c => c.UserProjects).FirstOrDefault();
                if (issue == null)
                    return (permissions, 404);

                project = issue.Status.Project;
            }

            if (phaseId != null)
            {
                var phase = _unitOfWork.Phases.Find(x => x.Id == phaseId).Include(c => c.Project).ThenInclude(c => c.UserProjects).FirstOrDefault();
                if (phase == null)
                    return (permissions, 404);

                project = phase.Project;
            }

            if (statusId != null)
            {
                var status = _unitOfWork.Statuses.Find(x => x.Id == statusId).Include(c => c.Project).ThenInclude(c => c.UserProjects).FirstOrDefault();
                if (status == null)
                    return (permissions, 404);

                project = status.Project;
            }


            if (labelId != null)
            {
                var label = _unitOfWork.Labels.Find(x => x.Id == labelId).Include(c => c.Project).ThenInclude(c => c.UserProjects).FirstOrDefault();
                if (label == null)
                    return (permissions, 404);

                project = label.Project;
            }


            if (commentId != null)
            {
                var comment = _unitOfWork.Comments.Find(x => x.Id == commentId).Include(c => c.Issue).ThenInclude(c => c.Status).ThenInclude(c => c.Project).ThenInclude(c => c.UserProjects).FirstOrDefault();
                if (comment == null)
                    return (permissions, 404);

                project = comment.Issue.Status.Project;
            }

            var myUser = await _jwtService.VerifyTokenAsync(token);
            if (myUser != null)
            {

                var roles = await _userManager.GetRolesAsync(myUser);
                var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name == (roles.FirstOrDefault() == null ? "" : roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();

                if (role != null && role.Name != null && role.Permissions.Select(x => x.Name).Contains("SETTING_DETAIL_ALL_PROJECTS"))
                {
                    if (option != null && option == PermissionCode.CheckMember)
                        return (permissions, PermissionCode.IsSettingAllProjectConfigurator);
                    permissions = new List<string>() { "IsMemberConfigurator", "IsProjectConfigurator", "IsIssueConfigurator", "IsCommentConfigurator" };
                }
                else
                {
                    if (option != null && option == PermissionCode.CheckMember)
                    {
                        if (project.LeadId == myUser.Id)
                            return (permissions, PermissionCode.IsLeader);
                        if(project.UserProjects.Select(x => x.UserId).Contains(myUser.Id))
                            return (permissions, PermissionCode.IsMember);
                        return (permissions, PermissionCode.NotHavePermission);
                    }

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
            return (permissions, 200);
        }
    }

    public static class PermissionCode
    {
        public static int NotHavePermission = 502;
        public static int IsMember = 503;
        public static int IsLeader = 504;
        public static int IsSettingAllProjectConfigurator = 504;
        public static string CheckMember = "CHECK_MEMBER";
    }
}
