using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Dashboard.Response
{
    public class DashboardOverviewResponse
    {
        public int OngoingTasks { get; set; }
        public int TotalTasks { get; set; }
        public int UnfinishedProjects { get; set; }
        public int ProjectsDueThisMonth { get; set; }
        public int PausedTasks { get; set; }
        public List<TaskCompletionRate> TaskCompletionRate { get; set; } = new List<TaskCompletionRate>();
    }

    public class TaskCompletionRate
    {
        public string Status { get; set; } = string.Empty;
        public double Percentage { get; set; }
    }
}
