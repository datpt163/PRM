using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.Response
{
    public class ResponseReportTask
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public List<UserReport> Users { get; set; } = new List<UserReport>();
        public List<StatusReport> Statuses { get; set; } = new List<StatusReport>();
    }

    public class UserReport
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public List<UserTaskStatus> UserTaskStatuses { get; set; } = new List<UserTaskStatus>();
    }

    public class UserTaskStatus
    {
        public Guid StatusId { get; set; }
        public int Total { get; set; }
    }

    public class StatusReport
    {
        public Guid Id { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

}
