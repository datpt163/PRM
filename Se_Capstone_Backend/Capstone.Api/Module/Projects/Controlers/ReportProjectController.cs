using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Module.Projects.Request;
using Capstone.Application.Module.Projects.Query;
using Capstone.Domain.Entities;
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
        //[Authorize(Roles = "REPORT_PROJECT")]
        public async Task<IActionResult> GenerateReport([FromBody] GenerateReportTask request)
        {
            var query = new GetReportTaskQuery
            {
                ProjectId = request.ProjectId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                PhaseId = request.PhaseId,
            };

            var result = await _mediator.Send(query);

            return ResponseOk(result, "Report generated successfully.");
        }

        [HttpPost("tasks/overview")]
        //[Authorize(Roles = "REPORT_PROJECT")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTaskOverview([FromBody] TaskOverviewRequest request)
        {
            var query = new GetTaskOverviewQuery
            {
                ProjectId = request.ProjectId,
                PhaseId = request.PhaseId,
                EndDate = request.EndDate,
                StartDate = request.StartDate,
            };

            var result = await _mediator.Send(query);

            return ResponseOk(result, "Task overview retrieved successfully.");
        }

        [HttpPost("tasks/completion-chart")]
        //[Authorize(Roles = "REPORT_PROJECT")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTaskCompletionChart([FromBody] TaskCompletionChartRequest request)
        {
            var query = new GetTaskCompletionChartQuery
            {
                ProjectId = request.ProjectId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                PhaseId = request.PhaseId,
            };

            var result = await _mediator.Send(query);
            return ResponseOk(result, "Task completion chart data retrieved successfully.");
        }


    }
}
