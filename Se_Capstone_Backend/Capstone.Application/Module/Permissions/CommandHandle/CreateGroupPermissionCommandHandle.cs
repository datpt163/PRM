using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Permissions.Command;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Capstone.Application.Module.Permissions.CommandHandle
{
    public class CreateGroupPermissionCommandHandle : IRequestHandler<CreateGroupPermissionCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateGroupPermissionCommandHandle(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(CreateGroupPermissionCommand request, CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(request.Name))
                return new ResponseMediator(Messages.name_empty, null);

            var groupPermission = await _unitOfWork.GroupPermissions.Find(p => p.Name.ToUpper().Equals(request.Name.ToUpper())).FirstOrDefaultAsync();
            if (groupPermission != null) 
                return new ResponseMediator(Messages.group_permission_exists, null);

            var groupPermissionCreated = new GroupPermission() { Name = request.Name.ToUpper() };
                _unitOfWork.GroupPermissions.Add(groupPermissionCreated);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", groupPermissionCreated);
        }
    }
}
