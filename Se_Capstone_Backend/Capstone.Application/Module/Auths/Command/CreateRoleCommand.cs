using Capstone.Application.Common.ResponseMediator;
using MediatR;


namespace Capstone.Application.Module.Auths.Command
{
    public class CreateRoleCommand : IRequest<ResponseMediator>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<Guid> PermissionsId { get; set; } = new List<Guid>();
    }
}
