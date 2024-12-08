using Capstone.Application.Module.Positions.Command;
using Capstone.Api.Common.ResponseApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Capstone.Application.Module.Positions.Query;
using Capstone.Api.Module.Positions.Request;
using Microsoft.AspNetCore.Authorization;
using Capstone.Api.Resources;

namespace Capstone.Api.Module.positions.Controllers
{
    [ApiController]
    [Route("api/positions")]
    public class PositionsController : BaseController
    {
        private readonly IMediator _mediator;

        public PositionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "CREATE_POSITION")]
        public async Task<IActionResult> Create([FromBody] CreatePositionCommand command)
        {
            var positionDto = await _mediator.Send(command);
            return ResponseCreated(positionDto, Messages.position_created_successfully);
        }

        [HttpPut]
        [Authorize(Roles = "UPDATE_POSITION")]

        public async Task<IActionResult> Update([FromBody] UpdatePositionCommand command)
        {
            var positionDto = await _mediator.Send(command);
            if (positionDto == null)
            {
                return ResponseNotFound(Messages.position_not_found_or_deleted);
            }
            return ResponseOk(positionDto, Messages.position_updated_successfully);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "DELETE_POSITION")]

        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeletePositionCommand { Id = id };
            var result = await _mediator.Send(command);
            if (!result)
            {
                return ResponseNotFound(Messages.position_not_found_or_deleted);
            }
            return ResponseNoContent();
        }

        [HttpGet("{id}")]
        //[Authorize(Roles = "GET_POSITION")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetPositionQuery { Id = id };
            var positionDto = await _mediator.Send(query);
            if (positionDto == null)
            {
                return ResponseNotFound(Messages.position_not_found_or_deleted);
            }
            return ResponseOk(positionDto, Messages.position_retrieved_successfully);
        }

        [HttpGet]
        //[Authorize(Roles = "GET_POSITION")]
        public async Task<IActionResult> GetList([FromQuery] GetPositionsListQuery request)
        {

            var response = await _mediator.Send(request);
            return ResponseOk(response.Data, response.Paging, Messages.position_retrieved_successfully);
        }

        [HttpGet("user/{userId}")]
        //[Authorize(Roles = "USER_POSITION")]
        public async Task<IActionResult> GetPositionsByUserId(Guid userId)
        {
            var query = new GetPositionsByUserIdQuery { UserId = userId };
            var positionResponses = await _mediator.Send(query);
            return ResponseOk(positionResponses, "Positions retrieved successfully");
        }

        [HttpDelete("user")]
        //[Authorize(Roles = "USER_POSITION")]
        public async Task<IActionResult> RemovePositionFromUser([FromBody] RemovePositionFromUserRequest request)
        {
            try
            {
                var command = new RemovePositionFromUserCommand
                {
                    UserId = request.UserId,
                    PositionId = request.PositionId
                };

                var result = await _mediator.Send(command);

                if (!result)
                {
                    return ResponseNotFound("User or position not found, or position not associated with the user");
                }

                return ResponseOk(result, "Remove position success!!!");
            }
            catch (Exception ex)
            {
                return ResponseBadRequest(ex.Message);
            }
        }

        [HttpDelete("user/multiple")]
        //[Authorize(Roles = "USER_POSITION")]
        public async Task<IActionResult> RemovePositionsFromUser([FromBody] RemovePositionsFromUserRequest request)
        {
            try
            {
                var command = new RemovePositionsFromUserCommand
                {
                    UserId = request.UserId,
                    PositionIds = request.PositionIds
                };

                var result = await _mediator.Send(command);

                if (!result)
                {
                    return ResponseNotFound("User or positions not found, or positions not associated with the user");
                }

                return ResponseOk(result, "Removed positions successfully!!!");
            }
            catch (Exception ex)
            {
                return ResponseBadRequest(ex.Message);
            }
        }

        [HttpPost("user")]
        //[Authorize(Roles = "USER_POSITION")]
        public async Task<IActionResult> AddPositionToUser([FromBody] AddPositionToUserRequest request)
        {
            try
            {
                var command = new AddPositionToUserCommand
                {
                    UserId = request.UserId,
                    PositionId = request.PositionId
                };

                var result = await _mediator.Send(command);

                if (!result)
                {
                    return ResponseNotFound("User not found, position not found, or user already has the position.");
                }

                return ResponseOk(result, "Position added to user successfully!");
            }
            catch (Exception ex)
            {
                return ResponseBadRequest(ex.Message);
            }
        }

        [HttpPost("user/multiple")]
        //[Authorize(Roles = "USER_POSITION")]
        public async Task<IActionResult> AddMultiplePositionsToUser([FromBody] AddMultiplePositionsToUserRequest request)
        {
            try
            {
                var command = new AddMultiplePositionsToUserCommand
                {
                    UserId = request.UserId,
                    PositionIds = request.PositionIds
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
