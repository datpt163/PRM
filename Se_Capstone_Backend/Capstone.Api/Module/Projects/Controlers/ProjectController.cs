using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Projects.Request;
using Capstone.Application.Module.Auths.Command;
using Capstone.Application.Module.Projects.Command;
using Capstone.Application.Module.Projects.Query;
using Capstone.Application.Module.Projects.Request;
using Capstone.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Capstone.Api.Module.Projects.Controlers
{
    [Route("api/projects")]
    [ApiController]
    public class ProjectController : BaseController
    {
        private readonly IMediator _mediator;

        public ProjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize(Roles = "ADD_PROJECT")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand request)
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
        [Authorize(Roles = "UPDATE_PROJECT")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
        {
            var result = await _mediator.Send(new UpdateProjectCommand(id, request.Name, request.Code, request.Description, request.StartDate, request.EndDate, request.LeadId, request.Status));
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
        [Authorize]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        public async Task<IActionResult> GetListProject(int? pageIndex,int? pageSize, bool? isVisible, ProjectStatus? status, string? search )
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new GetListProjectQuery(pageIndex, pageSize, isVisible, status, token, search));
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data, result.Paging);
            else
            {
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize(Roles = "DELETE_PROJECT")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var result = await _mediator.Send(new DeleteProjectCommand() { Id = id});
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            else
            {
                return ResponseNotFound(messageResponse: result.ErrorMessage);
            }
        }

        [HttpGet("{id}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        public async Task<IActionResult> GetProject(Guid id)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new GetDetailProjectQuery() { Id = id, Token = token });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result.Data);
            else
            {
                return ResponseNotFound(messageResponse: result.ErrorMessage);
            }
        }

        [HttpPut("{id}/visible/toggle")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize(Roles = "TOGGLE_VISIBLE_PROJECT")]
        public async Task<IActionResult> ToggleVisible(Guid id)
        {
            var result = await _mediator.Send(new ToggleProjectCommand() { Id = id });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result.Data);
            else
            {
                return ResponseNotFound(messageResponse: result.ErrorMessage);
            }
        }

        [HttpPost("members")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize(Roles = "ADD_MEMBER_TO_PROJECT")]
        public async Task<IActionResult> AddMember(AddMembersToProject request)
        {
            var result = await _mediator.Send(request);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            else
            {
                if(result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                else
                    return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpPut("{projectId}/members/{memberId}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        //[Authorize(Roles = "ADD_MEMBER_TO_PROJECT")]
        public async Task<IActionResult> UpdateMember(Guid projectId, Guid memberId, UpdateMemberRequest request)
        {
            var result = await _mediator.Send(new UpdateMemberCommand() {IsCommentConfigurator = request.IsCommentConfigurator, IsIssueConfigurator = request.IsIssueConfigurator, IsProjectConfigurator = request.IsProjectConfigurator, PositionId = request.PositionId, ProjectId = projectId, UserId = memberId});
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result.Data);
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                else
                    return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }


        [HttpPost("calculate-effort")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [AllowAnonymous]
        public async Task<IActionResult> CalculateEffort([FromBody] ProjectEffortCalculationRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ProjectName) || request.Tasks == null || !request.Tasks.Any())
            {
                return ResponseBadRequest("Invalid input data.");
            }

            var result = await _mediator.Send(new CalculateEffortMetricsQuery
            {
                ProjectName = request.ProjectName,
                Tasks = request.Tasks,
                IsCalculateDetails = request.IsCalculateDetails
            });

            return ResponseOk(result);
        }

        [HttpPost("details")]
        public async Task<IActionResult> GetProjectDetails([FromBody] GetProjectDetailsRequest request)
        {
            if (request == null || request.ProjectId == Guid.Empty)
            {
                return ResponseBadRequest("Invalid input data.");
            }

            var result = await _mediator.Send(new GetProjectDetailsQuery
            {
                ProjectId = request.ProjectId,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            });
            return ResponseOk(result);
        }

        [HttpPost("suggest")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSuggestProject([FromBody] SuggestInvMemberRequest request)
        {
            var query = new SuggestProjectQuery
            {
                ProjectName = request.ProjectName,
                ProjectDetail = request.ProjectDetail,
                UserStatistics = request.UserStatistics
            };

            var result = await _mediator.Send(query);

            return ResponseOk(result, "Successfully retrieved suggestions!");
        }

    }
}
