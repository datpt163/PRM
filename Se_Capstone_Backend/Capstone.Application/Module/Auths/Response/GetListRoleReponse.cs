

namespace Capstone.Application.Module.Auths.Response
{
    public class RoleDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO> { };
    }
}
