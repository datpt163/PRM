using Capstone.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Capstone.Domain.Entities
{
    [Table("positions")]
    public class Position : BaseEntity
    {
        public Guid Id { get; set; }
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;
        public string? Description {  get; set; } = string.Empty;
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    }
}
