namespace Capstone.Application.Module.Dashboard.Response
{
    public class UserOverviewResponse
    {
        public int TotalSkills { get; set; }
        public int TotalTasks { get; set; }
        public int TotalCurrentTasks { get; set; }
        public int TotalTasksDone { get; set; }
        public int TotalProjects { get; set; }
        public int TotalProjectsLead { get; set; }
        public int TotalCurrentProjects { get; set; }

        public List<OverViewTask> OverViewTasks { get; set; } = new List<OverViewTask>();
    }

    public class OverViewTask
    {
        public Guid ProjectId { get; set; }
        public Guid TaskId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string TaskName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Index { get; set; }


    }
}
