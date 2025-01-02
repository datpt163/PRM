using Capstone.Domain.Entities;
using Capstone.Application.Module.Skills.Query;
using Capstone.Application.Module.Skills.Response;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Skills.QueryHandle
{
    public class GetSkillQueryHandler : IRequestHandler<GetSkillQuery, SkillDto?>
    {
        private readonly IRepository<Skill> _skillRepository;

        public GetSkillQueryHandler(IRepository<Skill> skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<SkillDto?> Handle(GetSkillQuery request, CancellationToken cancellationToken)
        {
            var skill =  await _skillRepository.GetQuery().FirstOrDefaultAsync(x => x.Id == request.Id);
            if (skill == null)
            {
                return null;
            }

            return new SkillDto
            {
                Id = skill.Id,
                Title = skill.Title,
                Description = skill.Description,
                CreatedAt = skill.CreatedAt,
                UpdatedAt = skill.UpdatedAt,
                IsDeleted = skill.IsDeleted,
                CreatedBy = skill.CreatedBy,
                UpdatedBy = skill.UpdatedBy,
                Level = skill.Level,

            };
        }
    }
}
