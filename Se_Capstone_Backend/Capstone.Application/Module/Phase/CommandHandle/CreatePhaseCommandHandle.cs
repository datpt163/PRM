using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Phase.Command;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Phase.CommandHandle
{
    public class CreatePhaseCommandHandle : IRequestHandler<CreatePhaseCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreatePhaseCommandHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(CreatePhaseCommand request, CancellationToken cancellationToken)
        {
            var project = _unitOfWork.Projects.Find(x => x.Id == request.ProjectId).Include(x => x.Phases).FirstOrDefault();

            if (project == null)
                return new ResponseMediator(Messages.project_not_found, null, 404);

            if (string.IsNullOrEmpty(request.Title))
                return new ResponseMediator(Messages.title_empty, null, 404);

            if (project.Phases.Select(x => x.Title.Trim().ToUpper()).Contains(request.Title.Trim().ToUpper()))
                return new ResponseMediator(Messages.title_phase_available, null, 400);

            //if (request.ExpectedStartDate.Date < DateTime.Now.Date || request.ExpectedEndDate.Date < DateTime.Now.Date)
            //    return new ResponseMediator("Start date and end date must be greater than the current time", null);

            if (request.ExpectedEndDate.Date < request.ExpectedStartDate.Date)
                return new ResponseMediator(Messages.end_date_greater_than_start_date, null);


            var phase = new Domain.Entities.Phase() { ProjectId = request.ProjectId, Title = request.Title, Description = request.Description, ExpectedStartDate = request.ExpectedStartDate, ExpectedEndDate = request.ExpectedEndDate };
            var errorMessage = phase.IsValidExpectDate(project.Phases);
            if (!string.IsNullOrEmpty(errorMessage))
                return new ResponseMediator(errorMessage, null, 400);
            (PhaseStatus status, Domain.Entities.Phase? phaseRunning, Domain.Entities.Phase? phaseAfterPhaseRunning) = project.GetStatusPhaseOfProject();
            if((status == PhaseStatus.Running || status == PhaseStatus.Complete) && phaseRunning != null && request.ExpectedStartDate < phaseRunning.ExpectedEndDate)
                return new ResponseMediator(Messages.start_date_greater_than_phases, null);


            _unitOfWork.Phases.Add(phase);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", phase);
        }
    }
}
