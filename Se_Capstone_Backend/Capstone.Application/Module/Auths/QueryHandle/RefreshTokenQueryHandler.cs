using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auth.Response;
using Capstone.Application.Module.Auths.Model;
using Capstone.Application.Module.Auths.Query;
using Capstone.Application.Module.Auths.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Redis;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;


namespace Capstone.Application.Module.Auth.QueryHandle
{
    public class RefreshTokenQueryHandler : IRequestHandler<RefreshTokenQuery, ResponseMediator>
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RedisContext _redis;

        public RefreshTokenQueryHandler(UserManager<User> userManager, IJwtService jwtService, IOptions<JwtSettings> jwtSettings, IUnitOfWork unitOfWork, RedisContext redis)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
            _unitOfWork = unitOfWork;
            _redis = redis;
        }

        public async Task<ResponseMediator> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
        {
            var refreshToken = request.RefreshToken;
            if (!refreshToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                refreshToken = "Bearer " + refreshToken;
            }


            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken: cancellationToken);

            if (user == null)
            {
                return new ResponseMediator("Invalid refresh token", null, 400);
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(request.RefreshToken, validationParameters, out SecurityToken validatedToken);
                var jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new ResponseMediator("Invalid refresh token", null, 400);
                }

                var expiryDate = jwtToken.ValidTo;
                if (expiryDate <= DateTime.UtcNow)
                {
                    return new ResponseMediator("Refresh token has expired", null, 400);
                }

                var accessToken = await _jwtService.GenerateJwtTokenAsync(user, DateTime.Now.AddDays(10));
                var newRefreshToken = await _jwtService.GenerateJwtTokenAsync(user, DateTime.Now.AddDays(30));

                user.RefreshToken = newRefreshToken;
                await _userManager.UpdateAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.FirstOrDefault() != null)
                {
                    var role = _unitOfWork.Roles.Find(x => x.Name == roles.FirstOrDefault()).Include(c => c.Permissions).FirstOrDefault();
                    if (role != null)
                    {
                        var listCheckToken = _redis.GetData<List<MonitorTokenModel>>("ListMonitorToken");
                        if (listCheckToken != null)
                        {

                            listCheckToken.Add(new MonitorTokenModel() { RoleId = role.Id, Token = accessToken });
                            _redis.SetData<List<MonitorTokenModel>>("ListMonitorToken", listCheckToken, DateTime.Now.AddYears(20));
                        }
                        else
                        {
                            _redis.SetData<List<MonitorTokenModel>>("ListMonitorToken", new List<MonitorTokenModel>() { new MonitorTokenModel() { RoleId = role.Id, Token = accessToken } }, DateTime.Now.AddYears(20));
                        }

                        return new ResponseMediator("", new LoginResponse()
                        {
                            AccessToken = accessToken,
                            RefreshToken = newRefreshToken,
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
                    RefreshToken = newRefreshToken,
                    User = new RegisterResponse(null, null, user.Status, user.Email ?? "", user.Id, user.UserName ?? "", user.FullName, user.PhoneNumber ?? "", user.Avatar ?? "",
                                          user.Address ?? "", user.Gender, user.Dob, user.BankAccount, user.BankAccountName,
                                          user.CreateDate, user.UpdateDate, user.DeleteDate)
                });
            }
            catch (SecurityTokenExpiredException)
            {
                return new ResponseMediator("Refresh token has expired", null, (int)HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                return new ResponseMediator($"Token validation failed: {ex.Message}", null, (int)HttpStatusCode.Unauthorized);
            }
        }
    }
}
