using Capstone.Domain.Entities;
using Capstone.Application.Module.Skills.Command;
using Capstone.Application.Module.Skills.Response;
using Capstone.Infrastructure.Repository;
using MediatR;

namespace Capstone.Application.Module.Skills.CommandHandle
{
    public class UpdateSkillCommandHandler : IRequestHandler<UpdateSkillCommand, SkillDto?>
    {
        private readonly IRepository<Skill> _skillRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;

        public UpdateSkillCommandHandler(IRepository<Skill> skillRepository, IUnitOfWork unitOfWork, IIdentityService identityService)
        {
            _skillRepository = skillRepository;
            _unitOfWork = unitOfWork;
            _identityService = identityService;
        }

        public async Task<SkillDto?> Handle(UpdateSkillCommand request, CancellationToken cancellationToken)
        {
            var skill = _skillRepository.GetQuery().FirstOrDefault(x=> x.Id ==request.Id);
            if (skill == null)
            {
                return null;
            }
            if (request.Isdeleted != null)
            skill.IsDeleted = (bool)request.Isdeleted;
            if(!string.IsNullOrEmpty(request.Title))
            skill.Title = request.Title;
            if(!string.IsNullOrEmpty(request.Description))
            skill.Description = request.Description;

            skill.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            skill.UpdatedBy = null;
            await _identityService.SetUpdatedByAsync(skill);
            await _skillRepository.UpdateAsync(skill);
            await _unitOfWork.SaveChangesAsync();
            return new SkillDto
            {
                Id = skill.Id,
                Title = skill.Title,
                Description = skill.Description,
                CreatedAt = skill.CreatedAt,
                UpdatedAt = skill.UpdatedAt,
                IsDeleted = skill.IsDeleted,
                CreatedBy = skill.CreatedBy,
                UpdatedBy = skill.UpdatedBy
            };
        }
    }
}
