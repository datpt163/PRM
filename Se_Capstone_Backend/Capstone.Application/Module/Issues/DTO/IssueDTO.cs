using Capstone.Application.Module.Comments.CommentDTOs;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Issues.DTO
{
    public class IssueDTO
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int Percentage { get; set; } = 0;
        public Priority? Priority { get; set; }
        public float? EstimatedTime { get; set; }
        public int? ActualTime { get; set; } = 0;
        public int PercentDone { get; set; } = 0;
        public int Position { get; set; }
        public Guid? ReporterId { get;set;}
        public Guid? LabelId { get; set; }
        public Guid StatusId { get; set; }
        public Guid? PhaseId { get; set; }
        public Domain.Entities.Phase? Phase { get; set; }
        public Label? Label { get; set; }
        public Domain.Entities.Status? Status { get; set; }
        public UserForProjectDetailDTO? LastUpdateBy { get; set; }
        public UserForProjectDetailDTO Reporter { get; set; } = null!;
        public UserForProjectDetailDTO? Assignee { get; set; } = null!;
        public IssueDTO? ParentIssue { get; set; }
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
    }

    public class IssueDTO2
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int Percentage { get; set; } = 0;
        public Priority? Priority { get; set; }
        public float? EstimatedTime { get; set; }
        public int? ActualTime { get; set; } = 0;
        public int PercentDone { get; set; } = 0;
        public int Position { get; set; }
        public Guid? ParentIssueId { get; set; }
        public Domain.Entities.Phase? Phase { get; set; }
        public Label? Label { get; set; }
        public Domain.Entities.Status? Status { get; set; }
        public UserForProjectDetailDTO? LastUpdateBy { get; set; }
        public UserForProjectDetailDTO Reporter { get; set; } = null!;
        public UserForProjectDetailDTO? Assignee { get; set; } = null!;
        public IssueDTO? ParentIssue { get;set; }
        public List<IssueDTO> SubIssues { get; set; } = new List<IssueDTO>();
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
    }
}
