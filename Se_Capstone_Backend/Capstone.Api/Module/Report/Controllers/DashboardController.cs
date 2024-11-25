using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Capstone.Application.Module.Dashboard.Query;
using MediatR;

namespace Capstone.Api.Module.Dashboard.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("overview")]
        [Authorize(Roles = "DASHBOARD_VIEW")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var query = new GetDashboardOverviewQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
