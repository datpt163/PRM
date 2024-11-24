using Capstone.Application.Common.Jwt;
using Capstone.Application.Module.Issues.ConsumerRabbitMq.Message;
using Capstone.Application.Module.Status.ConsumerRabbitMq.Message;
using Capstone.Infrastructure.Repository;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
namespace Capstone.Api.Module.Statuses.SignalR
{
     [Authorize]
    public class StatusHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;

        public StatusHub(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
        {
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
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

                var httpContext = Context.GetHttpContext();

                var status = _unitOfWork.Statuses.Find(x => x.Id == statusId).Include(c => c.Project).ThenInclude(c => c.Statuses).FirstOrDefault();
                if (status == null)
                    throw new Exception("Status not found.");
                if(position > status.Project.Statuses.Count())
                    throw new Exception("Some thing wrong with position");
                if (position < 1)
                    throw new Exception("Some thing wrong with position");
                if (position == status.Position)
                    throw new Exception("Old position same new position");
                await _publishEndpoint.Publish(new OrderStatusMessage() { Status = status, Position = position });
                await Task.Delay(250);
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
                var httpContext = Context.GetHttpContext();

                var status = _unitOfWork.Statuses.Find(x => x.Id == statusId).Include(c => c.Issues).FirstOrDefault();
                if (status == null)
                    throw new Exception("Status not found.");

                var issue = _unitOfWork.Issues.Find(x => x.Id == issueId).FirstOrDefault();
                if (issue == null)
                    throw new Exception("Issue not found.");

                if (position < 0)
                    throw new Exception("Some thing wrong with position");

                if (position > status.Issues.Count())
                    throw new Exception("Some thing wrong with position");
                await _publishEndpoint.Publish(new OrderIssueMessage() {  StatusId = statusId, Position = position, IssueId = issueId });
                await Task.Delay(250);
                await Clients.Group(groupId).SendAsync("IssueOrderResponse", "Success");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task NotificationRequest(string groupId)
        {
            try
            {
                await Clients.Group(groupId).SendAsync("NotificationResponse", "Success");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

}
