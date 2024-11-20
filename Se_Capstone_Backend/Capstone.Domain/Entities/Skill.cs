using Capstone.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone.Domain.Entities
{
    [Table("skills")]
    public class Skill : BaseEntity

    {
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        public ICollection<User>? Users { get; set; } = new List<User>();
    }
}
