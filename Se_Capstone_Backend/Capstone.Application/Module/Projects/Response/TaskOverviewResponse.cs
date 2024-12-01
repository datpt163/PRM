namespace Capstone.Application.Module.Projects.Query
{
    public class TaskOverviewResponse
    {
        public int OngoingTasks { get; set; }
        public int TotalTasks { get; set; }
        public int DoneTasks { get; set; }
        public double OverallCompletionRate { get; set; }
        public float? TotalEffort { get; set; }
        public float? ActualEffot { get; set; } = 0;
        public float? EstimateEffort { get; set; } = 0;

        public List<TaskCompletionRate> TaskCompletionRate { get; set; } = new List<TaskCompletionRate>();
    }

    public class TaskCompletionRate
    {
        public string Status { get; set; } = string.Empty;
        public double Percentage { get; set; }
    }
}
