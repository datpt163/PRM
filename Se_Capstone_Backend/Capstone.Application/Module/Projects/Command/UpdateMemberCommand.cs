using Capstone.Application.Common.ResponseMediator;
using Capstone.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.Command
{
    public class UpdateMemberCommand : IRequest<ResponseMediator>
    {
        public Guid UserId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? PositionId { get; set; }
        public bool IsProjectConfigurator { get; set; } = false;
        public bool IsIssueConfigurator { get; set; } = false;
        public bool IsCommentConfigurator { get; set; } = false;
    }
}
