using Capstone.Application.Module.Issues.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Status.Responses
{
    public class KanbanResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } 
        public int Position { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool? IsDone { get; set; }
        public List<IssueDTO> Issues { get; set; } = new List<IssueDTO>();
        public int IssueCount { get; set; }
    }
}
