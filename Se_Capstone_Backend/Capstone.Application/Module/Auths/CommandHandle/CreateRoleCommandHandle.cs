using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auths.Command;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;



namespace Capstone.Application.Module.Auths.CommandHandle
{
    public class CreateRoleCommandHandle : IRequestHandler<CreateRoleCommand, ResponseMediator>
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public CreateRoleCommandHandle(RoleManager<Role> roleManager, IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }
        public async Task<ResponseMediator> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                return new ResponseMediator(Messages.role_name_empty, null);
            }

            if (string.IsNullOrEmpty(request.Description))
            {
                return new ResponseMediator(Messages.description_empty, null);
            }


            if (string.IsNullOrEmpty(request.Color))
            {
                return new ResponseMediator(Messages.color_empty, null);
            }

            if (request.PermissionsId == null || request.PermissionsId.Count == 0)
                return new ResponseMediator(Messages.list_permission_empty, null, 400);

            var roleExists = await _roleManager.RoleExistsAsync(request.Name);
            if (roleExists)
            {
                return new ResponseMediator(Messages.role_already_exists, null);
            }

            var result = await _roleManager.CreateAsync(new Role() { Name = request.Name.ToUpper()});

            if (result.Succeeded)
            {
                var createdRole = await _unitOfWork.Roles.Find(c => c.Name == request.Name.ToUpper()).Include(c => c.Permissions).FirstOrDefaultAsync();
                if (createdRole != null) {
                    createdRole.Description = request.Description;
                    createdRole.Color = request.Color;
                    foreach (var p in request.PermissionsId) {
                        var permission = await _unitOfWork.Permissions.FindOneAsync(c => c.Id == p);
                        if (permission != null) 
                            createdRole.Permissions.Add(permission);
                    }
                    _unitOfWork.Roles.Update(createdRole);
                    await _unitOfWork.SaveChangesAsync();
                    return new ResponseMediator("", new { color = createdRole.Color, id = createdRole.Id, name = createdRole.Name, description = createdRole.Description, permissions = createdRole.Permissions }); ;
                }
                return new ResponseMediator(Messages.failed_to_create_role, null);
            }
            else
            {
                return new ResponseMediator(Messages.failed_to_create_role, null);
            }
        }
    }
}
