namespace Capstone.Api.Module.Report.Request
{
    public class GetSkillsRequest
    {
        public string? Title { get; set; } 
        public int? MinimumUsers { get; set; } 
        public int? MaximumUsers { get; set; }
        public Guid? UserId { get; set; }
    }
}
