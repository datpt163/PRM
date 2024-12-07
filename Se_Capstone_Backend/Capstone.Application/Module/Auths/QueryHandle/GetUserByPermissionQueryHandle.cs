using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auths.Query;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Capstone.Application.Module.Auths.QueryHandle
{
    public class GetUserByPermissionQueryHandle : IRequestHandler<GetUserByPermissionQuery, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public GetUserByPermissionQueryHandle(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<ResponseMediator> Handle(GetUserByPermissionQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.permissionName))
                return new ResponseMediator(Messages.permission_name_empty, null, 400);

            var permission = await _unitOfWork.Permissions.Find(x => x.Name.Trim().ToUpper().Equals(request.permissionName.Trim().ToUpper())).Include(c => c.Roles).FirstOrDefaultAsync();

            if (permission == null)
                return new ResponseMediator(Messages.permission_not_found, null, 404);

            var users = new List<User>();

            foreach (var role in permission.Roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                users.AddRange(usersInRole);
            }

            users = users.Distinct().ToList();

            return new ResponseMediator("", users.Select(x => new
            {
                Id = x.Id,
                UserName = x.UserName,
                FullName = x.FullName,
                Dob = x.Dob,
                Email = x.Email,
            })
            .ToList());
        }
    }
}
