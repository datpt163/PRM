using Capstone.Application.Common.ResponseMediator;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Issues.Command
{
    public class AddIssueCommand : IRequest<ResponseMediator>
    {
        public AddIssueCommand(string token, string title, string? description, DateTime? startDate, DateTime? dueDate, Priority? priority, float? estimatedTime, Guid? parentIssueId, Guid? assignedId,Guid statusId, Guid? labelId)
        {
            Token = token;
            Title = title;
            Description = description;
            StartDate = startDate;
            DueDate = dueDate;
            Priority = priority;
            EstimatedTime = estimatedTime;
            ParentIssueId = parentIssueId;
            AssignedToId = assignedId;
            StatusId = statusId;
            LabelId = labelId;
        }

        public string Token { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority? Priority { get; set; }
        public float? EstimatedTime { get; set; } 
        public Guid? ParentIssueId { get; set; }
        public Guid? AssignedToId { get; set; }
        public Guid StatusId { get; set; }
        public Guid? LabelId { get; set; }
        public Guid? PhaseId { get; set; }
    }
}
