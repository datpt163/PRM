using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Module.Projects.Request;
using Capstone.Application.Module.Projects.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Module.Projects.Controllers
{
    [Route("api/projects/reports/")]
    [ApiController]
    public class ReportProjectController : BaseController
    {
        private readonly IMediator _mediator;

        public ReportProjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("tasks")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateReport([FromBody] GenerateReportTask request)
        {
            var query = new GetReportTaskQuery
            {
                ProjectId = request.ProjectId,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            var result = await _mediator.Send(query);

            return ResponseOk(result, "Report generated successfully.");
        }

    }
}
