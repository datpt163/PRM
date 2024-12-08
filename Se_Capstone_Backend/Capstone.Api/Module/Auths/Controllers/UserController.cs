using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Auths.Request;
using Capstone.Api.Resources;
using Capstone.Application.Module.Auth.Command;
using Capstone.Application.Module.Auths.Query;
using Capstone.Application.Module.Auths.Response;
using Capstone.Application.Module.Users.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Capstone.Api.Module.Auths.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : BaseController
    {

        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerResponse(200, "Success", typeof(CreateUserResponse))]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize(Roles = "ADD_USER")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterRequest request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new RegisterCommand(request.RoleId, token, request.Email, request.Password, request.UserName, request.FullName, request.Address, request.Gender, request.Dob, request.Phone));
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                if(result.StatusCode == 404)
                    return ResponseNotFound(result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);

            }
            return ResponseOk(result.Data);
        }

        [HttpGet("profile")]
        [SwaggerResponse(200, "Success", typeof(RegisterResponse))]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new ProfileQuery() { Token = token });
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(result.Data);
            return ResponseBadRequest(messageResponse: result.ErrorMessage);
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserRequest request)
        {
            var command = new UpdateUserCommand
            {
                AvatarFile = request.Avatar,
                FullName = request.FullName,
                Phone = request.Phone,
                Address = request.Address, 
                Gender = request.Gender,   
                Dob = request.Dob,         
                BankAccount = request.BankAccount,  
                BankAccountName = request.BankAccountName
            };

            var updatedUser = await _mediator.Send(command);

            if (updatedUser == null)
            {
                return ResponseNotFound(Messages.user_not_found_or_update_failed);
            }

            return ResponseOk(dataResponse: updatedUser);
        }
    }
}
