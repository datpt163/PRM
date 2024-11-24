using Capstone.Application.Module.Report.Response;
using Capstone.Application.Module.Report.Query;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Capstone.Domain.Entities;
using Capstone.Application.Module.Skills.Response;

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
            var skillQuery = _unitOfWork.Skills.GetQueryNoTracking()
                            .Include(s => s.Users)
                            .Where(s => !s.IsDeleted);

            if (!string.IsNullOrEmpty(query.Title))
            {
                skillQuery = skillQuery.Where(s => EF.Functions.Like(s.Title.ToLower(), $"%{query.Title.ToLower()}%"));
            }

            var skills = await skillQuery.ToListAsync(cancellationToken);



            if (query.MinimumUsers.HasValue)
            {
                skills = skills.Where(s => s.Users == null ||  s.Users.Count >= query.MinimumUsers.Value).ToList();
            }

            if (query.MaximumUsers.HasValue)
            {
                skills = skills.Where(s => s.Users == null || s.Users.Count <= query.MaximumUsers.Value).ToList();
            }

            if (query.UserId.HasValue)
            {
                skills = skills.Where(s => s.Users !=null && s.Users.Any(u => u.Id == query.UserId.Value)).ToList();
            }

            var skillDto = skills
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.Description,
                    Users = s.Users != null ? s.Users.Select(u => new UserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? string.Empty,
                        FullName = u.FullName ?? string.Empty
                    }).ToList() : new List<UserDto>()
                })
                .ToList();

            var result = skillDto
                .Select(s => new SkillReport
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    UserCount = s.Users.Count,
                    Users = s.Users
                })
                .OrderBy(x=> x.UserCount)
                .ToList();

            return result;
        }
    }
}
