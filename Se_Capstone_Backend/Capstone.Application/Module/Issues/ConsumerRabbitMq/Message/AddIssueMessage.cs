using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Issues.ConsumerRabbitMq.Message
{
    public class AddIssueMessage2
    {
        public Domain.Entities.Issue? Issue { get; set; }
        public Guid StatusId { get; set; }
    }
}
