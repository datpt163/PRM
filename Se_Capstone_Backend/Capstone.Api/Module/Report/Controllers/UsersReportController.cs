using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Module.Projects.Request;
using Capstone.Api.Module.Report.Request;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Report.Query;
using Capstone.Application.Module.Report.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Capstone.Api.Module.Report.Controllers
{
    [Route("api/users/reports/")]
    [ApiController]
    public class UsersReportController : BaseController
    {
        private readonly IMediator _mediator;

        public UsersReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("skills")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSkills([FromBody] GetSkillsRequest request)
        {
            var query = new GetSkillsReportQuery
            {
                Title = request.Title,
                MinimumUsers = request.MinimumUsers,
                MaximumUsers = request.MaximumUsers,
                UserId = request.UserId
            };

           var result = await _mediator.Send(query);

            return ResponseOk(result, "");
        }
    }
}