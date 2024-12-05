using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Statuses.Requests;
using Capstone.Application.Common.ProjectAuthorize;
using Capstone.Application.Module.Status.Command;
using Capstone.Application.Module.Status.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Capstone.Api.Module.Statuses.Controllers
{
    [Route("api/statuses")]
    [ApiController]
    public class StatusController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IManagePermissionProject _managePermissionProject;

        public StatusController(IMediator mediator, IManagePermissionProject managePermissionProject)
        {
            _managePermissionProject = managePermissionProject;
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> CreateStatus([FromBody] CreateStatusCommand request)
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

        [HttpPost("default")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize(Roles = "ADD_DEFAULT_STATUS")]
        public async Task<IActionResult> CreateStatusDefault([FromBody] CreateStatusDefaultCommand request)
        {
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

        [HttpGet]
        public async Task<IActionResult> GetListStatus(Guid? projectId)
        {
            var result = await _mediator.Send(new GetListStatusQuery() { projectId = projectId });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data);
            else
            {
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpGet("default")]
        [Authorize(Roles = "READ_DEFAULT_STATUS")]
        public async Task<IActionResult> GetListStatusDefault()
        {
            var result = await _mediator.Send(new GetListStatusDefaultQuery());
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data);
            else
            {
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> DeleteStatus(Guid id, [FromBody] DeleteStatusRequest newStatus)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            (bool isAuthorize, int status) = await _managePermissionProject.IsAuthorizedAsync(token, "IsProjectConfigurator", statusId: id);
            if (status == 404)
                return ResponseNotFound(messageResponse: "Not found");

            if (!isAuthorize)
                return Forbid();
            var result = await _mediator.Send(new DeleteStatusCommand() { Id = id, NewStatusId = newStatus.newStatusId });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpDelete("default/{id}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize(Roles = "DELETE_DEFAULT_STATUS")]
        public async Task<IActionResult> DeleteStatusDefault(Guid id)
        {
            var result = await _mediator.Send(new DeleteStatusDefaultCommand() { Id = id });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
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
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            (bool isAuthorize, int status) = await _managePermissionProject.IsAuthorizedAsync(token, "IsProjectConfigurator", statusId: id);
            if (status == 404)
                return ResponseNotFound(messageResponse: "Not found");

            if (!isAuthorize)
                return Forbid();
            var result = await _mediator.Send(new UpdateStatusCommand() { Id = id, Name = request.Name, Description = request.Description, Color = request.Color, IsDone = request.IsDone });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data);
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }


        [HttpPut("default/{id}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize(Roles = "UPDATE_DEFAULT_STATUS")]
        public async Task<IActionResult> UpdateStatusDefault(Guid id, [FromBody] UpdateStatusDefaultRequest request)
        {
            var result = await _mediator.Send(new UpdateStatusDefaultCommand() { Id = id, Name = request.Name, Description = request.Description, Color = request.Color, IsDone = request.IsDone});
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data);
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }
    }
}
