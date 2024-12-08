using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Phase.Query;
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

namespace Capstone.Application.Module.Phase.QueryHandle
{
    public class GetListPhaseQueryHandle : IRequestHandler<GetListPhaseQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetListPhaseQueryHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(GetListPhaseQuery request, CancellationToken cancellationToken)
        {
            var project = await _unitOfWork.Projects.GetQuery(x => x.Id == request.ProjectId).Include(c => c.Phases).FirstOrDefaultAsync();
            if(project == null)
                return new ResponseMediator(Messages.project_not_found, null, 404);

            (PhaseStatus status, Domain.Entities.Phase? phaseRunning, Domain.Entities.Phase? phaseAfterPhaseRunning) = project.GetStatusPhaseOfProject();

            return new ResponseMediator("", new
            {
                Phases = project.Phases,
                Status = status,
            });
        }
    }
}
