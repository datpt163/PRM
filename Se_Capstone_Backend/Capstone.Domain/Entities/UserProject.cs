using Capstone.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Capstone.Domain.Entities
{
    [Table("userProjects")]
    public class UserProject : BaseEntity
    {
        public Guid UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; } = null!;

        public Guid ProjectId { get; set; }
        [JsonIgnore]
        public Project Project { get; set; } = null!;

        public Guid? PositionId { get; set; }
        [JsonIgnore]
        public Position? Position { get; set; } 
        public bool IsProjectConfigurator { get; set; } = false;
        public bool IsIssueConfigurator { get; set; } = false;
        public bool IsCommentConfigurator { get; set; } = false;
    }
}
