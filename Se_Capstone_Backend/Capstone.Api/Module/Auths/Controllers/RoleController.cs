using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Common.ResponseApi.Model;
using Capstone.Application.Module.Auths.Command;
using Capstone.Application.Module.Auths.Query;
using Capstone.Application.Module.Auths.Response;
using Capstone.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Capstone.Api.Module.Auths.Controllers
{
    [Route("api/roles")]
    [ApiController]
    public class RoleController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public RoleController(IMediator mediator, RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            _mediator = mediator;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(Roles = "ADD_ROLE")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand request)
        {
            var result = await _mediator.Send(request);

            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result.Data);
            return ResponseBadRequest(messageResponse: result.ErrorMessage);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "READ_DETAIL_ROLE")]
        public async Task<IActionResult> Detail(Guid id)
        {
            var result = await _mediator.Send(new RoleDetailQuery() { Id = id});

            if (string.IsNullOrEmpty(result.ErrorMessage))
                return ResponseOk(dataResponse: result.Data);
            return ResponseNotFound(messageResponse: result.ErrorMessage);
        }

        [HttpPut]
        [Authorize(Roles = "UPSERT_ROLE")]
        public async Task<IActionResult> CreateRole([FromBody] UpdateRoleCommand request)
        {
            var result = await _mediator.Send(request);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                if(result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
            return ResponseOk(dataResponse: result.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "DELETE_ROLE")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var result = await _mediator.Send(new DeleteRoleCommand() { Id = id});

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                if (result.StatusCode == 404)
                    return ResponseNotFound(messageResponse: result.ErrorMessage);
                return ResponseBadRequest(messageResponse: result.ErrorMessage);
            }
            return ResponseNoContent();
        }

        [HttpPost("add-role-for-user")]
        [SwaggerResponse(204, "Successful")]
        [SwaggerResponse(400, "Fail", typeof(ResponseFail))]
       
        public async Task<IActionResult> AddRoleToUser([FromBody] AddRoleForUserCommand request)
        {
            var result = await _mediator.Send(request);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                if (result.StatusCode == 400)
                    return ResponseBadRequest(messageResponse: result.ErrorMessage);
                return ResponseNotFound(messageResponse: result.ErrorMessage);
            }

            return ResponseOk(dataResponse: result.Data);

        }

        [HttpGet]
        [SwaggerResponse(200, "Successful", typeof(List<RoleDTO>))]
        [Authorize(Roles = "READ_LIST_ROLE")]
        public async Task<IActionResult> GetList([FromQuery] GetListRoleQuery request)
        {
            var result = await _mediator.Send(request);
            return ResponseOk(dataResponse: result.Data);
        }
    }
}
