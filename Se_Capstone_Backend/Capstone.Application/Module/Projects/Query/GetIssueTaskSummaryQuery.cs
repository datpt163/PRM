using Capstone.Application.Module.Projects.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.Query
{
    public class GetIssueTaskSummaryQuery : IRequest<IssueTaskSummaryResponse>
    {
        public Guid ProjectId { get; set; }
        public Guid? PhaseId { get; set; }
        public Guid UserId { get; set; }
    }
}
