using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Phases.Request;
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

        public PhaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> CreatePhase([FromBody] CreatePhaseCommand request)
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

        [HttpPut("{id}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> UpdatePhase(Guid id, [FromBody] UpdatePhaseRequest request)
        {
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
