using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auth.Query;
using Capstone.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Capstone.Application.Module.Auth.Response;
using Capstone.Infrastructure.Repository;
using Capstone.Application.Module.Auths.Response;
using Capstone.Infrastructure.Redis;
using Capstone.Application.Module.Auths.Model;
using Microsoft.EntityFrameworkCore;
using Capstone.Infrastructure.Helpers;
using Capstone.Application.Resources;
using Capstone.Domain.Module.Auth.TokenBlackList;

namespace Capstone.Application.Module.Auth.QueryHandle
{
    public class LoginQueryHandle : IRequestHandler<LoginQuery, ResponseMediator>
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RedisContext _redis;
        public LoginQueryHandle(UserManager<User> userManager, IJwtService jwtService, IUnitOfWork unitOfWork, RedisContext redis)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _redis = redis;
        }

        public async Task<ResponseMediator> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.Email))
            {
                if (!EmailHelper.IsValidEmail(request.Email))
                {
                    throw new ArgumentException(Messages.invalid_email_format);
                }
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    if(user.Status != Domain.Enums.UserStatus.Active)
                        return new ResponseMediator(Messages.account_inactive, null, 400);
                    var accessToken = await _jwtService.GenerateJwtTokenAsync(user, DateTime.Now.AddDays(10));
                    var refreshToken = await _jwtService.GenerateJwtTokenAsync(user, DateTime.Now.AddDays(30));
                    user.RefreshToken = refreshToken;
                    await _userManager.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                    if (roles.Any())
                    {
                        var role = _unitOfWork.Roles.Find(x => x.Name == roles.FirstOrDefault()).Include(c => c.Permissions).FirstOrDefault();
                        if(role != null)
                        {
                            var listCheckToken = _redis.GetData<List<MonitorTokenModel>>("ListMonitorToken");
                            if (listCheckToken != null)
                            {
                                var id = new MonitorTokenModel() { UserId = user.Id, RoleId = role.Id, Token = accessToken };
                                listCheckToken.Add(id);
                                listCheckToken.Add(new MonitorTokenModel() { RoleId = Guid.NewGuid(), Token = refreshToken, UserId = user.Id, });
                                _redis.SetData("ListMonitorToken", listCheckToken, DateTime.Now.AddYears(20));
                            }
                            else
                            {
                                _redis.SetData("ListMonitorToken", new List<MonitorTokenModel>() { new MonitorTokenModel() { RoleId = role.Id, Token = accessToken, UserId = user.Id} }, DateTime.Now.AddYears(20));
                            }
                            return new ResponseMediator("", new LoginResponse()
                            {
                                AccessToken = accessToken,
                                RefreshToken = refreshToken,
                                User = new RegisterResponse(role.Id, role.Name, user.Status, user.Email ?? "", user.Id, user.UserName ?? "", user.FullName, user.PhoneNumber ?? "", user.Avatar ?? "",
                                            user.Address ?? "", user.Gender, user.Dob, user.BankAccount, user.BankAccountName,
                                            user.CreateDate, user.UpdateDate, user.DeleteDate)
                                { Permissions = role.Permissions, RoleColor = role.Color}
                            });
                        }
                    }


                    var listCheckToken2 = _redis.GetData<List<MonitorTokenModel>>("ListMonitorToken");
                    if (listCheckToken2 != null)
                    {

                        listCheckToken2.Add(new MonitorTokenModel() { RoleId = Guid.NewGuid(), Token = accessToken , UserId = user.Id, });
                        listCheckToken2.Add(new MonitorTokenModel() { RoleId = Guid.NewGuid(), Token = refreshToken, UserId = user.Id, });
                        _redis.SetData("ListMonitorToken", listCheckToken2, DateTime.Now.AddYears(20));
                    }
                    else
                    {
                        _redis.SetData("ListMonitorToken", new List<MonitorTokenModel>() { new MonitorTokenModel() { RoleId = Guid.NewGuid(), Token = accessToken, UserId = user.Id } }, DateTime.Now.AddYears(20));
                    }

                    return new ResponseMediator("", new LoginResponse()
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        User = new RegisterResponse(null, null, user.Status, user.Email ?? "", user.Id, user.UserName ?? "", user.FullName, user.PhoneNumber ?? "", user.Avatar ?? "",
                                              user.Address ?? "", user.Gender, user.Dob, user.BankAccount, user.BankAccountName,
                                              user.CreateDate, user.UpdateDate, user.DeleteDate)
                    });
                }
                return new ResponseMediator(Messages.password_not_correct, null, 400);
            }
            return new ResponseMediator(Messages.account_not_found, null, 404);
        }
    }
}
