using Capstone.Application.Module.Dashboard.Response;
using MediatR;

namespace Capstone.Application.Module.Dashboard.Query
{
    public class GetProjectStatsByMonthQuery : IRequest<List<ProjectStatsResponse>>
    {
        public DateTime Date { get; set; }
    }
}
