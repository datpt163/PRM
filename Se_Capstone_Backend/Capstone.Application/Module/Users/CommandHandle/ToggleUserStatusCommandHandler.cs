using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.TokenService;
using Capstone.Application.Module.Auths.Model;
using Capstone.Application.Module.Users.Command;
using Capstone.Domain.Entities;
using Capstone.Domain.Enums;
using Capstone.Domain.Module.Auth.TokenBlackList;
using Capstone.Infrastructure.Redis;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Users.CommandHandle
{
    public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, bool>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenRevocationService _tokenRevocationService;
        private readonly RedisContext _redis;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IJwtService _jwtService;

        public ToggleUserStatusCommandHandler(IRepository<User> userRepository, IUnitOfWork unitOfWork, ITokenRevocationService tokenRevocationService, IJwtService jwtService, UserManager<User> userManager, RedisContext redis, ITokenBlacklistService tokenBlacklistService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _tokenRevocationService = tokenRevocationService;
            _tokenBlacklistService = tokenBlacklistService;
            _redis = redis;
            _jwtService = jwtService;
        }

        public async Task<bool> Handle(ToggleUserStatusCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetQuery().FirstOrDefaultAsync(x => x.Id == command.UserId);

            if (user == null)
            {
                return false;
            }

            if (user.Status == UserStatus.Inacitve)
            {
                user.Status = UserStatus.Active;
            }
            else
            {
                await _tokenRevocationService.RevocationTokenAsync(user.Id);
                user.Status = UserStatus.Inacitve;
                var listMonitorToken = _redis.GetData<List<MonitorTokenModel>>("ListMonitorToken");
                if (listMonitorToken != null)
                {
                    var tokensToRemove = new List<MonitorTokenModel>();

                    foreach (var monitorToken in listMonitorToken)
                    {
                        if (monitorToken.UserId == command.UserId)
                        {
                            await _tokenBlacklistService.BlacklistTokenAsync(monitorToken.Token, 401);
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
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }

}
