

namespace Capstone.Application.Module.Auths.Model
{
    public class MonitorTokenModel
    {
        public Guid RoleId { get; set; }
        public Guid UserId { get; set; }
        public string Token {  get; set; } = string.Empty;
    }
}
