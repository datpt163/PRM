using Capstone.Application.Module.Dashboard.Response;
using MediatR;
using System;

namespace Capstone.Application.Module.Dashboard.Query
{
    public class GetProjectStatsByYearQuery : IRequest<List<ProjectStatsResponse>>
    {
        public int Year { get; set; }
    }
}
