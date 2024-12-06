using Capstone.Application.Common.ResponseMediator;
using MediatR;
using System.Text.Json.Serialization;

namespace Capstone.Application.Module.Issues.Command
{
    public class DeleteIssueCommand : IRequest<ResponseMediator>
    {
        public Guid Id { get; set; }
        [JsonIgnore]
        public string Token { get; set; } = string.Empty;
    }
}
