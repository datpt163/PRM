using Capstone.Application.Module.Report.Response;
using Capstone.Application.Module.Report.Query;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Report.QueryHandle
{
    public class GetSkillsReportQueryHandler : IRequestHandler<GetSkillsReportQuery, List<SkillReport>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSkillsReportQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SkillReport>> Handle(GetSkillsReportQuery query, CancellationToken cancellationToken)
        {
            var skillQuery = _unitOfWork.Skills.GetQueryNoTracking().Where(x=> !x.IsDeleted)
                .Include(s => s.Users)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.Title))
            {
                skillQuery = skillQuery.Where(s => s.Title.Contains(query.Title));
            }

            if (query.MinimumUsers.HasValue)
            {
                skillQuery = skillQuery.Where(s => s.Users != null && s.Users.Count >= query.MinimumUsers.Value);
            }

            if (query.MaximumUsers.HasValue)
            {
                skillQuery = skillQuery.Where(s => s.Users != null && s.Users.Count <= query.MaximumUsers.Value);
            }

            if (query.UserId.HasValue)
            {
                skillQuery = skillQuery.Where(s => s.Users != null && s.Users.Any(u => u.Id == query.UserId.Value));
            }

            var skills = await skillQuery.Select(s => new SkillReport
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                UserCount = s.Users != null ? s.Users.Count : 0,
                Users = s.Users != null
                    ? s.Users
                        .Where(u => u != null && !string.IsNullOrEmpty(u.UserName))
                        .Select(u => new UserDto
                        {
                            Id = u.Id,
                            UserName = u.UserName ?? string.Empty,
                            FullName = u.FullName ?? string.Empty 
                        }).ToList()
                    : new List<UserDto>()
            }).ToListAsync(cancellationToken);

            return skills;
        }
    }
}
