namespace Capstone.Api.Module.Projects.Request
{
    public class UpdateMemberRequest
    {
        public Guid? PositionId { get; set; }
        public bool IsProjectConfigurator { get; set; } = false;
        public bool IsIssueConfigurator { get; set; } = false;
        public bool IsCommentConfigurator { get; set; } = false;
    }
}
