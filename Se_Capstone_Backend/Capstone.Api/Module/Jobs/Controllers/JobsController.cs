using Capstone.Application.Module.Jobs.Command;
using Capstone.Api.Common.ResponseApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Capstone.Application.Module.Jobs.Query;
using Capstone.Api.Resources;


namespace Capstone.Api.Module.Jobs.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController : BaseController
    {
        private readonly IMediator _mediator;

        public JobsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJobCommand command)
        {
            var jobDto = await _mediator.Send(command);
            return ResponseCreated(jobDto, Messages.job_created_successfully);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateJobCommand command)
        {
            var jobDto = await _mediator.Send(command);
            if (jobDto == null)
            {
                return ResponseNotFound(Messages.job_not_found_or_deleted);
            }
            return ResponseOk(jobDto, Messages.job_updated_successfully);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteJobCommand { Id = id };
            var result = await _mediator.Send(command);
            if (!result)
            {
                return ResponseNotFound(Messages.job_not_found_or_deleted);
            }
            return ResponseNoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetJobQuery { Id = id };
            var jobDto = await _mediator.Send(query);
            if (jobDto == null)
            {
                return ResponseNotFound(Messages.job_not_found_or_deleted);
            }
            return ResponseOk(jobDto, Messages.job_retrieved_successfully);
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] GetJobsListQuery request)
        {

            var response = await _mediator.Send(request);
            return ResponseOk(response.Data, response.Paging, Messages.job_retrieved_successfully);
        }


    }
}
