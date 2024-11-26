using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.EmailHTML;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Projects.Command;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.Transports;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Capstone.Application.Module.Projects.CommandHandle
{
    public class AddMembersToProjectCommandHandle : IRequestHandler<AddMembersToProject, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publisher;
        private readonly IJwtService _jwtService;
        public AddMembersToProjectCommandHandle(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _publisher = publishEndpoint;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(AddMembersToProject request, CancellationToken cancellationToken)
        {
            var userAsign = await _jwtService.VerifyTokenAsync(request.Token);
            if(userAsign == null)
                return new ResponseMediator("User not found", null, 404);

            var project = _unitOfWork.Projects.Find(x => x.Id == request.ProjectId).Include(c => c.UserProjects).FirstOrDefault();
            if (project == null)
                return new ResponseMediator("Project not found", null, 404);

            var userIds = request.MemberIds.Except(project.UserProjects.Select(x => x.UserId).ToList());
            foreach (var userId in userIds)
            {
                var user = _unitOfWork.Users.FindOne(x => x.Id == userId);
                if (user != null)
                {
                    _unitOfWork.Notifications.Add(new Notification() { CreatedAt = DateTime.Now, UserId = user.Id, Type = "assignMember", Data = JsonSerializer.Serialize(new { type = "assignMember", projectId = project.Id, projectName = project.Name, assignerName = userAsign.FullName, assignerUsername = userAsign.UserName, assignerAvatar = userAsign.Avatar }) });
                    await _publisher.Publish(new SendEmailMessage() { ToEmail = user.Email == null ? "" : user.Email, Body = EmailMessage.AssignMember(project.Name, user.UserName == null ? "" : user.UserName, project.Description, project.Id), Subject = $"Welcome to the {project.Name} Team!" });
                }
            }

            project.UserProjects = new List<UserProject>();
            foreach (var s in request.MemberIds)
            {
                var staff = _unitOfWork.Users.FindOne(x => x.Id == s);
                if (staff == null)
                    return new ResponseMediator("Member not found", null, 404);
                if(staff.Id != project.LeadId)
                    project.UserProjects.Add(new UserProject() { ProjectId = request.ProjectId, UserId = staff.Id, IsIssueConfigurator = false, IsProjectConfigurator = false});
            }
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator(JsonSerializer.Serialize(userIds), null, 200);
        }
    }
}
