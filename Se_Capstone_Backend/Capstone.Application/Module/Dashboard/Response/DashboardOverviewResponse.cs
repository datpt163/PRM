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
        
        public int TotalProjects { get; set; }

        public int TotalProjectsDone { get; set; }

        public int TotalSkillsEmployee { get; set; }
    }

}
