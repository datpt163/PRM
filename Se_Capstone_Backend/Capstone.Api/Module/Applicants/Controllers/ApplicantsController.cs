using Capstone.Api.Common.ResponseApi.Controllers;
using Capstone.Api.Resources;
using Capstone.Application.Module.Applicants.Command;
using Capstone.Application.Module.Applicants.Query;
using Capstone.Application.Module.Applicants.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Capstone.Api.Module.Applicants.Controllers
{
    [ApiController]
    [Route("api/applicants")]
    public class ApplicantsController : BaseController
    {
        private readonly IMediator _mediator;

        public ApplicantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicants([FromQuery] GetApplicantListQuery query)
        {
            var response = await _mediator.Send(query);
            return ResponseOk(response.Data, response.Paging, Messages.ApplicantsRetrievedSuccessfully);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetApplicantDetail(Guid id)
        {
            var query = new GetApplicantDetailQuery(id);
            var applicant = await _mediator.Send(query);
            if (applicant == null)
            {
                return ResponseNotFound(Messages.ApplicantNotFound);
            }
            return ResponseOk(dataResponse: applicant, Messages.ApplicantRetrievedSuccessfully);
        }

        [HttpDelete("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteApplicant(Guid id)
        {
            var command = new DeleteApplicantCommand(id);
            var result = await _mediator.Send(command);

            if (result == null)
            {
                return ResponseNotFound(Messages.ApplicantNotFoundOrDeletionFailed);
            }

            return ResponseOk(dataResponse: result, Messages.ApplicantDeletedSuccessfully);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddApplicant([FromForm] AddApplicantCommand command)
        {
            var applicantDto = await _mediator.Send(command);
            return ResponseCreated(applicantDto, Messages.ApplicantCreatedSuccessfully);
        }

        [HttpPut]
        [AllowAnonymous]
        public async Task<IActionResult> Update([FromForm] UpdateApplicantCommand command)
        {
            var applicantDto = await _mediator.Send(command);
            if (applicantDto == null)
            {
                return ResponseNotFound(Messages.ApplicantNotFoundOrDeleted);
            }
            return ResponseOk(applicantDto, Messages.ApplicantUpdatedSuccessfully);
        }
    }
}
