using AutoMapper;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Issues.DTO;
using Capstone.Application.Module.Issues.Query;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Issues.QueryHandle
{
    public class GetDetailIssueQueryHandle : IRequestHandler<GetDetailIssueQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetDetailIssueQueryHandle(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(GetDetailIssueQuery request, CancellationToken cancellationToken)
        {
            var issue = await _unitOfWork.Issues.Find(x => x.Id == request.Id).Include(c => c.Phase).Include(c => c.ParentIssue).Include(c => c.Label).Include(c => c.Status).Include(c => c.LastUpdateBy).Include(c => c.Reporter).Include(c => c.Assignee).Include(c => c.SubIssues).ThenInclude(c => c.Assignee).Include(c => c.SubIssues).ThenInclude(c => c.Status).Include(c => c.Comments).ThenInclude(c => c.User).FirstOrDefaultAsync();
            if(issue == null)
                return new ResponseMediator("Issue  not found", null, 404);
            var response = _mapper.Map<IssueDTO2?>(issue);
            return new ResponseMediator("", response);

        }
    }
}
