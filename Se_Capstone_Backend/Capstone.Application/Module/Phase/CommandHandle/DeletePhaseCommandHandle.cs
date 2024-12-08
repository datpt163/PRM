using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Phase.Command;
using Capstone.Application.Resources;
using Capstone.Infrastructure.Repository;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Phase.CommandHandle
{
    public class DeletePhaseCommandHandle : IRequestHandler<DeletePhaseCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletePhaseCommandHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(DeletePhaseCommand request, CancellationToken cancellationToken)
        {
            var phase = await _unitOfWork.Phases.Find(x => x.Id == request.Id).FirstOrDefaultAsync();
            if(phase == null)
                return new ResponseMediator(Messages.phase_not_found, null, 404);
            if(phase.ActualStartDate.HasValue)
                return new ResponseMediator(Messages.phase_running_or_completed, null, 404);

            _unitOfWork.Phases.Remove(phase);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}
