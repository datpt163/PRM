using Capstone.Application.Module.Labels.Command;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Application.Module.Notifications.Query;
using Capstone.Application.Module.Notifications.Command;

namespace Capstone.Api.Module.Notification
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetMyNotification(int? pageIndex, int? pageSize)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new GetListNotificationQuery() { PageIndex = pageIndex , PageSize = pageSize, Token = token});
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data, result.Paging);
            else
            {
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpPost("mark-read")]
        [Authorize]
        public async Task<IActionResult> MarkRead(NotificationRequest request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new MarkReadNotificationCommand() { Ids = request.Ids, Token = token });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            else
            {
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new DeleteNotificationCommand() { Id = id, Token = token });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            else
            {
                if(result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return Forbid();
            }
        }
    }
}
