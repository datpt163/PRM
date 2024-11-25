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
        [Authorize(Roles = "GET_APPLICANT")]
        public async Task<IActionResult> GetApplicants([FromQuery] GetApplicantListQuery query)
        {
            var response = await _mediator.Send(query);
            return ResponseOk(response.Data, response.Paging, Messages.ApplicantsRetrievedSuccessfully);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "GET_APPLICANT")]
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
        [Authorize(Roles = "DELETE_APPLICANT")]
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
        [Authorize(Roles = "ADD_APPLICANT")]
        public async Task<IActionResult> AddApplicant([FromForm] AddApplicantCommand command)
        {
            var applicantDto = await _mediator.Send(command);
            return ResponseCreated(applicantDto, Messages.ApplicantCreatedSuccessfully);
        }

        [HttpPut]
        [Authorize(Roles = "UPDATE_APPLICANT")]
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
