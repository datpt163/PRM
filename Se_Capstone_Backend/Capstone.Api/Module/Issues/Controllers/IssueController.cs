using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Issues.Request;
using Capstone.Api.Module.Statuses.Requests;
using Capstone.Application.Module.Issues.Command;
using Capstone.Application.Module.Issues.Query;
using Capstone.Application.Module.Status.Command;
using Capstone.Application.Module.Status.Query;
using Capstone.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Capstone.Api.Resources;
using Microsoft.AspNetCore.SignalR;
using Capstone.Api.Module.Statuses.SignalR;
using System.Text.Json;

namespace Capstone.Api.Module.Issues.Controllers
{
    [Route("api/issues")]
    [ApiController]
    public class IssueController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IHubContext<StatusHub> _statusHubContext;

        public IssueController(IMediator mediator, IHubContext<NotificationHub> context, IHubContext<StatusHub> statusHubContext)
        {
            _hubContext = context;
            _mediator = mediator;
            _statusHubContext = statusHubContext;
        }

        [HttpPost]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> CreateStatus([FromBody] CreateIssueRequest request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new AddIssueCommand(token, request.Title, request.Description, request.StartDate, request.DueDate, request.Priority, request.EstimatedTime, request.ParentIssueId, request.AssigneeId, request.StatusId, request.LabelId));
            if (result.StatusCode == 403)
                return Forbid();

            if (result.StatusCode == 205 || (result.StatusCode == 200))
            {
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    try
                    {
                        var ids = JsonSerializer.Deserialize<List<Guid>>(result.ErrorMessage);
                        foreach (var userIdd in (ids == null ? new List<Guid>() : ids))
                        {
                            await _hubContext.Clients.Group(userIdd + "")
                                                   .SendAsync("NotificationResponse", "Success");
                            await _statusHubContext.Clients.Group(userIdd + "")
                                                 .SendAsync("IssueOrderResponse", "Success");
                        }
                    }
                    catch
                    {
                    }
                }
                return ResponseOk(result.Data);
            }
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetListStatus(Guid? projectId, int? pageIndex, [FromQuery] int? pageSize, string? title, Priority? priority, [FromQuery] List<Guid>? assigneeIds, [FromQuery] Guid? reporterId, [FromQuery] List<Guid>? statusIds, [FromQuery] List<Guid>? labelIds, [FromQuery] List<Guid>? phaseIds, DateTime? startDate, DateTime? dueDate)
        {
            var result = await _mediator.Send(new GetListIssuesQuery() { StartDate = startDate, DueDate = dueDate, ProjectId = projectId, PageIndex = pageIndex, PageSize = pageSize, Title = title, Priority = priority, AssigneeId = assigneeIds, ReporterId = reporterId, StatusId = statusIds, LabelId = labelIds, PhaseId = phaseIds }); ;
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data, result.Paging);
            else
            {
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpGet("kanban")]
        public async Task<IActionResult> GetListStatus(Guid? projectId, string? title, Priority? priority, [FromQuery] List<Guid>? assigneeIds, [FromQuery] Guid? reporterId, [FromQuery] List<Guid>? statusIds, [FromQuery] List<Guid>? labelIds, [FromQuery] List<Guid>? phaseIds, DateTime? startDate, DateTime? dueDate)
        {
            var result = await _mediator.Send(new GetListStatusKanbanQuery() { StartDate = startDate, DueDate = dueDate, projectId = projectId, Title = title, Priority = priority, AssigneeId = assigneeIds, ReporterId = reporterId, StatusId = statusIds, LabelId = labelIds, PhaseId = phaseIds });
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
        public async Task<IActionResult> DeleteStatus(Guid id)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new DeleteIssueCommand() { Id = id, Token = token});
            if (result.StatusCode == 403)
                return Forbid();

            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpGet("{id}")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> GetDetailIssue(Guid id)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new GetDetailIssueQuery() { Id = id, Token = token });
            if (result.StatusCode == 403)
                return Forbid();
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
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateIssueRequest request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new UpdateIssueCommand(id, token, request.Title, request.Description, request.StartDate, request.DueDate, request.Percentage, request.Priority, request.EstimatedTime, request.ParentIssueId, request.AssigneeId, request.StatusId, request.LabelId) { PhaseId = request.PhaseId, ActualTime = request.ActualTime, ActualDate = request.ActualDate, ReporterId = request.ReporterId });
            if (result.StatusCode == 403)
                return Forbid();

            if (result.StatusCode == 200)
            {
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    try
                    {
                        var ids = JsonSerializer.Deserialize<List<Guid>>(result.ErrorMessage);
                        foreach (var userIdd in (ids == null ? new List<Guid>() : ids))
                        {
                            await _hubContext.Clients.Group(userIdd + "")
                                                   .SendAsync("NotificationResponse", "Success");
                            await _statusHubContext.Clients.Group(userIdd + "")
                                               .SendAsync("IssueOrderResponse", "Success");
                        }
                    }
                    catch
                    {
                    }
                }
                return ResponseOk(result.Data);
            }
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }
    }
}
