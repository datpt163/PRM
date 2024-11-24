using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auths.Command;
using Capstone.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Capstone.Application.Module.Auths.CommandHandle
{
    public class AddRoleForUserCommandHandle : IRequestHandler<AddRoleForUserCommand, ResponseMediator>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

       public AddRoleForUserCommandHandle(RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<ResponseMediator> Handle(AddRoleForUserCommand request, CancellationToken cancellationToken)
        {

            var user = await _userManager.FindByIdAsync(request.UserId + "");
            if (user == null)
            {
                return new ResponseMediator("User not found", null, 404);
            }

            var role = await _roleManager.FindByIdAsync(request.RoleId + "");

            if (role == null)
            {
                return new ResponseMediator("Role not found", null, 404);
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Count >= 1)
                return new ResponseMediator("This user already has a role", null, 400);

            var result = await _userManager.AddToRoleAsync(user, role.Name ?? "");

            if (result.Succeeded)
            {
                return new ResponseMediator("", null);
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                var errorMessage = string.Join("; ", errors);
                return new ResponseMediator(errorMessage, errors);
            }
        }
    }
}
    