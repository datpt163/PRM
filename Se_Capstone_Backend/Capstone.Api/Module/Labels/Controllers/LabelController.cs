using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Labels.Requests;
using Capstone.Application.Module.Labels.Command;
using Capstone.Application.Module.Labels.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Capstone.Api.Module.Labels.Controllers
{
    [Route("api/labels")]
    [ApiController]
    public class LabelController : BaseController
    {
        private readonly IMediator _mediator;

        public LabelController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> CreateLabel([FromBody] CreateLabelCommand request)
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

        [HttpPost("default")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize(Roles = "ADD_DEFAULT_LABEL")]
        public async Task<IActionResult> CreateLabelDefault([FromBody] CreateLabelDefaultCommand request)
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
        public async Task<IActionResult> GetListLabel(Guid? projectId)
        {
            var result = await _mediator.Send(new GetListLabelQuery() { projectId = projectId });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data);
            else
            {
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpGet("default")]
        [Authorize(Roles = "READ_DEFAULT_LABEL")]
        public async Task<IActionResult> GetListLabelDefault()
        {
            var result = await _mediator.Send(new GetListLabelDefaultQuery());
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
        public async Task<IActionResult> DeleteLabel(Guid id)
        {
            var result = await _mediator.Send(new DeleteLabelCommand() { Id = id });
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
        [Authorize(Roles = "DELETE_DEFAULT_LABEL")]
        public async Task<IActionResult> DeleteLabelDefault(Guid id)
        {
            var result = await _mediator.Send(new DeleteDefaultLabelCommand() { Id = id });
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
        public async Task<IActionResult> UpdateLabel(Guid id, [FromBody] UpdateLabelRequest request)
        {
            var result = await _mediator.Send(new UpdateLabelCommand() { Id = id, Title = request.Title, Description = request.Description });
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
        [Authorize(Roles = "UPDATE_DEFAULT_LABEL")]
        public async Task<IActionResult> UpdateDefaultLabel(Guid id, [FromBody] UpdateLabelDefaultRequest request)
        {
            var result = await _mediator.Send(new UpdateDefaultLabelCommand() { Id = id, Title = request.Title, Description = request.Description });
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
