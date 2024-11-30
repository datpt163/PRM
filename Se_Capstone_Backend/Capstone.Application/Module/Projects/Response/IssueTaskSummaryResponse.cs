using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.Response
{
    public class IssueTaskSummaryResponse
    {
        public int TotalIssues { get; set; }
        public int TotalUnfinishedTasks { get; set; }
    }

}
