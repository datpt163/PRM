using Capstone.Domain.Enums;

namespace Capstone.Application.Module.Projects.Response
{
    public class ProjectDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsVisible { get; set; } = false;
        public Guid? LeadId { get; set; }
        public string? LeadName { get; set; }
        public string? LeadPosition { get; set; }
        public string? LeadAvatar { get; set; }
        public List<string> MyPermissions { get; set; } = new List<string>();
        public List<UserForProjectDetailDTO> Members { get; set; } = new List<UserForProjectDetailDTO> { };
    }

    public class UserForProjectDetailDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? RoleName { get; set; }
        public string? PositionName { get; set; }
        public bool IsProjectConfigurator { get; set; }
        public bool IsIssueConfigurator { get; set; }
        public string? Avatar { get; set; }
        public bool IsConfigurator { get; set; } = false;
    }
}
