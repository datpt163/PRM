using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Projects.Command;
using Capstone.Infrastructure.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.CommandHandle
{
    public class UpdateMemberCommandHandle : IRequestHandler<UpdateMemberCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        public UpdateMemberCommandHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
        {
            var userProject = _unitOfWork.UserProjects.Find(x => x.ProjectId == request.ProjectId && x.UserId == request.UserId).FirstOrDefault();
            if (userProject == null)
                return new ResponseMediator("User project not found", null, 404);

            if(request.PositionId.HasValue)
            {
                var position = _unitOfWork.Positions.Find(x => x.Id == request.PositionId).FirstOrDefault();
                if (userProject == null)
                    return new ResponseMediator("Position not found", null, 404);
            }

            userProject.PositionId = request.PositionId;
            userProject.IsProjectConfigurator = request.IsProjectConfigurator;
            userProject.IsIssueConfigurator = request.IsIssueConfigurator;
            userProject.IsCommentConfigurator = request.IsCommentConfigurator;

            _unitOfWork.UserProjects.Update(userProject);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", userProject);
        }
    }
}
