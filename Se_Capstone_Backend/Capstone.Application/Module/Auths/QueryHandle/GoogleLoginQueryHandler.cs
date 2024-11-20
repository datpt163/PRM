using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auth.Response;
using Capstone.Application.Module.Auths.Model;
using Capstone.Application.Module.Auths.Query;
using Capstone.Application.Module.Auths.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Redis;
using Capstone.Infrastructure.Repository;
using CloudinaryDotNet;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;


namespace Capstone.Application.Module.Auths.QueryHandle
{
    public class GoogleLoginQueryHandler : IRequestHandler<GoogleLoginQuery, ResponseMediator>
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RedisContext _redis;

        public GoogleLoginQueryHandler(UserManager<User> userManager, IJwtService jwtService, IUnitOfWork unitOfWork, RedisContext redis)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _redis = redis;

        }

        public async Task<ResponseMediator> Handle(GoogleLoginQuery request, CancellationToken cancellationToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
            if (payload == null)
            {
                return new ResponseMediator("Invalid Google token", null, 400);
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                return new ResponseMediator("Account not found", null, 404);

            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _jwtService.GenerateJwtTokenAsync(user, DateTime.Now.AddDays(10));
            var refreshToken = await _jwtService.GenerateJwtTokenAsync(user, DateTime.Now.AddDays(30));
            user.RefreshToken = "Bearer " + refreshToken;
            await _userManager.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            if (roles.Any())
            {
                var role = _unitOfWork.Roles.Find(x => x.Name == roles.FirstOrDefault()).Include(c => c.Permissions).FirstOrDefault();
                if (role != null)
                {
                    var listCheckToken = _redis.GetData<List<MonitorTokenModel>>("ListMonitorToken");
                    if (listCheckToken != null)
                    {

                        listCheckToken.Add(new MonitorTokenModel() { RoleId = role.Id, Token = accessToken });
                        _redis.SetData("ListMonitorToken", listCheckToken, DateTime.Now.AddYears(20));
                    }
                    else
                    {
                        _redis.SetData("ListMonitorToken", new List<MonitorTokenModel>() { new MonitorTokenModel() { RoleId = role.Id, Token = accessToken } }, DateTime.Now.AddYears(20));
                    }

                    return new ResponseMediator("", new LoginResponse()
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        User = new RegisterResponse(role.Id, role.Name, user.Status, user.Email ?? "", user.Id, user.UserName ?? "", user.FullName, user.PhoneNumber ?? "", user.Avatar ?? "",
                                    user.Address ?? "", user.Gender, user.Dob, user.BankAccount, user.BankAccountName,
                                    user.CreateDate, user.UpdateDate, user.DeleteDate)
                        { Permissions = role.Permissions, RoleColor = role.Color }
                    });
                }
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
    }

}
