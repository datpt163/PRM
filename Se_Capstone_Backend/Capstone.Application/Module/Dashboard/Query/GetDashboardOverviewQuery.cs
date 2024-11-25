using Capstone.Application.Module.Dashboard.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Dashboard.Query
{
    public class GetDashboardOverviewQuery : IRequest<DashboardOverviewResponse>
    {
    }
}
