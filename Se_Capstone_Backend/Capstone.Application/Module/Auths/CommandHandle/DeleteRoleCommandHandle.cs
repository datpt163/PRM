using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auths.Command;
using Capstone.Application.Module.Auths.Model;
using Capstone.Domain.Entities;
using Capstone.Domain.Module.Auth.TokenBlackList;
using Capstone.Infrastructure.Redis;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Auths.CommandHandle
{
    public class DeleteRoleCommandHandle : IRequestHandler<DeleteRoleCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<Role> _roleManager;
        private readonly RedisContext _redis;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        public DeleteRoleCommandHandle(IUnitOfWork unitOfWork, RoleManager<Role> roleManager, RedisContext redis, ITokenBlacklistService tokenBlacklistService)
        {
            _tokenBlacklistService = tokenBlacklistService;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _redis = redis; 
        }

        public async Task<ResponseMediator> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var role = _unitOfWork.Roles.FindOne(x => x.Id == request.Id);
            if (role == null)
                return new ResponseMediator("Role not found", null, 404);

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return new ResponseMediator("Delete fail", null, 404);
            }

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

            return new ResponseMediator("", null);

        }
    }
}
