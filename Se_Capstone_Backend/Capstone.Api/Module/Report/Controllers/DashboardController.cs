using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Capstone.Application.Module.Dashboard.Query;
using MediatR;
using Capstone.Api.Common.ResponseApi.Controllers;

namespace Capstone.Api.Module.Dashboard.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("overview")]
        [Authorize(Roles = "VIEW_DASHBOARD")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var query = new GetDashboardOverviewQuery();
            var result = await _mediator.Send(query);
            return ResponseOk(result);
        }

        [HttpGet("projects-by-month")]
        //[Authorize(Roles = "VIEW_DASHBOARD")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProjectStatsByMonth([FromQuery] DateTime date)
        {
            var query = new GetProjectStatsByMonthQuery { Date = date };
            var result = await _mediator.Send(query);
            return ResponseOk(result);
        }

        [HttpGet("projects-by-year")]
        //[Authorize(Roles = "VIEW_DASHBOARD")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProjectStatsByYear([FromQuery] int year)
        {
            var query = new GetProjectStatsByYearQuery { Year = year };
            var result = await _mediator.Send(query);

            return ResponseOk(result);
        }

        [HttpGet("user-overview/{userId}")]
        //[Authorize(Roles = "VIEW_USER_DASHBOARD")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserOverview(Guid userId)
        {
            var query = new GetUserOverviewQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return ResponseOk(result);
        }


    }
}
