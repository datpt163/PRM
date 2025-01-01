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
            var phase = _unitOfWork.Phases.Find(x => x.Id == request.ProjectId).FirstOrDefault();
            if(phase == null)
                return new ResponseMediator("Phase not found", null, 404);

            if(phase.ActualStartDate == null)
                phase.ActualStartDate = DateTime.Now;
            else
                phase.ActualEndDate = DateTime.Now;
            
            _unitOfWork.Phases.Update(phase);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}
