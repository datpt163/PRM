using Capstone.Application.Common.ResponseMediator;
using Capstone.Domain.Enums;
using MediatR;
using System.ComponentModel;


namespace Capstone.Application.Module.Projects.Command
{
    public class UpdateProjectCommand : IRequest<ResponseMediator>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [DefaultValue("2024-10-21T09:50:31.798")]
        public DateTime StartDate { get; set; }
        [DefaultValue("2024-10-22T09:50:31.798")]
        public DateTime EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public Guid? TeamLeadId { get; set; }
        public string Token { get; set; } = string.Empty;
        public float? TotalEffort { get; set; } 

        public UpdateProjectCommand(Guid id, string name, string code, string description, DateTime startDate, DateTime endDate, Guid? teamLeadId, ProjectStatus projectStatus, float? totalEffort)
        {
            Id = id;
            Name = name;
            Code = code;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            TeamLeadId = teamLeadId;
            Status = projectStatus;
            TotalEffort = totalEffort;
        }
    }
}
