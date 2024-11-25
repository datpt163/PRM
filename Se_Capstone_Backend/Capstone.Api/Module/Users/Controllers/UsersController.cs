using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Users.Models;
using Capstone.Application.Module.Auths.Query;
using Capstone.Application.Module.Users.Command;
using Capstone.Application.Module.Users.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;


namespace Capstone.Api.Module.Users.Controllers
{
    [ApiController]
    [Route("api/users/")]
    public class UsersController : BaseController
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "GET_USER")]
        public async Task<IActionResult> GetUsers([FromBody] GetUserListQuery query)
        {
            var response = await _mediator.Send(query);
            
            return ResponseOk(response.Data, response.Paging);

        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "GET_USER")]
        public async Task<IActionResult> GetUserDetail(Guid id)
        {
            var query = new GetUserDetailQuery(id);
            var user = await _mediator.Send(query);
            if (user == null)
            {
                return ResponseNotFound("User not found");
            }
            return ResponseOk(dataResponse: user);
        }

        [HttpPut]
        [Authorize(Roles = "UPDATE_USER")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserRequest request)
        {
            var command = new UpdateUserCommand
            {
                Id = request.Id,
                AvatarFile = request.Avatar,
                FullName = request.FullName,
                Phone = request.Phone,
                Address = request.Address,
                Gender = request.Gender,
                Dob = request.Dob,
                BankAccount = request.BankAccount,
                BankAccountName = request.BankAccountName,
                Status = request.Status,
                RoleId = request.RoleId,
            };

            var updatedUser = await _mediator.Send(command);

            if (updatedUser == null)
            {
                return ResponseNotFound("User not found or update failed.");
            }

            return ResponseOk(dataResponse: updatedUser);
        }

        [HttpPut("toggle-status")]
        [Authorize(Roles = "TOGGLE_USER")]
        public async Task<IActionResult> ToggleUserStatus([FromForm] ToggleUserStatusRequest request)
        {
            var command = new ToggleUserStatusCommand
            {
                UserId = request.UserId
            };

            var isToggled = await _mediator.Send(command);

            if (!isToggled)
            {
                return ResponseNotFound("User not found or status toggle failed.");
            }

            return ResponseOk(isToggled ,"User status updated successfully.");
        }


        [HttpGet("get-by-permission")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        public async Task<IActionResult> GetByPermission(string permissionName)
        {
            var result = await _mediator.Send(new GetUserByPermissionQuery() { permissionName = permissionName });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result.Data);
            else
            {
                if(result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [HttpPost("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveUsers([FromBody] GetActiveUsersRequest request)
        {
            var query = new GetActiveUsersQuery
            {
                UserInProject = request.UserInProject,
            };
            var response = await _mediator.Send(query);

            return ResponseOk(response); 
        }
    }
}