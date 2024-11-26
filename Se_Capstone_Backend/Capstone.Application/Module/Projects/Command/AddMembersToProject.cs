using Capstone.Application.Common.ResponseMediator;
using MediatR;
using System.Text.Json.Serialization;


namespace Capstone.Application.Module.Projects.Command
{
    public class AddMembersToProject : IRequest<ResponseMediator>
    {
        public Guid ProjectId { get; set; }
        public List<Guid> MemberIds { get; set; } = new List<Guid>();
        [JsonIgnore]
        public string Token { get; set; } = string.Empty;
    }
}
