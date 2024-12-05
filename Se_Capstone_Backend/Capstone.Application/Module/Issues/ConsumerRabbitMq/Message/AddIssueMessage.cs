using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Issues.ConsumerRabbitMq.Message
{
    public class AddIssueMessage2
    {
        public Guid StatusId { get; set; }
        public int Index { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority? Priority { get; set; }
        public float? EstimatedTime { get; set; }
        public float? ActualTime { get; set; }
        public Guid? ParentIssueId { get; set; }
        public Guid ReporterId { get; set; }
        public Guid? AssigneeId { get; set; }
        public Guid? LastUpdateById { get; set; }
        public Guid? LabelId { get; set; }
        public Guid? PhaseId { get; set; }
    }
}


