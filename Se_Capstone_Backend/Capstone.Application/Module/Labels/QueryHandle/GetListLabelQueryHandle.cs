using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Labels.Query;
using Capstone.Application.Resources;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Capstone.Application.Module.Labels.QueryHandle
{
    public class GetListLabelQueryHandle : IRequestHandler<GetListLabelQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        public GetListLabelQueryHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(GetListLabelQuery request, CancellationToken cancellationToken)
        {
            if (!request.projectId.HasValue)
                return new ResponseMediator(Messages.label_id_null, null);

            var labels = await _unitOfWork.Labels.GetQuery(x => x.ProjectId == request.projectId).Include(c => c.Issues).Select(x => new
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                IssueCount = x.Issues.Count,
            }).ToListAsync();
            return new ResponseMediator("",labels);
        }
    }
}
