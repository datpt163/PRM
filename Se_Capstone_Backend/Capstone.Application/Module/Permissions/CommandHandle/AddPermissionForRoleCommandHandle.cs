using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Permissions.Command;
using Capstone.Application.Resources;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Capstone.Application.Module.Permissions.CommandHandle
{
    public class AddPermissionForRoleCommandHandle : IRequestHandler<AddPermissionForRoleCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddPermissionForRoleCommandHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(AddPermissionForRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _unitOfWork.Roles.Find(r => r.Id == request.RoleId).Include(p => p.Permissions).FirstOrDefaultAsync();
            if (role == null)
                return new ResponseMediator(Messages.role_not_found, null);
            var permissionsOfRole = role.Permissions.Select(p => p.Id);

            var listPermission = request.Permissions.Where(p => !permissionsOfRole.Contains(p.PermissionId)).Select(p => p.PermissionId).ToList();

            var permissions = _unitOfWork.Permissions.Find(p => listPermission.Contains(p.Id)).ToList();

            foreach(var p in permissions)
                role.Permissions.Add(p);

            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}
