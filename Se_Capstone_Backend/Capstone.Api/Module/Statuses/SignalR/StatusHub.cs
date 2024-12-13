using Capstone.Application.Common.Email.EmailQueue;
using Capstone.Application.Common.EmailHTML;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Module.Issues.ConsumerRabbitMq;
using Capstone.Application.Module.Issues.ConsumerRabbitMq.Message;
using Capstone.Application.Module.Status.ConsumerRabbitMq.Message;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading;
using static MassTransit.ValidationResultExtensions;
namespace Capstone.Api.Module.Statuses.SignalR
{
    public class StatusHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IJwtService _jwtService;
        private readonly IPublishEndpoint _publisher;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IRequestClient<OrderStatusMessage2> _requestClient;
        private readonly IRequestClient<OrderIssueMessage2> _requestClient2;

        public StatusHub(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint, IJwtService jwtService, IPublishEndpoint publisher, IHubContext<NotificationHub> hubContext, IRequestClient<OrderStatusMessage2> requestClient, IRequestClient<OrderIssueMessage2> requestClient2)
        {
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _jwtService = jwtService;
            _publisher = publisher;
            _hubContext = hubContext;
            _requestClient = requestClient;
            _requestClient2 = requestClient2;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                Console.WriteLine("Connnect success");
                await base.OnConnectedAsync(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connnect Fail" + ex.Message);
            }
        }

        public async Task JoinGroup(string groupId)
        {
            Console.WriteLine("Join group success success");
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }
        public async Task StatusOrderRequest(string groupId, Guid statusId, int position)
        {
            try
            {
                await Clients.Group(groupId).SendAsync("StatusOrderResponse", "Success");

                var status = _unitOfWork.Statuses.Find(x => x.Id == statusId).Include(c => c.Project).ThenInclude(c => c.Statuses).FirstOrDefault();
                if (status == null)
                    throw new Exception("Status not found.");
                //if(position > status.Project.Statuses.Count())
                //    throw new Exception("Some thing wrong with position");
                //if (position < 1)
                //    throw new Exception("Some thing wrong with position");
                if (position == status.Position)
                    throw new Exception("Old position same new position");
                var response2 = await _requestClient.GetResponse<UserResponse>(new OrderStatusMessage2() { Status = status, Position = position });
                //await _publishEndpoint.Publish(new OrderStatusMessage() { Status = status, Position = position });
                //await Task.Delay(250);
                await Clients.Group(groupId).SendAsync("StatusOrderResponse", "Success");
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.ToString());
            }
        }

        public async Task IssueOrderRequest(string groupId, Guid issueId, Guid statusId, int position)
        {
            try
            {
                var status = _unitOfWork.Statuses.Find(x => x.Id == statusId).Include(c => c.Issues).FirstOrDefault();
                if (status == null)
                    throw new Exception("Status not found.");

                var issue = _unitOfWork.Issues.Find(x => x.Id == issueId).Include(c => c.Reporter).Include(c => c.Assignee).Include(c => c.Status).FirstOrDefault();
                if (issue == null)
                    throw new Exception("Issue not found.");

                //if (position < 0)
                //    throw new Exception("Some thing wrong with position");

                //if (position > status.Issues.Count())
                //    throw new Exception("Some thing wrong with position");
                var httpContext = Context.GetHttpContext();
                var token = httpContext?.Request.Headers["Authorization"].ToString();
                token = token?.Replace("Bearer ", "");
                var users = new List<User>();
                users.Add(issue.Reporter);
                if(issue.Assignee != null)
                    users.Add(issue.Assignee);

                if (!string.IsNullOrEmpty(token))
                {
                    var userQuery =  await _jwtService.VerifyTokenAsync(token);
                    if(userQuery != null)
                    {
                        users.RemoveAll(x => x.Id != userQuery.Id);
                        if (issue.StatusId != statusId)
                        {
                            foreach (var u in users)
                            {
                                _unitOfWork.Notifications.Add(new Domain.Entities.Notification() { CreatedAt = DateTime.Now, UserId = u.Id, Type = "updateIssue", Data = JsonSerializer.Serialize(new { type = "updateIssue", projectId = issue.Status.ProjectId, issueId = issue.Id, issueName = issue.Title, issueIndex = issue.Index, issueStatusName = status.Name, updaterName = userQuery.FullName, updaterUserName = userQuery.UserName, updaterAvatar = userQuery.Avatar }) });
                                await _unitOfWork.SaveChangesAsync();
                                await _publisher.Publish(new SendEmailMessage() { ToEmail = u.Email == null ? "" : u.Email, Body = EmailMessage.UpdateIssue(issue.Title, issue.Id + "", issue.Status.ProjectId + ""), Subject = $"[ {issue.Title} ]You have a new update" });
                                await _hubContext.Clients.Group(u.Id + "")
                                     .SendAsync("NotificationResponse", "Success");
                            }
                        }
                    }
                }
                var response2 = await _requestClient2.GetResponse<UserResponse>(new OrderIssueMessage2() { StatusId = statusId, Position = position, IssueId = issueId });
                //await _publishEndpoint.Publish(new OrderIssueMessage() {  StatusId = statusId, Position = position, IssueId = issueId });
                //await Task.Delay(250);
                await Clients.Group(groupId).SendAsync("IssueOrderResponse", "Success");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

}
