using Capstone.Domain.Enums;

namespace Capstone.Application.Module.Dashboard.Response
{
    public class ProjectStatsResponse
    {
        public ProjectStatus Status { get; set; }
        public int Count { get; set; }
    }
}
