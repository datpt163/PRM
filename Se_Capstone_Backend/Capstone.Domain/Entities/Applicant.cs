using Capstone.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone.Domain.Entities
{
    [Table("applicants")]
    public class Applicant : BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        [MaxLength(100)]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? CvLink { get; set; } = string.Empty; 
        public bool? IsOnBoard { get; set; }
        public Guid MainJobId { get; set; }
        public Job MainJob { get; set; } = new Job();
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
