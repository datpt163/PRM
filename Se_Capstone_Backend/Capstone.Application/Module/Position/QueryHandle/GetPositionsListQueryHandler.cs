using Capstone.Domain.Entities;
using Capstone.Application.Module.Positions.Query;
using Capstone.Application.Module.Positions.Response;
using Capstone.Infrastructure.Repository;
using MediatR;
using Capstone.Application.Common.Paging;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Positions.QueryHandle
{
    public class GetPositionsListQueryHandler : IRequestHandler<GetPositionsListQuery, PagingResultSP<PositionDto>>
    {
        private readonly IRepository<Position> _positionRepository;

        public GetPositionsListQueryHandler(IRepository<Position> positionRepository)
        {
            _positionRepository = positionRepository;
        }

        public async Task<PagingResultSP<PositionDto>> Handle(GetPositionsListQuery request, CancellationToken cancellationToken)
        {
            var query = _positionRepository.GetQueryNoTracking();

            if (request.IsDeleted != null)
            {
                query = query.Where(x => x.IsDeleted == request.IsDeleted);
            }

            if (!string.IsNullOrEmpty(request.Title))
            {
                query = query.Where(x => x.Title.ToLower().Contains(request.Title.ToLower()));
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                query = query.Where(x => x.Description.ToLower().Contains(request.Description.ToLower()));
            }

            query = query.OrderBy(x => x.CreatedAt);

            int totalCount = await query.CountAsync(cancellationToken);

            if (request.PageIndex <= 0) request.PageIndex = 1;
            if (request.PageSize <= 0) request.PageSize = 10;
            var pagedPositionsQuery = query.OrderByAndPaginate(request);

            var pagedPositions = await pagedPositionsQuery
                .Select(position => new PositionDto
                {
                    Id = position.Id,
                    Title = position.Title,
                    Description = position.Description ?? string.Empty,
                    CreatedAt = position.CreatedAt,
                    UpdatedAt = position.UpdatedAt,
                    IsDeleted = position.IsDeleted,
                    CreatedBy = position.CreatedBy,
                    UpdatedBy = position.UpdatedBy
                }).ToListAsync(cancellationToken);

            return new PagingResultSP<PositionDto>(pagedPositions, totalCount, request.PageIndex, request.PageSize);
        }
    }
}
