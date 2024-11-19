using AutoMapper;
using Capstone.Application.Common.Paging;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Issues.DTO;
using Capstone.Application.Module.Issues.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace Capstone.Application.Module.Issues.QueryHandle
{
    public class GetListIssueQueryHandle : IRequestHandler<GetListIssuesQuery, PagingResultSP<Application.Module.Issues.DTO.IssueDTO>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetListIssueQueryHandle(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagingResultSP<DTO.IssueDTO>> Handle(GetListIssuesQuery request, CancellationToken cancellationToken)
        {
            if (!request.ProjectId.HasValue)
                return new PagingResultSP<Application.Module.Issues.DTO.IssueDTO>() { ErrorMessage = "Project not found" };

            var project = await _unitOfWork.Projects.Find(x => x.Id == request.ProjectId).Include(c => c.Statuses).ThenInclude(c => c.Issues).ThenInclude(c => c.Assignee).FirstOrDefaultAsync();
            if (project == null)
                return new PagingResultSP<Application.Module.Issues.DTO.IssueDTO>() { ErrorMessage = "Project not found" };

            var issueIds = project.Statuses.SelectMany(x => x.Issues).ToList().Select(c => c.Id);
            var issues = _unitOfWork.Issues.Find(x => issueIds.Contains(x.Id)).Include(c => c.Phase).Include(c => c.Label).Include(c => c.Status).Include(c => c.LastUpdateBy).Include(c => c.ParentIssue).Include(c => c.Reporter).Include(c => c.Assignee).Include(c => c.Comments).OrderByDescending(x => x.Index).ToList();
            if (request.Index.HasValue)
                issues = issues.Where(x => x.Index == request.Index.Value).ToList();

            if (request.Title != null)
                issues = issues.Where(x => x.Title.Trim().ToUpper().Contains(request.Title.Trim().ToUpper())).ToList();
            if (request.Priority != null)
            {
                if (!(request.Priority.HasValue && ((int)request.Priority < 1 || (int)request.Priority > 5)))
                    issues = issues.Where(x => x.Priority == request.Priority).ToList();
            }

            if (request.StartDate.HasValue)
            {
               issues = issues.Where(x => x.StartDate != null && x.StartDate.Value.Date >= request.StartDate.Value.Date).ToList();
            }
            if (request.DueDate.HasValue)
            {
                issues = issues.Where(x => x.DueDate != null && x.DueDate.Value.Date <= request.DueDate.Value.Date).ToList();
            }

            if (request.AssigneeId != null && request.AssigneeId.Count > 0)
            {

                issues = issues.Where(x => request.AssigneeId.Contains(x.Id)).ToList();
            }

            if (request.ReporterId.HasValue)
            {
                //if (_unitOfWork.Users.FindOne(x => x.Id == request.ReporterId.Value) == null)
                //    return new ResponseMediator("Assignee not found", null, 404);
                //else
                issues = issues.Where(x => x.ReporterId == request.ReporterId).ToList();
            }

            if (request.StatusId != null && request.StatusId.Count > 0)
            {
                issues = issues.Where(x => request.StatusId.Contains(x.StatusId)).ToList();
            }

            if (request.LabelId != null && request.LabelId.Count > 0)
            {
                issues = issues.Where(x => x.LabelId != null && request.LabelId.Contains(x.LabelId.Value)).ToList();
            }

            if (request.PhaseId != null && request.PhaseId.Count > 0)
            {
                issues = issues.Where(x => x.PhaseId != null && request.PhaseId.Contains(x.PhaseId.Value)).ToList();
            }

            if (request.PageIndex.HasValue && request.PageSize.HasValue)
            {
                if (request.PageIndex.Value < 1 || request.PageSize.Value < 0)
                    return new PagingResultSP<Application.Module.Issues.DTO.IssueDTO>() { ErrorMessage = "PageIndex, PageSize must >= 0" };

                int skip = (request.PageIndex.Value - 1) * request.PageSize.Value;
                var IssuePaging = issues.OrderByDescending(c => c.Index).Skip(skip).Take(request.PageSize.Value).ToList();
                var totalCount = issues.Count();
                var result = new PagingResultSP<Application.Module.Issues.DTO.IssueDTO>((_mapper.Map<List<Application.Module.Issues.DTO.IssueDTO>>(IssuePaging)).OrderByDescending(c => c.Index).ToList(), totalCount, request.PageIndex.Value, request.PageSize.Value);
                return result;
            }

            return new PagingResultSP<Application.Module.Issues.DTO.IssueDTO>() { Data = (_mapper.Map<List<Application.Module.Issues.DTO.IssueDTO>>(issues)).OrderByDescending(c => c.Index).ToList() };
        }



    }
}
