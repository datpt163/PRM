using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Capstone.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Type { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public bool HasRead { get; set; } = false;
        [JsonIgnore]
        public User User { get; set; } = null!;
    }
}
