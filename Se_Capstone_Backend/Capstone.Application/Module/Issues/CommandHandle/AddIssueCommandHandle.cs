using AutoMapper;
using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.EmailHTML;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ProjectAuthorize;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Issues.Command;
using Capstone.Application.Module.Issues.ConsumerRabbitMq;
using Capstone.Application.Module.Issues.ConsumerRabbitMq.Message;
using Capstone.Application.Module.Issues.DTO;
using Capstone.Application.Module.Status.ConsumerRabbitMq.Message;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Redis;
using Capstone.Infrastructure.Repository;
using MassTransit;
using MassTransit.RabbitMqTransport;
using MassTransit.Transports;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Reflection.Emit;
using System.Text.Json;


namespace Capstone.Application.Module.Issues.CommandHandle
{
    public class AddIssueCommandHandle : IRequestHandler<AddIssueCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly RedisContext _redisContext;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publisher;
        private readonly IRequestClient<AddIssueMessage2> _requestClient;
        private readonly IManagePermissionProject _managePermissionProject;  
        public AddIssueCommandHandle(IPublishEndpoint publishEndpoint, IUnitOfWork unitOfWork, IJwtService jwtService, RedisContext redisContext, IMapper mapper, IRequestClient<AddIssueMessage2> requestClient, IManagePermissionProject managePermissionProject)
        {
            _requestClient = requestClient;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _redisContext = redisContext;
            _publisher = publishEndpoint;
            _managePermissionProject = managePermissionProject;
        }

        public async Task<ResponseMediator> Handle(AddIssueCommand request, CancellationToken cancellationToken)
        {
            int responseSuccess = 200;
            var userAssignee = new User();
            if (string.IsNullOrEmpty(request.Title))
                return new ResponseMediator(Messages.title_empty, null, 400);

            if (request.StartDate.HasValue && request.DueDate.HasValue && request.StartDate.Value.Date > request.DueDate.Value.Date)
                return new ResponseMediator(Messages.end_date_greater_than_start_date, null, 400);

            if (request.Priority.HasValue && ((int)request.Priority < 1 || (int)request.Priority > 5))
                return new ResponseMediator(Messages.priority_range, null, 400);

            if (request.EstimatedTime.HasValue && request.EstimatedTime.Value <= 0)
                return new ResponseMediator(Messages.estimated_time_greater_than_zero, null, 400);

            if (request.ParentIssueId.HasValue)
                if (_unitOfWork.Issues.FindOne(x => x.Id == request.ParentIssueId.Value) == null)
                    return new ResponseMediator(Messages.parent_issue_not_found, null, 404);

            if (request.LabelId.HasValue && _unitOfWork.Labels.FindOne(x => x.Id == request.LabelId) == null)
                return new ResponseMediator(Messages.label_not_found, null, 404);
            var status = _unitOfWork.Statuses.Find(x => x.Id == request.StatusId).Include(c => c.Project).ThenInclude(c => c.Phases).Include(c => c.Issues).FirstOrDefault();
            if (status == null)
                return new ResponseMediator(Messages.status_not_valid, null, 404);

            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new ResponseMediator(Messages.user_not_found, null, 404);

            (List<string> permissions, int authorizeProjectCode) = await _managePermissionProject.GetPermissionAsync(request.Token, statusId: request.StatusId, option: PermissionCode.CheckMember);
            if (authorizeProjectCode != PermissionCode.IsMember && authorizeProjectCode != PermissionCode.IsLeader && authorizeProjectCode != PermissionCode.IsSettingAllProjectConfigurator)
                return new ResponseMediator("", null, 403);

            if (request.AssignedToId.HasValue)
            {
                var assignee = _unitOfWork.Users.FindOne(x => x.Id == request.AssignedToId);
                if (assignee == null)
                    return new ResponseMediator(Messages.user_not_found, null, 404);
                if(assignee.Id != user.Id)
                    responseSuccess = 205;
                userAssignee = assignee;
            }


            var lastUpdateById = user.Id;
            var assignedById = user.Id;
            var index = SetIndex(status.Project.Id);

            Guid? phaseId = null;
            var result = status.Project.GetStatusPhaseOfProject();
            if ((result.status == PhaseStatus.Running || result.status == PhaseStatus.Complete) && result.phaseRunning != null)
                phaseId = result.phaseRunning.Id;

            var issue = new Issue()
            {
                Index = index,
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                DueDate = request.DueDate,
                Priority = request.Priority,
                EstimatedTime = request.EstimatedTime,
                ParentIssueId = request.ParentIssueId,
                ReporterId = assignedById,
                AssigneeId = request.AssignedToId,
                LastUpdateById = lastUpdateById,
                StatusId = request.StatusId,
                LabelId = request.LabelId,
                PhaseId = phaseId
            };
            //var issueMessage = new AddIssueMessage2
            //{
            //    Index = index,
            //    Title = request.Title,
            //    Description = request.Description,
            //    StartDate = request.StartDate,
            //    DueDate = request.DueDate,
            //    Priority = request.Priority,
            //    EstimatedTime = request.EstimatedTime,
            //    ParentIssueId = request.ParentIssueId,
            //    ReporterId = assignedById,
            //    AssigneeId = request.AssignedToId,
            //    LastUpdateById = lastUpdateById,
            //    StatusId = request.StatusId,
            //    LabelId = request.LabelId,
            //    PhaseId = phaseId
            //};

            //var response2 = await _requestClient.GetResponse<UserResponse>(issueMessage);

         
            var position = status.Issues.Where(x => x.ParentIssue == null).Count();
            issue.Position = position;
            _unitOfWork.Issues.Add(issue);
            await _unitOfWork.SaveChangesAsync();
            var response = _mapper.Map<IssueDTO>(issue);
            if(responseSuccess == 205)
            {
                _unitOfWork.Notifications.Add(new Notification() { CreatedAt = DateTime.Now, 
                                                                   UserId = userAssignee.Id, 
                                                                   Type = "assignIssue", 
                                                                   Data = JsonSerializer.Serialize(new { type = "assignIssue", assignerName = user.FullName, assignerUsername = user.UserName, assignerAvatar = user.Avatar, projectId = status.ProjectId, issueName = request.Title, issueId = issue.Id, issueIndex = index, issueStatusName = status.Name}) });
                await _unitOfWork.SaveChangesAsync();
                await _publisher.Publish(new SendEmailMessage() { ToEmail = userAssignee.Email == null ? "" : userAssignee.Email, Body = EmailMessage.AssignIssue(request.Title, request.Description, request.StartDate, request.DueDate, issue.Id + "", status.ProjectId + ""), Subject = $"[ {status.Project.Name} ]You are assigned to issue {request.Title}" });
            }
            var ids = new List<Guid>();
            ids.Add(userAssignee.Id);
            ids.Add(status.ProjectId);
            return new ResponseMediator(JsonSerializer.Serialize(ids), response, responseSuccess);
        }


        public int SetIndex(Guid ProjectId)
        {
            var index = _redisContext.GetData<int>("IndexProject" + ProjectId);
            index++;
            _redisContext.SetData("IndexProject" + ProjectId, index, DateTime.Now.AddYears(1));
            return index;
        }
    }
}
