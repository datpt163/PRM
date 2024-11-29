using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Capstone.Domain.Entities
{
    [Table("statuses")]
    public class Status
    {
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? Description { get; set; } = string.Empty;
        public int Position { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool? IsDone { get; set; } = false;
        public Guid ProjectId { get; set; }
        [JsonIgnore]
        public Project Project { get; set; } = null!;
        [JsonIgnore]
        public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    }
}
