using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Phases.Request;
using Capstone.Application.Common.ProjectAuthorize;
using Capstone.Application.Module.Phase.Command;
using Capstone.Application.Module.Phase.Query;
using Capstone.Application.Module.Status.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Capstone.Api.Module.Phases.Controllers
{
    [Route("api/phases")]
    [ApiController]
    public class PhaseController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IManagePermissionProject _managePermissionProject;

        public PhaseController(IMediator mediator, IManagePermissionProject managePermissionProject)
        {
            _managePermissionProject = managePermissionProject;
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> CreatePhase([FromBody] CreatePhaseCommand request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            (bool isAuthorize, int status) = await _managePermissionProject.IsAuthorizedAsync(token, "IsProjectConfigurator", projectId: request.ProjectId);
            if (status == 404)
                return ResponseNotFound(messageResponse: "Not found");

            if (!isAuthorize)
                return Forbid();
            var result = await _mediator.Send(request);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data);
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);

            }
        }

        [HttpPut("{id}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> UpdatePhase(Guid id, [FromBody] UpdatePhaseRequest request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            (bool isAuthorize, int status) = await _managePermissionProject.IsAuthorizedAsync(token, "IsProjectConfigurator", phaseId: id);
            if (status == 404)
                return ResponseNotFound(messageResponse: "Not found");

            if (!isAuthorize)
                return Forbid();
            var result = await _mediator.Send(new UpdatePhaseCommand() { Id = id, Title = request.Title, Description = request.Description, ExpectedEndDate = request.ExpectedEndDate, ExpectedStartDate = request.ExpectedStartDate});
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data);
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> UpdatePhase(Guid id)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            (bool isAuthorize, int status) = await _managePermissionProject.IsAuthorizedAsync(token, "IsProjectConfigurator", phaseId: id);
            if (status == 404)
                return ResponseNotFound(messageResponse: "Not found");

            if (!isAuthorize)
                return Forbid();
            var result = await _mediator.Send(new DeletePhaseCommand() { Id = id});
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpPut("complete")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> CompletePhase(Guid projectId)
        {
            var result = await _mediator.Send(new CompletePhaseCommand() {ProjectId = projectId });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetListPhase(Guid projectId)
        {
            var result = await _mediator.Send(new GetListPhaseQuery() { ProjectId = projectId });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data);
            else
            {
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }
    }
}
