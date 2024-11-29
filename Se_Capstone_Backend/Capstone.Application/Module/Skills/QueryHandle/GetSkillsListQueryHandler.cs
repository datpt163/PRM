using Capstone.Domain.Entities;
using Capstone.Application.Module.Skills.Query;
using Capstone.Application.Module.Skills.Response;
using Capstone.Infrastructure.Repository;
using MediatR;
using Capstone.Application.Common.Paging;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Skills.QueryHandle
{
    public class GetSkillsListQueryHandler : IRequestHandler<GetSkillsListQuery, PagingResultSP<SkillDto>>
    {
        private readonly IRepository<Skill> _skillRepository;

        public GetSkillsListQueryHandler(IRepository<Skill> skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<PagingResultSP<SkillDto>> Handle(GetSkillsListQuery request, CancellationToken cancellationToken)
        {
            var query = _skillRepository.GetQueryNoTracking();

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

            int totalCount = await query.CountAsync(cancellationToken);

            if (request.PageIndex <= 0) request.PageIndex = 1;
            if (request.PageSize <= 0) request.PageSize = 10;
            var pagedSkillsQuery = query.OrderByAndPaginate(request);

            var pagedSkills = await pagedSkillsQuery
                .Select(skill => new SkillDto
                {
                    Id = skill.Id,
                    Title = skill.Title,
                    Description = skill.Description,
                    CreatedAt = skill.CreatedAt,
                    UpdatedAt = skill.UpdatedAt,
                    IsDeleted = skill.IsDeleted,
                    CreatedBy = skill.CreatedBy,
                    UpdatedBy = skill.UpdatedBy
                }).ToListAsync(cancellationToken);

            return new PagingResultSP<SkillDto>(pagedSkills, totalCount, request.PageIndex, request.PageSize);
        }
    }
}
