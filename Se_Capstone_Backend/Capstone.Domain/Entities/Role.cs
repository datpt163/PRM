using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace Capstone.Domain.Entities
{
    public class Role : IdentityRole<Guid>
    {
        public string? Description { get; set; } = string.Empty;
        public string Color { get; set; } = "blue";
        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
