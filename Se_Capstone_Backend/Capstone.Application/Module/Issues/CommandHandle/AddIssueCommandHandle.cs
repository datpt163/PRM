using AutoMapper;
using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.EmailHTML;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Issues.Command;
using Capstone.Application.Module.Issues.ConsumerRabbitMq;
using Capstone.Application.Module.Issues.ConsumerRabbitMq.Message;
using Capstone.Application.Module.Issues.DTO;
using Capstone.Application.Module.Status.ConsumerRabbitMq.Message;
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
        public readonly IPublishEndpoint _publisher;
        private readonly IRequestClient<AddIssueMessage2> _requestClient;

        //public AddIssueCommandHandle(IUnitOfWork unitOfWork, IJwtService jwtService, RedisContext redisContext, IMapper mapper, IPublishEndpoint publishEndpoint)
        //{
        //    _publishEndpoint = publishEndpoint;
        //    _mapper = mapper;
        //    _unitOfWork = unitOfWork;
        //    _jwtService = jwtService;
        //    _redisContext = redisContext;
        //}

        public AddIssueCommandHandle(IPublishEndpoint publishEndpoint, IUnitOfWork unitOfWork, IJwtService jwtService, RedisContext redisContext, IMapper mapper, IRequestClient<AddIssueMessage2> requestClient)
        {
            _requestClient = requestClient;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _redisContext = redisContext;
            _publisher = publishEndpoint;
        }

        public async Task<ResponseMediator> Handle(AddIssueCommand request, CancellationToken cancellationToken)
        {
            int responseSuccess = 200;
            var userAssignee = new User();
            if (string.IsNullOrEmpty(request.Title))
                return new ResponseMediator("Title empty", null, 400);

            if (request.StartDate.HasValue && request.DueDate.HasValue && request.StartDate.Value.Date > request.DueDate.Value.Date)
                return new ResponseMediator("Due date must greater or equal start date", null, 400);

            if (request.Priority.HasValue && ((int)request.Priority < 1 || (int)request.Priority > 5))
                return new ResponseMediator("Priority must be between 1 and 5", null, 400);

            if (request.EstimatedTime.HasValue && request.EstimatedTime.Value <= 0)
                return new ResponseMediator("Estimated time must be greater than 0 hour", null, 400);

            if (request.ParentIssueId.HasValue)
                if (_unitOfWork.Issues.FindOne(x => x.Id == request.ParentIssueId.Value) == null)
                    return new ResponseMediator("Parent issue not found", null, 404);

            if (request.LabelId.HasValue && _unitOfWork.Labels.FindOne(x => x.Id == request.LabelId) == null)
                return new ResponseMediator("Label  not found", null, 404);

            var status = _unitOfWork.Statuses.Find(x => x.Id == request.StatusId).Include(c => c.Project).ThenInclude(c => c.Phases).Include(c => c.Issues).FirstOrDefault();
            if (status == null)
                return new ResponseMediator("Status  not found", null, 404);

            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new ResponseMediator("User  not found", null, 404);


            if (request.AssignedToId.HasValue)
            {
                var assignee = _unitOfWork.Users.FindOne(x => x.Id == request.AssignedToId);
                if (assignee == null)
                    return new ResponseMediator("Assigned user not found", null, 404);
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
            var response2 = await _requestClient.GetResponse<UserResponse>(new AddIssueMessage2 { Issue = issue, StatusId = request.StatusId });
            //await _publishEndpoint.Publish(new AddIssueMessage2() { Issue = issue, StatusId = request.StatusId });
            //await Task.Delay(350, cancellationToken);
            var response = _mapper.Map<IssueDTO>(issue);
            if(responseSuccess == 205)
            {
                _unitOfWork.Notifications.Add(new Notification() { CreatedAt = DateTime.Now, UserId = userAssignee.Id, Type = "assignIssue", Data = JsonSerializer.Serialize(new { type = "assignIssue", assignerName = user.FullName, assignerUsername = user.UserName, assignerAvatar = user.Avatar, projectId = status.ProjectId, issueName = request.Title, issueId = Guid.Parse(response2.Message.UserId) , issueIndex = index, issueStatusName = status.Name}) });
                await _unitOfWork.SaveChangesAsync();
                await _publisher.Publish(new SendEmailMessage() { ToEmail = userAssignee.Email == null ? "" : userAssignee.Email, Body = EmailMessage.AssignIssue(request.Title, request.Description, request.StartDate, request.DueDate, response2.Message.UserId, status.ProjectId + ""), Subject = $"[ {status.Project.Name} ]You are assigned to issue {request.Title}" });
            }
            return new ResponseMediator("", response);
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
