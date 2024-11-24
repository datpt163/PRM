using Capstone.Application.Common.Paging;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Notifications.Query
{
    public class GetListNotificationQuery : IRequest<PagingResultSP<Notification>>
    {
        public string Token { get; set; } = string.Empty;
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
