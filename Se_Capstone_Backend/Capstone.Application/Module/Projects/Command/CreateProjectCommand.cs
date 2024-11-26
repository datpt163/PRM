using Capstone.Application.Common.ResponseMediator;
using MediatR;
using System.ComponentModel;
using System.Text.Json.Serialization;


namespace Capstone.Application.Module.Projects.Command
{
    public class CreateProjectCommand : IRequest<ResponseMediator>
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [DefaultValue("2024-10-21T09:50:31.798")]
        public DateTime? StartDate { get; set; }
        [DefaultValue("2024-10-22T09:50:31.798")]
        public DateTime? EndDate { get; set; }
        public bool? IsVisible { get; set; }
        public Guid? LeadId { get; set; }
        [JsonIgnore]
        public string Token { get; set; } = string.Empty;
    }
}
