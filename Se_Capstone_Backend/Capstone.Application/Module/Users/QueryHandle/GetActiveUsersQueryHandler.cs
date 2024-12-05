using Capstone.Application.Module.Users.Query;
using Capstone.Application.Module.Users.Response;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Users.QueryHandle
{
    public class GetActiveUsersQueryHandler : IRequestHandler<GetActiveUsersQuery, List<UserStatisticsResponse>>
    {
        private readonly IRepository<User> _userRepository;

        public GetActiveUsersQueryHandler(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserStatisticsResponse>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetQueryNoTracking()
                .Include(u => u.Skills.Where(s => !s.IsDeleted))
                .Include(u => u.UserProjects.Where(x=> x.Project.Status != ProjectStatus.Finished))
                .ThenInclude(c => c.Project)
                .Include(u => u.UserProjects.Where(x => x.Project.Status != ProjectStatus.Finished))
                .ThenInclude(up => up.Position)
                .Where(u => u.Status == UserStatus.Active && request.UserInProject ==null || !request.UserInProject.Contains(u.Id))
                .ToListAsync(cancellationToken);

            var userResponses = users.Select(user => new UserStatisticsResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Skills = string.Join(", ", user.Skills?.Select(s => s.Title) ?? Enumerable.Empty<string>()) +","+ string.Join(", ", user.UserProjects.Where(x=> x.Position!=null).Select(x=> x.Position?.Title)),
                ActiveProjectCount = user.UserProjects.Select(x => x.Project).Count()
            }).ToList();

            return userResponses;
        }
    }
}
