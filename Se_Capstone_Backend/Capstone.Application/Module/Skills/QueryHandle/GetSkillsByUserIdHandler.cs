using Capstone.Application.Module.Skills.Query;
using Capstone.Application.Module.Skills.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Skills.QueryHandle
{
    public class GetSkillsByUserIdHandler : IRequestHandler<GetSkillsByUserIdQuery, List<SkillDto>>
    {
        private readonly IRepository<User> _userRepository;

        public GetSkillsByUserIdHandler(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<SkillDto>> Handle(GetSkillsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetQueryNoTracking()
                            .Include(x => x.Skills)
                            .FirstOrDefaultAsync(x => x.Id == request.UserId);

            if (user?.Skills != null)
            {
                user.Skills = user.Skills.Where(skill => !skill.IsDeleted).ToList();
            }


            if (user == null || user.Skills == null)
            {
                return new List<SkillDto>();
            }

            var skillDtos = user.Skills.Select(skill => new SkillDto
            {
                Id = skill.Id,
                Title = skill.Title,
                Description = skill.Description,
                Level = skill.Level,

            }).ToList();

            return skillDtos;
        }
    }

}
