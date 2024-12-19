using Capstone.Application.Module.Skills.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capstone.Application.Module.Skills.Response;
using Capstone.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Skills.QueryHandle
{
    public class GetUsersBySkillTitleQueryHandler : IRequestHandler<GetUsersBySkillTitleQuery, List<UsersSkillDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUsersBySkillTitleQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UsersSkillDto>> Handle(GetUsersBySkillTitleQuery request, CancellationToken cancellationToken)
        {
            var searchKeyword = request.SkillTitle.ToLower();

           var skillQuery = _unitOfWork.Skills
                .GetQueryNoTracking()
                .Include(skill => skill.Users)
                .Where(skill => skill.Title.ToLower().Contains(searchKeyword));

            var users = await skillQuery
                .SelectMany(skill => skill.Users)
                .Distinct() 
                .Select(user => new UsersSkillDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName
                })
                .ToListAsync(cancellationToken);

            return users;
        }
    }

}
