using AutoMapper;
using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.EmailHTML;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Projects.Command;
using Capstone.Application.Module.Projects.Response;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Repository;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Capstone.Application.Module.Projects.CommandHandle
{
    public class UpdateProjectCommandHandle : IRequestHandler<UpdateProjectCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IPublishEndpoint _publisher;
        private readonly IJwtService _jwtService;
        public UpdateProjectCommandHandle(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager, IPublishEndpoint publishEndpoint, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _publisher = publishEndpoint;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ResponseMediator> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var userAssign = await _jwtService.VerifyTokenAsync(request.Token);
            if (userAssign == null)
                return new ResponseMediator(Messages.user_not_found, null);
            int statusCodeSuccess = 200;
            if (!(request.Status == ProjectStatus.NotStarted || request.Status == ProjectStatus.InProgress || request.Status == ProjectStatus.Finished || request.Status == ProjectStatus.Canceled))
                return new ResponseMediator(Messages.status_not_valid, null);

            var projectCheckCode = _unitOfWork.Projects.Find(p => p.Code.Trim().ToUpper().Equals(request.Code.Trim().ToUpper()) && request.Id != p.Id).FirstOrDefault();

            if (projectCheckCode != null)
                return new ResponseMediator(Messages.project_code_exists, null);

            if (request.EndDate.HasValue && request.StartDate.HasValue)
                if (request.EndDate.Value.Date < request.StartDate.Value.Date)
                    return new ResponseMediator(Messages.end_date_greater_than_start_date, null);


            var project = _unitOfWork.Projects.Find(x => x.Id == request.Id).Include(c => c.Lead).FirstOrDefault();
            if (project == null)
                return new ResponseMediator(Messages.project_not_found, null, 404);

            if (request.TeamLeadId != null)
            {
                var user = _unitOfWork.Users.Find(u => u.Id == request.TeamLeadId).FirstOrDefault();
                if (user == null)
                    return new ResponseMediator(Messages.team_lead_not_found, null, 404);
                if ((project.LeadId == null || project.LeadId != request.TeamLeadId) && request.TeamLeadId != userAssign.Id)
                {
                    _unitOfWork.Notifications.Add(new Notification() { CreatedAt = DateTime.Now, UserId = user.Id, Type = "assignLeader", Data = JsonSerializer.Serialize(new { type = "assignLeader", projectId = request.Id, projectName = request.Name, assignerName = userAssign.FullName, assignerUserName = userAssign.UserName, assignerAvatar = userAssign.Avatar }) });
                    await _publisher.Publish(new SendEmailMessage() { ToEmail = user.Email == null ? "" : user.Email, Body = EmailMessage.AssignLeader(request.Name, user.UserName == null ? "" : user.UserName, request.Id), Subject = $"🎉 congratulations! you’ve been assigned as the project leader for {request.Name}" });
                    statusCodeSuccess = 205;
                }
                //var roles = await _userManager.GetRolesAsync(user);
                //if (roles == null || roles.Count == 0)
                //    return new ResponseMediator("This user not have role to become a leader", null);

                //var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name.Equals(roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();
                //if (role == null)
                //    return new ResponseMediator("This user not have role to become a leader", null);

                //if (!role.Permissions.Any(x => x.Name == "IS_PROJECT_LEAD"))
                //    return new ResponseMediator("This user not have role to become a leader", null);
            }

            project.Name = request.Name.Trim();
            project.Code = request.Code.Trim();
            project.Description = request.Description;
            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;
            project.LeadId = request.TeamLeadId;
            project.UpdatedAt = DateTime.Now;

            //Check Status
            if ((project.Status != ProjectStatus.Finished && request.Status == ProjectStatus.Finished))
            {
                var projectPhaseQuery = _unitOfWork.Projects.GetQueryNoTracking()
                    .Include(x => x.Phases)
                    .ThenInclude(x => x.Issues)
                    .ThenInclude(x => x.Status)
                    .Where(x => !x.IsDeleted);

                var projectStatusesQuery = _unitOfWork.Projects.GetQueryNoTracking()
                    .Include(x => x.Statuses)
                    .ThenInclude(x => x.Issues)
                    .Where(x => !x.IsDeleted);

                var hasUndoneIssues = await projectPhaseQuery
                    .SelectMany(p => p.Phases)
                    .SelectMany(ph => ph.Issues)
                    .AnyAsync(i => i.Status.IsDone == false || i.Status.IsDone == null)
                     || await projectStatusesQuery
                    .SelectMany(p => p.Statuses)
                    .SelectMany(s => s.Issues)
                    .AnyAsync(i => i.Status.IsDone == false || i.Status.IsDone == null);


                if (hasUndoneIssues)
                {
                    return new ResponseMediator("There is some task need change status to done!!", null);
                }

            }

            project.LeadId = request.TeamLeadId;
            project.TotalEffort = request.TotalEffort;
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
            var response = _mapper.Map<ProjectDTO>(project);
            return new ResponseMediator(request.TeamLeadId + "", response, statusCodeSuccess);
        }
    }
}
