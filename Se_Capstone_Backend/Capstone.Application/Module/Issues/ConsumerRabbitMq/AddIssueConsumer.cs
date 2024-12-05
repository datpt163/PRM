using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Issues.ConsumerRabbitMq.Message;
using Capstone.Application.Module.Status.ConsumerRabbitMq.Message;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Repository;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Issues.ConsumerRabbitMq
{
    public class AddIssueConsumer : IConsumer<AddIssueMessage2>
    {
        private readonly IUnitOfWork _unitOfWork;
        public AddIssueConsumer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<AddIssueMessage2> context)
        {
            Console.WriteLine("vaooooooooooooooooooo");
            var issue = new Issue()
            {
                Index = context.Message.Index,
                Title = context.Message.Title,
                Description = context.Message.Description,
                StartDate = context.Message.StartDate,
                DueDate = context.Message.DueDate,
                Priority = context.Message.Priority,
                EstimatedTime = context.Message.EstimatedTime,
                ParentIssueId = context.Message.ParentIssueId,
                ReporterId = context.Message.ReporterId,
                AssigneeId = context.Message.AssigneeId,
                LastUpdateById = context.Message.LastUpdateById,
                StatusId = context.Message.StatusId,
                LabelId = context.Message.LabelId,
                PhaseId = context.Message.PhaseId
            };

            if (issue != null)
            {
                var status = _unitOfWork.Statuses.Find(x => x.Id == context.Message.StatusId).Include(c => c.Project).ThenInclude(c => c.Phases).Include(c => c.Issues).FirstOrDefault();
                if (status == null)
                    return;
                var position = status.Issues.Where(x => x.ParentIssue == null).Count();
                issue.Position = position;
                _unitOfWork.Issues.Add(issue);
                await _unitOfWork.SaveChangesAsync();
                await context.RespondAsync(new UserResponse() { UserId = issue.Id + "" });
            }
            await context.RespondAsync(new UserResponse() { UserId = "" });
        }
    }

    public class UserResponse
    {
        public string UserId { get; set; } = string.Empty;
    }
}
