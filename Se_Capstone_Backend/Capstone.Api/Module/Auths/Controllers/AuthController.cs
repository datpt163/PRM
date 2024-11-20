using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Api.Module.Auths.Request;
using Capstone.Application.Module.Auth.Query;
using Capstone.Application.Module.Auth.Response;
using Capstone.Application.Module.Auths.Command;
using Capstone.Application.Module.Auths.Query;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Capstone.Api.Module.Auth.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenQuery query)
        {
            var result = await _mediator.Send(query);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result.Data);

            return ResponseBadRequest(messageResponse: result.ErrorMessage);
        }

        [SwaggerResponse(200, "Successful", typeof(ResponseSuccess<LoginResponse>))]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginQuery googleLoginQuery)
        {
            var result = await _mediator.Send(googleLoginQuery);

            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result.Data, "Login Successful!");
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [SwaggerResponse(200, "Successful", typeof(ResponseSuccess<LoginResponse>))]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [HttpPost("login")]
        public async Task<IActionResult> Auth([FromBody] LoginQuery loginQuery)
        {
            var result = await _mediator.Send(loginQuery);

            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result.Data);
            else
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
        }

        [SwaggerResponse(204, "Successful")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ResetPasswordRequest request)
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _mediator.Send(new ChangePasswordCommand(token, request.OldPassword, request.NewPassword));

            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            return ResponseBadRequest(messageResponse: result.ErrorMessage);
        }

        [SwaggerResponse(204, "Logout successful")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [HttpPost("logout")]
        [Authorize] 
        public async Task<IActionResult> Logout()
        {
            string token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var logoutQuery = new LogoutQuery { Token = token }; 
            var result = await _mediator.Send(logoutQuery);

            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result);
            return ResponseBadRequest(messageResponse: result.ErrorMessage);
        }

        [HttpPost("forgot-password")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [SwaggerResponse(204, "Success")]
        public async Task<IActionResult> SendEmailForgotPass([FromBody] ForgotPasswordQuery forgotPasswordQuery)
        {
            var result = await _mediator.Send(forgotPasswordQuery);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            return ResponseBadRequest(messageResponse: result.ErrorMessage);
        }

        [HttpPost("reset-password")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [SwaggerResponse(204, "Success")]
        public async Task<IActionResult> ResetPassWord([FromBody] ResetPasswordCommand request)
        {
            var result = await _mediator.Send(request);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseNoContent();
            return ResponseBadRequest(messageResponse: result.ErrorMessage);
        }

        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [HttpPost("admin-change-password")]
        [Authorize(Roles = "CHANGE_PASSWORD_USER")]
        public async Task<IActionResult> AdminChange([FromBody] ChangePassUserCommand request)
        {
            var result = await _mediator.Send(request);
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                if(result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
            return ResponseNoContent();
        }

        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
        [HttpPost("check-code")]
        public async Task<IActionResult> CheckCode([FromBody] CheckCodeQuery request)
        {
            var result = await _mediator.Send(request);
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
            return ResponseNoContent();
        }
    }
}
