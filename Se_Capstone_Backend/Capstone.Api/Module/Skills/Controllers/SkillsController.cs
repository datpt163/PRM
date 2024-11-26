using Capstone.Application.Module.Skills.Command;
using Capstone.Api.Common.ResponseApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Capstone.Application.Module.Skills.Query;
using Capstone.Api.Module.Skills.Request;
using Microsoft.AspNetCore.Authorization;

namespace Capstone.Api.Module.skills.Controllers
{
    [ApiController]
    [Route("api/skills")]
    public class SkillsController : BaseController
    {
        private readonly IMediator _mediator;

        public SkillsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "CREATE_SKILL")]
        public async Task<IActionResult> Create([FromBody] CreateSkillCommand command)
        {
            var skillDto = await _mediator.Send(command);
            return ResponseCreated(skillDto, "Skill created successfully");
        }

        [HttpPut]
        [Authorize(Roles = "UPDATE_SKILL")]
        public async Task<IActionResult> Update([FromBody] UpdateSkillCommand command)
        {
            var skillDto = await _mediator.Send(command);
            if (skillDto == null)
            {
                return ResponseNotFound("Skill not found or has been deleted");
            }
            return ResponseOk(skillDto, "Skill updated successfully");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "DELETE_SKILL")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteSkillCommand { Id = id };
            var result = await _mediator.Send(command);
            if (!result)
            {
                return ResponseNotFound("Skill not found or already deleted");
            }
            return ResponseNoContent();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "CREATE_SKILL")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetSkillQuery { Id = id };
            var skillDto = await _mediator.Send(query);
            if (skillDto == null)
            {
                return ResponseNotFound("Skill not found or has been deleted");
            }
            return ResponseOk(skillDto, "Skill retrieved successfully");
        }

        [HttpGet]
        [Authorize(Roles = "GET_SKILL")]
        public async Task<IActionResult> GetList([FromQuery] GetSkillsListQuery request)
        {

            var response = await _mediator.Send(request);
            return ResponseOk(response.Data, response.Paging, "Skills retrieved successfully");
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "GET_SKILL")]
        public async Task<IActionResult> GetSkillsByUserId(Guid userId)
        {
            var query = new GetSkillsByUserIdQuery { UserId = userId };
            var skillResponses = await _mediator.Send(query);
            return ResponseOk(skillResponses, "Skills retrieved successfully");
        }

        [HttpDelete("user")]
        [Authorize(Roles = "SKILL_USER")]
        public async Task<IActionResult> RemoveSkillFromUser([FromBody] RemoveSkillFromUserRequest request)
        {
            try
            {
                var command = new RemoveSkillFromUserCommand
                {
                    UserId = request.UserId,
                    SkillId = request.SkillId
                };

                var result = await _mediator.Send(command);

                if (!result)
                {
                    return ResponseNotFound("User or skill not found, or skill not associated with the user");
                }

                return ResponseOk(result, "Remove skill success!!!");
            }
            catch (Exception ex)
            {
                return ResponseBadRequest(ex.Message);
            }
        }

        [HttpDelete("user/multiple")]
        [Authorize(Roles = "SKILL_USER")]
        public async Task<IActionResult> RemoveSkillsFromUser([FromBody] RemoveSkillsFromUserRequest request)
        {
            try
            {
                var command = new RemoveSkillsFromUserCommand
                {
                    UserId = request.UserId,
                    SkillIds = request.SkillIds
                };

                var result = await _mediator.Send(command);

                if (!result)
                {
                    return ResponseNotFound("User or skills not found, or skills not associated with the user");
                }

                return ResponseOk(result, "Removed skills successfully!!!");
            }
            catch (Exception ex)
            {
                return ResponseBadRequest(ex.Message);
            }
        }

        [HttpPost("user")]
        [Authorize(Roles = "SKILL_USER")]
        public async Task<IActionResult> AddSkillToUser([FromBody] AddSkillToUserRequest request)
        {
            try
            {
                var command = new AddSkillToUserCommand
                {
                    UserId = request.UserId,
                    SkillId = request.SkillId
                };

                var result = await _mediator.Send(command);

                if (!result)
                {
                    return ResponseNotFound("User not found, skill not found, or user already has the skill.");
                }

                return ResponseOk(result, "Skill added to user successfully!");
            }
            catch (Exception ex)
            {
                return ResponseBadRequest(ex.Message);
            }
        }

        [HttpPost("user/multiple")]
        [Authorize(Roles = "SKILL_USER")]
        public async Task<IActionResult> AddMultipleSkillsToUser([FromBody] AddMultipleSkillsToUserRequest request)
        {
            try
            {
                var command = new AddMultipleSkillsToUserCommand
                {
                    UserId = request.UserId,
                    SkillIds = request.SkillIds
                };

                var result = await _mediator.Send(command);
                return ResponseOk(result.Success, result.Message);
            }
            catch (Exception ex)
            {
                return ResponseBadRequest(ex.Message);
            }
        }

    }
}
