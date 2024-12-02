using Capstone.Application.Module.Issues.ConsumerRabbitMq.Message;
using Capstone.Infrastructure.Repository;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Issues.ConsumerRabbitMq
{
    public class OrderIssueConsumer : IConsumer<OrderIssueMessage2>
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderIssueConsumer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<OrderIssueMessage2> context)
        {
            var status = _unitOfWork.Statuses.Find(x => x.Id == context.Message.StatusId).Include(c => c.Issues).FirstOrDefault();
            if (status == null)
                throw new Exception("Status not found.");

            var issue = _unitOfWork.Issues.Find(x => x.Id == context.Message.IssueId).Include(c => c.Status).ThenInclude(c => c.Issues).FirstOrDefault();
            if (issue == null)
                throw new Exception("Issue not found.");

            foreach (var iss in issue.Status.Issues)
                if (iss.Position > issue.Position)
                    iss.Position--;

            issue.StatusId = context.Message.StatusId;
            foreach (var iss in status.Issues)
                if (iss.Position >= context.Message.Position)
                    iss.Position++;
            issue.Position = context.Message.Position;
            _unitOfWork.Issues.Update(issue);
            _unitOfWork.Statuses.Update(status);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
