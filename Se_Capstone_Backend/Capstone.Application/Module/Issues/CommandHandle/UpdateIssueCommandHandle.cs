using AutoMapper;
using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.EmailHTML;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Issues.Command;
using Capstone.Application.Module.Issues.DTO;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using Google.Apis.Util;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Capstone.Application.Module.Issues.CommandHandle
{
    public class UpdateIssueCommandHandle : IRequestHandler<UpdateIssueCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        public readonly IPublishEndpoint _publisher;
        public UpdateIssueCommandHandle(IUnitOfWork unitOfWork, IJwtService jwtService, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _publisher = publishEndpoint;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<ResponseMediator> Handle(UpdateIssueCommand request, CancellationToken cancellationToken)
        {
            var users = new List<User>();
            bool checkAssign = false;
            var userAssignee = new User();
            var issue = _unitOfWork.Issues.Find(x => x.Id == request.Id).Include(c => c.Status).Include(c => c.Phase).Include(c => c.Label).Include(c => c.Status).ThenInclude(c => c.Issues).Include(c => c.LastUpdateBy).Include(c => c.ParentIssue).Include(c => c.Reporter).Include(c => c.Assignee).Include(c => c.SubIssues).Include(c => c.Comments).FirstOrDefault();
            if (issue == null)
                return new ResponseMediator("Issue not found", null);

            if (string.IsNullOrEmpty(request.Title))
                return new ResponseMediator("Title empty", null, 400);

            if (request.StartDate.HasValue && request.DueDate.HasValue && request.StartDate.Value.Date > request.DueDate.Value.Date)
                return new ResponseMediator("Start date must greater or equal due date", null, 400);

            if (request.Priority.HasValue && ((int)request.Priority < 1 || (int)request.Priority > 5))
                return new ResponseMediator("Priority must be between 1 and 5", null, 400);

            if (request.Percentage < 0 && request.Percentage > 100)
                return new ResponseMediator("Percentage must be greater or equal than 0 and less or equal than 100", null, 400);

            if (request.EstimatedTime.HasValue && request.EstimatedTime.Value < 0)
                return new ResponseMediator("Estimated time must be greater or equal than 0 hour", null, 400);

            if (request.ActualTime.HasValue && request.ActualTime.Value < 0)
                return new ResponseMediator("Actual time must be greater or equal than 0 hour", null, 400);

            if (request.EstimatedTime.HasValue && request.EstimatedTime.Value <= 0)
                return new ResponseMediator("Estimated time must be greater than 0 hour", null, 400);

            if (request.ParentIssueId.HasValue)
                if (_unitOfWork.Issues.FindOne(x => x.Id == request.ParentIssueId.Value) == null)
                    return new ResponseMediator("Parent issue not found", null, 404);
            if (request.AssigneeId.HasValue && _unitOfWork.Users.FindOne(x => x.Id == request.AssigneeId) == null)
                return new ResponseMediator("Assigned user not found", null, 404);

            if (request.LabelId.HasValue && _unitOfWork.Labels.FindOne(x => x.Id == request.LabelId) == null)
                return new ResponseMediator("Label  not found", null, 404);

            if (request.PhaseId.HasValue && _unitOfWork.Phases.FindOne(x => x.Id == request.PhaseId) == null)
                return new ResponseMediator("Phase  not found", null, 404);

            var status = _unitOfWork.Statuses.Find(x => x.Id == request.StatusId).Include(c => c.Project).ThenInclude(c => c.Phases).Include(c => c.Issues).FirstOrDefault();
            if (status == null)
                return new ResponseMediator("Status  not found", null, 404);

            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new ResponseMediator("User  not found", null, 404);

            if((issue.AssigneeId == null && request.AssigneeId != null ) || ( issue.AssigneeId != null && request.AssigneeId != null && issue.AssigneeId != request.AssigneeId))
            {
                var assignee = _unitOfWork.Users.FindOne(x => x.Id == request.AssigneeId);
                if (assignee == null)
                    return new ResponseMediator("Assigned user not found", null, 404);
                if (assignee.Id != user.Id)
                {
                    checkAssign = true;
                    userAssignee = assignee;
                    users.Add(assignee);
                }
            }


            if (issue.StatusId != request.StatusId)
            {
                foreach (var iss in issue.Status.Issues)
                    if (iss.Position > issue.Position)
                        iss.Position--;

                foreach (var issu in status.Issues)
                    issu.Position++;
                issue.Position = 0;
            }


            DateTime? actualDate = null;
            bool flag = false;
            if (issue.Status.IsDone.HasValue && issue.Status.IsDone.Value && (status.IsDone == false || status.IsDone == null))
            {
                actualDate = null;
                flag = true;
            }
            if ((issue.Status.IsDone == null || issue.Status.IsDone == false) && (status.IsDone == true))
            {
                actualDate = DateTime.Now;
                flag = true;
            }
            issue.ActualDate = actualDate;
            if (flag)
                issue.ActualDate = DateTime.Now;
            issue.Title = request.Title;
            issue.Description = request.Description;
            issue.StartDate = request.StartDate;
            issue.DueDate = request.DueDate;
            issue.Percentage = request.Percentage;
            issue.Priority = request.Priority;
            issue.EstimatedTime = request.EstimatedTime;
            issue.ParentIssueId = request.ParentIssueId;
            issue.AssigneeId = request.AssigneeId;
            issue.LastUpdateById = user.Id;
            issue.StatusId = request.StatusId;
            issue.LabelId = request.LabelId;
            issue.PhaseId = request.PhaseId;
            issue.ActualTime = request.ActualTime;
            _unitOfWork.Issues.Update(issue);
            users.Add(issue.Reporter);

            await _unitOfWork.SaveChangesAsync();

            if (checkAssign)
            {
                _unitOfWork.Notifications.Add(new Notification() { CreatedAt = DateTime.Now, UserId = userAssignee.Id, Type = "assignIssue", Data = JsonSerializer.Serialize(new { type = "assignIssue", assignerName = user.FullName, assignerUsername = user.UserName, assignerAvatar = user.Avatar, projectId = status.ProjectId, issueName = request.Title, issueId = request.Id, issueIndex = issue.Index, issueStatusName = status.Name }) });
                await _unitOfWork.SaveChangesAsync();
                await _publisher.Publish(new SendEmailMessage() { ToEmail = userAssignee.Email == null ? "" : userAssignee.Email, Body = EmailMessage.AssignIssue(request.Title, request.Description, request.StartDate, request.DueDate, request.Id + "", status.ProjectId + ""), Subject = $"[ {status.Project.Name} ]You are assigned to issue {request.Title}" });
            }

            users.Add(issue.Reporter);
            if (issue.Assignee != null)
                users.Add(issue.Assignee);
            users.RemoveAll(x => x.Id == user.Id);

            foreach(var u in users)
            {
                _unitOfWork.Notifications.Add(new Notification() { CreatedAt = DateTime.Now, UserId = u.Id, Type = "updateIssue", Data = JsonSerializer.Serialize(new { type = "updateIssue", projectId = status.ProjectId, issueId = request.Id, issueName = request.Title, issueIndex = issue.Index, issueStatusName = status.Name, updaterName = user.FullName, updaterUserName = user.UserName, updaterAvatar = user.Avatar }) });
                await _unitOfWork.SaveChangesAsync();
                await _publisher.Publish(new SendEmailMessage() { ToEmail = u.Email == null ? "" : u.Email, Body = EmailMessage.UpdateIssue(request.Title, request.Id + "", status.ProjectId + ""), Subject = $"[ {request.Title} ]You have a new update" });
            }

            var response = _mapper.Map<IssueDTO?>(issue);
            return new ResponseMediator(JsonSerializer.Serialize(users.Select(x => x.Id)), response, 200);
        }
    }
}
