using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.Response
{
    public class IssueCompletionData
    {
        public string Period { get; set; }
        public int CompletedTasks { get; set; }
        public int DueTasks { get; set; }
    }
}
