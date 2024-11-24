using Capstone.Application.Common.ResponseMediator;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Notifications.Command
{
    public class DeleteNotificationCommand : IRequest<ResponseMediator>
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
