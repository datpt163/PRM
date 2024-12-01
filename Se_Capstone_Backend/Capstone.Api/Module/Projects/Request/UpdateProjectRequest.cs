using Capstone.Domain.Enums;
using System.ComponentModel;

namespace Capstone.Api.Module.Projects.Request
{
    public class UpdateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [DefaultValue("2024-10-21T09:50:31.798")]
        public DateTime StartDate { get; set; }
        [DefaultValue("2024-10-22T09:50:31.798")]
        public DateTime EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public Guid? LeadId { get; set; }
        public float? TotalEffort { get; set; }

    }
}
