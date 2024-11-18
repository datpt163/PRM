using AutoMapper;
using Capstone.Application.Common.Paging;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Issues.DTO;
using Capstone.Application.Module.Status.Query;
using Capstone.Application.Module.Status.Responses;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Status.QueryHandle
{
    public class GetListStatusQueryHandle : IRequestHandler<GetListStatusQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetListStatusQueryHandle(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(GetListStatusQuery request, CancellationToken cancellationToken)
        {
            if (!request.projectId.HasValue)
                return new ResponseMediator("Project id null", null);

            var statuses = await _unitOfWork.Statuses.GetQuery(x => x.ProjectId == request.projectId).
                Include(x => x.Issues)
                .OrderBy(x => x.Position).Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Position = x.Position,
                    Color = x.Color,
                    IsDone = x.IsDone,
                    IssueCount = x.Issues.Count,
                }).ToListAsync();
            return new ResponseMediator("", statuses);
        }
    }

    public class GetListStatusKanbanHandle : IRequestHandler<GetListStatusKanbanQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetListStatusKanbanHandle(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(GetListStatusKanbanQuery request, CancellationToken cancellationToken)
        {
            if (!request.projectId.HasValue)
                return new ResponseMediator("Project id null", null);

            var statuses = await _unitOfWork.Statuses.GetQuery(x => x.ProjectId == request.projectId).
                Include(x => x.Issues.Where(c => c.ParentIssueId == null)).ThenInclude(c => c.Phase).
                 Include(x => x.Issues).ThenInclude(c => c.Label).
                 Include(x => x.Issues).ThenInclude(c => c.LastUpdateBy).
                 Include(x => x.Issues).ThenInclude(c => c.Reporter).
                 Include(x => x.Issues).ThenInclude(c => c.Assignee)
                .OrderBy(x => x.Position).Select(x => new KanbanResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Position = x.Position,
                    Color = x.Color,
                    IsDone = x.IsDone,
                    Issues = _mapper.Map<List<IssueDTO>>(x.Issues.OrderBy(x => x.Position)),
                    IssueCount = x.Issues.Count,
                }).ToListAsync();

            if (request.Index.HasValue)
                foreach (var status in statuses)
                {
                    status.Issues = status.Issues.Where(x => x.Index == request.Index.Value).ToList();
                }

            if (request.Title != null)
                foreach (var status in statuses)
                {
                    status.Issues = status.Issues.Where(x => x.Title.Trim().ToUpper().Contains(request.Title.Trim().ToUpper())).ToList();
                }
           
            if (request.Priority != null)
            {
                if (!(request.Priority.HasValue && ((int)request.Priority < 1 || (int)request.Priority > 5)))
                    foreach (var status in statuses)
                    {
                        status.Issues = status.Issues.Where(x => x.Priority == request.Priority).ToList();

                    }
            }

            if (request.StartDate.HasValue)
                foreach (var status in statuses)
                {
                    status.Issues = status.Issues.Where(x => x.StartDate != null && x.StartDate.Value.Date >= request.StartDate.Value.Date).ToList();
                }
         
            if (request.DueDate.HasValue)
                foreach (var status in statuses)
                {
                    status.Issues = status.Issues.Where(x => x.DueDate != null && x.DueDate.Value.Date <= request.DueDate.Value.Date).ToList();
                }
         
            if (request.AssigneeId != null && request.AssigneeId.Count > 0)
                foreach (var status in statuses)
                {
                    status.Issues = status.Issues.Where(x => request.AssigneeId.Contains(x.Id)).ToList();
                }
           

            if (request.ReporterId.HasValue)
                foreach (var status in statuses)
                {
                    status.Issues = status.Issues.Where(x => x.ReporterId == request.ReporterId).ToList();
                }
         

            if (request.StatusId != null && request.StatusId.Count > 0)
                foreach (var status in statuses)
                {
                    status.Issues = status.Issues.Where(x => request.StatusId.Contains(x.StatusId)).ToList();
                }

            if (request.LabelId != null && request.LabelId.Count > 0)
                foreach (var status in statuses)
                {
                    status.Issues = status.Issues.Where(x => x.LabelId != null && request.LabelId.Contains(x.LabelId.Value)).ToList();
                }
           

            if (request.PhaseId != null && request.PhaseId.Count > 0)
                foreach (var status in statuses)
                {
                    status.Issues = status.Issues.Where(x => x.PhaseId != null && request.PhaseId.Contains(x.PhaseId.Value)).ToList();
                }
           

            return new ResponseMediator("", statuses);
        }
    }
}
