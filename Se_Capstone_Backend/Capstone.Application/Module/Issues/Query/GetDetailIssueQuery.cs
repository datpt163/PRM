using Capstone.Application.Common.ResponseMediator;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Issues.Query
{
    public class GetDetailIssueQuery : IRequest<ResponseMediator>
    {
        public Guid Id { get; set; } 
        public string Token { get; set; } = string.Empty;
    }
}
