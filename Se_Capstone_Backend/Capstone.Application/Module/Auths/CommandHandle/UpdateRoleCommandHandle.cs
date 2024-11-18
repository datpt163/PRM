using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auths.Command;
using Capstone.Application.Module.Auths.Model;
using Capstone.Domain.Entities;
using Capstone.Domain.Module.Auth.TokenBlackList;
using Capstone.Infrastructure.Redis;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Auths.CommandHandle
{
    public class UpdateRoleCommandHandle : IRequestHandler<UpdateRoleCommand, ResponseMediator>
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RedisContext _redis;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        public UpdateRoleCommandHandle(RoleManager<Role> roleManager, IUnitOfWork unitOfWork, RedisContext redis, ITokenBlacklistService tokenBlacklistService)
        {
            _tokenBlacklistService = tokenBlacklistService;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _redis = redis;
        }

        public async Task<ResponseMediator> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                return new ResponseMediator("Role name empty", null,400);
            }

            if (string.IsNullOrEmpty(request.Description))
            {
                return new ResponseMediator("Description empty", null,400);
            }
            if (string.IsNullOrEmpty(request.Color))
            {
                return new ResponseMediator("Color empty", null);
            }
            if (request.PermissionsId == null || request.PermissionsId.Count == 0)
                return new ResponseMediator("List Permission empty", null, 400);

            var roleUpdate = await _unitOfWork.Roles.Find(x => x.Id == request.Id).Include(c => c.Permissions).FirstOrDefaultAsync();

            if (roleUpdate == null)
                return new ResponseMediator("Role not found", null,404);
            var rolePermissionIds = roleUpdate.Permissions.Select(p => p.Id).ToList();
            if ( _unitOfWork.Roles.FindOne( x => x.Name != null && x.Name == request.Name.ToUpper() && x.Id != request.Id ) != null)
            {
                return new ResponseMediator("This role name already exists", null);
            }

            var role = await _roleManager.FindByNameAsync(roleUpdate.Name ?? "");


            if(role == null)
            {
                return new ResponseMediator("Failed to create role", null);
            }
            else
            {
                role.Name = request.Name.ToUpper();
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    var createdRole = await _unitOfWork.Roles.Find(c => c.Name == request.Name.ToUpper()).Include(c => c.Permissions).FirstOrDefaultAsync();
                    if (createdRole != null)
                    {
                        createdRole.Description = request.Description;
                        createdRole.Color = request.Color;
                        createdRole.Permissions = new List<Permission>();
                        foreach (var p in request.PermissionsId)
                        {
                            var permission = await _unitOfWork.Permissions.FindOneAsync(c => c.Id == p);
                            if (permission != null)
                                createdRole.Permissions.Add(permission);
                        }
                        _unitOfWork.Roles.Update(createdRole);
                        await _unitOfWork.SaveChangesAsync();

                       
                        bool arePermissionsIdentical = !rolePermissionIds.Except(request.PermissionsId).Any()
                                           && !request.PermissionsId.Except(rolePermissionIds).Any();

                        if (!arePermissionsIdentical)
                        {
                            var listMonitorToken = _redis.GetData<List<MonitorTokenModel>>("ListMonitorToken");
                            if (listMonitorToken != null)
                            {
                                var tokensToRemove = new List<MonitorTokenModel>();

                                foreach (var monitorToken in listMonitorToken)
                                {
                                    if (monitorToken.RoleId == request.Id)
                                    {
                                        await _tokenBlacklistService.BlacklistTokenAsync(monitorToken.Token, 888);
                                        tokensToRemove.Add(monitorToken);
                                    }
                                }

                                foreach (var tokenToRemove in tokensToRemove)
                                {
                                    listMonitorToken.Remove(tokenToRemove);
                                }

                                _redis.SetData<List<MonitorTokenModel>>("ListMonitorToken", listMonitorToken, DateTime.Now.AddYears(10));
                            }
                        }


                        return new ResponseMediator("", new { id = createdRole.Id, name = createdRole.Name, description = createdRole.Description, permissions = createdRole.Permissions , color = createdRole.Color });
                    }
                    return new ResponseMediator("Failed to update role", null);
                }
                else
                {
                    return new ResponseMediator("Failed to update role", null);
                }
            }
        }
    }
}
