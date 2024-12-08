using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Labels.Command;
using Capstone.Application.Resources;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Capstone.Application.Module.Labels.CommandHandle
{
    public class CreateLabelCommandHandle : IRequestHandler<CreateLabelCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        public CreateLabelCommandHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Title))
                return new ResponseMediator(Messages.title_empty, null, 400);

            var project = _unitOfWork.Projects.Find(x => x.Id == request.ProjectId).Include(c => c.Labels).FirstOrDefault();
            if (project == null)
                return new ResponseMediator(Messages.project_not_found, null, 404);
            if (project.Labels.Select(x => x.Title.Trim().ToUpper()).Contains(request.Title.Trim().ToUpper()))
                return new ResponseMediator(Messages.title_label_available, null, 400);
            var label = new Capstone.Domain.Entities.Label() { Title = request.Title, ProjectId = request.ProjectId, Description = request.Description};
            _unitOfWork.Labels.Add(label);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", label);
        }
    }
}
