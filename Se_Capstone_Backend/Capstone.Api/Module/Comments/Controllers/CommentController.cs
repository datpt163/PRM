using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Comments.Request;
using Capstone.Api.Module.Labels.Requests;
using Capstone.Api.Module.Statuses.SignalR;
using Capstone.Application.Common.ProjectAuthorize;
using Capstone.Application.Module.Comments.Command;
using Capstone.Application.Module.Labels.Command;
using Capstone.Application.Module.Labels.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace Capstone.Api.Module.Comments.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IManagePermissionProject _managePermissionProject;

        public CommentController(IMediator mediator, IHubContext<NotificationHub> hubContext, IManagePermissionProject managePermissionProject)
        {
            _managePermissionProject = managePermissionProject;
            _hubContext = hubContext;
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromBody] AddCommentRequest request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new AddCommentCommand() { Content = request.Content, IssueId = request.IssueId, Token = token});
            if (result.StatusCode == 200)
            {
                return ResponseOk(result.Data);
            }
            else if(result.StatusCode == 205)
            {
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    var ids = JsonSerializer.Deserialize<List<Guid>>(result.ErrorMessage);
                    foreach (var id in (ids == null ? new List<Guid>() : ids))
                    {
                        await _hubContext.Clients.Group(id + "")
                                               .SendAsync("NotificationResponse", "Success");
                    }
                    return ResponseOk(result.Data);
                }
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new DeleteCommentCommand() { Id = id, Token = token });
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
        public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new UpdateCommentCommand() { Id = id, Content = request.Content, Token = token });
            if (result.StatusCode == 200)
            {
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    var ids = JsonSerializer.Deserialize<List<Guid>>(result.ErrorMessage);
                    foreach (var userId in (ids == null ? new List<Guid>() : ids))
                    {
                        await _hubContext.Clients.Group(userId + "")
                                               .SendAsync("NotificationResponse", "Success");
                    }
                    return ResponseOk(result.Data);
                }
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
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
