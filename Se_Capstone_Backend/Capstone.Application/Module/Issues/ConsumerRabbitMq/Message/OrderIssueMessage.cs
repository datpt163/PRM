using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Issues.ConsumerRabbitMq.Message
{
    public class OrderIssueMessage2
    {
        public Guid IssueId { get; set; }
        public Guid StatusId { get; set; }
        public int Position { get; set; }
    }
}
