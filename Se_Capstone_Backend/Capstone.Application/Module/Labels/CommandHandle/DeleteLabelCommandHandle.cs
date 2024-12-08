using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Labels.Command;
using Capstone.Application.Resources;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Capstone.Application.Module.Labels.CommandHandle
{
    public class DeleteLabelCommandHandle : IRequestHandler<DeleteLabelCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        public DeleteLabelCommandHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseMediator> Handle(DeleteLabelCommand request, CancellationToken cancellationToken)
        {
            var label = _unitOfWork.Labels.Find(x => x.Id == request.Id).Include(c => c.Issues).Include(c => c.Project).FirstOrDefault();
            if (label == null)
                return new ResponseMediator(Messages.label_not_found, null, 404);

            _unitOfWork.Labels.Remove(label);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}
