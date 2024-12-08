using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Phase.Command;
using Capstone.Application.Resources;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Phase.CommandHandle
{
    public class CompletePhaseCommandHandle : IRequestHandler<CompletePhaseCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompletePhaseCommandHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(CompletePhaseCommand request, CancellationToken cancellationToken)
        {
            var project = _unitOfWork.Projects.Find(x => x.Id == request.ProjectId).Include(c => c.Phases).FirstOrDefault();
            if(project == null)
                return new ResponseMediator(Messages.project_not_found, null, 404);

            (PhaseStatus status, Domain.Entities.Phase? phaseRunning, Domain.Entities.Phase? phaseAfterPhaseRunning) = project.GetStatusPhaseOfProject();
            if(status == PhaseStatus.Complete)
                return new ResponseMediator(Messages.no_more_phase, null, 404);
            else if(status == PhaseStatus.NoPhase)
                return new ResponseMediator(Messages.project_no_phase, null, 400);
            else if( status == PhaseStatus.NoPhaseRunning)
            {
                var phase = project.Phases.FirstOrDefault();
                if(phase != null)
                    phase.ActualStartDate = DateTime.Now;
            }
            else if (status == PhaseStatus.Running && phaseRunning != null && phaseAfterPhaseRunning != null)
            {
                phaseRunning.ActualEndDate = DateTime.Now;
                phaseAfterPhaseRunning.ActualStartDate = DateTime.Now;
            }
            
            _unitOfWork.Projects.Update(project);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}
