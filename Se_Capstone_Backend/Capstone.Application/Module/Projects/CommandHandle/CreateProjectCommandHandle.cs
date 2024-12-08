using AutoMapper;
using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.FileService;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Projects.Command;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Text.Json;
using Capstone.Application.Common.EmailHTML;
using MassTransit;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Resources;
namespace Capstone.Application.Module.Projects.CommandHandle
{
    public class CreateProjectCommandHandle : IRequestHandler<CreateProjectCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mappper;
        private readonly IFileService _fileService;
        private readonly IPublishEndpoint _publisher;
        private readonly IJwtService _jwtService;
        public CreateProjectCommandHandle(IUnitOfWork unitOfWork, UserManager<User> userManager, IMapper mapper, IFileService fileService, IPublishEndpoint publisher, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mappper = mapper;
            _fileService = fileService;
            _publisher = publisher;
        }
        public async Task<ResponseMediator> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var userAssign = await _jwtService.VerifyTokenAsync(request.Token);
            if(userAssign == null)
                return new ResponseMediator(Messages.user_not_found, null);

            int statusCodeSuccess = 200;
            var toEmail = "";
            var project = _unitOfWork.Projects.Find(p => p.Code.Trim().ToUpper().Equals(request.Code.Trim().ToUpper())).FirstOrDefault();

            if (project != null)
                return new ResponseMediator(Messages.project_code_exists, null);

            //if (request.StartDate.Date < DateTime.Now.Date || request.EndDate.Date < DateTime.Now.Date)
            //    return new ResponseMediator("Start date and end date must be greater than the current time", null);

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                if (request.EndDate.Value.Date < request.StartDate.Value.Date)
                    return new ResponseMediator(Messages.end_date_greater_than_start_date, null);
            }
          
            UserDTO userDto = new UserDTO();
            if (request.LeadId != null)
            {
                var user = _unitOfWork.Users.Find(u => u.Id == request.LeadId).FirstOrDefault();
                if (user == null)
                    return new ResponseMediator(Messages.team_lead_not_found, null, 404);
                // var roles = await _userManager.GetRolesAsync(user);
                // if (roles == null || roles.Count == 0)
                //     return new ResponseMediator("This user not have lead project permission to become a leader", null);

                // var role = _unitOfWork.Roles.Find(x => x.Name != null && x.Name.Equals(roles.FirstOrDefault())).Include(c => c.Permissions).FirstOrDefault();
                // if (role == null)
                //     return new ResponseMediator("This user not have role to become a leader", null);

                // if (!role.Permissions.Any(x => x.Name == "IS_PROJECT_LEAD"))
                //     return new ResponseMediator("This user not have role to become a leader", null);

                userDto.Id = user.Id;
                userDto.Name = user.FullName;
                toEmail = user.Email;
                if(user.Id != userAssign.Id)
                statusCodeSuccess = 205;
            }


            var projectCreate = new Project(request.Name.Trim(), request.Code.Trim(), request.Description, request.StartDate, request.EndDate, request.LeadId, false);
            projectCreate.TotalEffort = request.TotalEffort;
            if (request.IsVisible != null)
                projectCreate.IsVisible = request.IsVisible.Value;

            _unitOfWork.Projects.Add(projectCreate);
            await _unitOfWork.SaveChangesAsync();
            if (statusCodeSuccess == 205)
            {
                _unitOfWork.Notifications.Add(new Notification() { CreatedAt = DateTime.Now, UserId = userDto.Id, Type = "assignLeader", Data = JsonSerializer.Serialize(new { type = "assignLeader", projectId = projectCreate.Id, projectName = request.Name, assignerName = userAssign.FullName, assignerUserName = userAssign.UserName, assignerAvatar = userAssign.Avatar }) });
                await _publisher.Publish(new SendEmailMessage() { ToEmail = toEmail == null ? "" : toEmail, Body = EmailMessage.AssignLeader(request.Name, userDto.Name, projectCreate.Id), Subject = $"🎉 congratulations! you’ve been assigned as the project leader for {request.Name}" });
            }
            var defaultStatueses = await CreateDefaultStatus(projectCreate.Id);
            var listDefaultStatus = new List<Domain.Entities.Status>();
            foreach (var s in defaultStatueses)
                listDefaultStatus.Add(new Domain.Entities.Status() { Name = s.Name, Position = s.Position, Description = s.Description, ProjectId = s.ProjectId, Color = s.Color, IsDone = s.IsDone });
            _unitOfWork.Statuses.AddRange(listDefaultStatus);
            var defaultLabels = await CreateDefaultLabels(projectCreate.Id);
            var listDefaultLabels = new List<Label>();
            foreach (var s in defaultLabels)
                listDefaultLabels.Add(new Label() { Title = s.Title, Description = s.Description, ProjectId = s.ProjectId,});
            _unitOfWork.Labels.AddRange(listDefaultLabels);
            await _unitOfWork.SaveChangesAsync();

            var response = _mappper.Map<ProjectDTO>(projectCreate);
            response.LeadId = userDto.Id;
            response.LeadName = userDto.Name; 
            return new ResponseMediator(userDto.Id + "", response, statusCodeSuccess);
        }

        public async Task<List<Domain.Entities.Status>> CreateDefaultStatus(Guid projectId)
        {
            string module = "Module";
            string project = "Projects";
            string folder = "Default";
            string fileName = "DefaultStatus.json";
            string path = Path.Combine(module, project, folder, fileName);
            var fileContent = await _fileService.ReadFileAsync(path);

            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent), Messages.file_content_cannot_be_null);

            var statuses = JsonSerializer.Deserialize<List<Domain.Entities.Status>>(fileContent) ?? new List<Domain.Entities.Status>();
            foreach (var s in statuses)
                s.ProjectId = projectId;
            return statuses;
        }


        public async Task<List<Label>> CreateDefaultLabels(Guid projectId)
        {
            string module = "Module";
            string project = "Projects";
            string folder = "Default";
            string fileName = "DefaultLabel.json";
            string path = Path.Combine(module, project, folder, fileName);
            var labels = JsonSerializer.Deserialize<List<Label>>(await _fileService.ReadFileAsync(path)) ?? new List<Label>();
            foreach (var s in labels)
                s.ProjectId = projectId;
            return labels;
        }
    }
}
