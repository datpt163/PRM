using Capstone.Application.Module.Skills.Command;
using Capstone.Api.Common.ResponseApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Capstone.Application.Module.Skills.Query;
using Capstone.Api.Module.Skills.Request;
using Microsoft.AspNetCore.Authorization;
using Capstone.Api.Resources;

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
            return ResponseCreated(skillDto, Messages.skill_created_successfully);
        }

        [HttpPut]
        [Authorize(Roles = "UPDATE_SKILL")]
        public async Task<IActionResult> Update([FromBody] UpdateSkillCommand command)
        {
            var skillDto = await _mediator.Send(command);
            if (skillDto == null)
            {
                return ResponseNotFound(Messages.skill_not_found_or_deleted);
            }
            return ResponseOk(skillDto, Messages.skill_updated_successfully);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "DELETE_SKILL")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteSkillCommand { Id = id };
            var result = await _mediator.Send(command);
            if (!result)
            {
                return ResponseNotFound(Messages.skill_not_found_or_deleted);
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
                return ResponseNotFound(Messages.skill_not_found_or_deleted);
            }
            return ResponseOk(skillDto, Messages.skill_retrieved_successfully);
        }

        [HttpGet]
        [Authorize(Roles = "GET_SKILL")]
        public async Task<IActionResult> GetList([FromQuery] GetSkillsListQuery request)
        {

            var response = await _mediator.Send(request);
            return ResponseOk(response.Data, response.Paging, Messages.skill_retrieved_successfully);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "GET_SKILL")]
        public async Task<IActionResult> GetSkillsByUserId(Guid userId)
        {
            var query = new GetSkillsByUserIdQuery { UserId = userId };
            var skillResponses = await _mediator.Send(query);
            return ResponseOk(skillResponses, Messages.skill_retrieved_successfully);
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
                    return ResponseNotFound(Messages.user_or_skill_not_found_or_not_associated);
                }

                return ResponseOk(result, Messages.removed_skills_successfully);
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
                    return ResponseNotFound(Messages.user_or_skill_not_found_or_not_associated);
                }

                return ResponseOk(result, Messages.removed_skills_successfully);
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
                    return ResponseNotFound(Messages.user_or_skill_not_found_or_not_associated);
                }

                return ResponseOk(result, Messages.skill_added_to_user_successfully);
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

        [HttpPost("user/by-skill-title")]
        [AllowAnonymous]
        [Authorize(Roles = "SKILL_USER")]
        public async Task<IActionResult> GetUsersBySkillTitle([FromBody] GetUsersBySkillTitleRequest request)
        {
            try
            {
                var query = new GetUsersBySkillTitleQuery
                {
                    SkillTitle = request.SkillTitle,
                };

                var result = await _mediator.Send(query);

                return ResponseOk(result);
            }
            catch (Exception ex)
            {
                return ResponseBadRequest(ex.Message);
            }
        }

    }
}
