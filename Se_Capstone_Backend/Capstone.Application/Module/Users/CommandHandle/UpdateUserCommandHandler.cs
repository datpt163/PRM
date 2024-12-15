using Capstone.Application.Common.Cloudinaries;
using Capstone.Application.Common.Jwt;
using Capstone.Application.Module.Auths.Model;
using Capstone.Application.Module.Users.Command;
using Capstone.Application.Module.Users.Response;
using Capstone.Domain.Entities;
using Capstone.Domain.Module.Auth.TokenBlackList;
using Capstone.Infrastructure.Redis;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Application.Module.Users.CommandHandle
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto?>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly RedisContext _redis;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        public UpdateUserCommandHandler(IRepository<Role> roleRepository, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IRepository<User> userRepository, CloudinaryService cloudinaryService, IJwtService jwtService, UserManager<User> userManager, RedisContext redis, ITokenBlacklistService tokenBlacklistService)
        {
            _userRepository = userRepository;
            _cloudinaryService = cloudinaryService;
            _jwtService = jwtService;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _roleRepository = roleRepository;
            _userManager = userManager;
            _tokenBlacklistService = tokenBlacklistService;
            _redis = redis;
        }

        public async Task<UserDto?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            Guid? roleId = null;
            string? roleName = null;
            var user = new User();
            if (request.Id == null)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    string token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                    user = await _jwtService.VerifyTokenAsync(token);
                }
            }
            else
            {
                user = await _userRepository.GetQuery()
                            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            }


            if (user == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(request.FullName))
            {
                user.FullName = request.FullName;
            }

            if (!string.IsNullOrEmpty(request.Phone))
            {
                user.PhoneNumber = request.Phone;
            }

            if (!string.IsNullOrEmpty(request.Address))
            {
                user.Address = request.Address;
            }

            if (request.Gender.HasValue)
            {
                user.Gender = (Domain.Enums.Gender)request.Gender.Value;
            }
            if (request.Status.HasValue)
            {
                user.Status = (Domain.Enums.UserStatus)request.Status.Value;
            }
            if (request.Dob.HasValue)
            {
                user.Dob = request.Dob;
            }

            if (!string.IsNullOrEmpty(request.BankAccount))
            {
                user.BankAccount = request.BankAccount;
            }

            if (!string.IsNullOrEmpty(request.BankAccountName))
            {
                user.BankAccountName = request.BankAccountName;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var roleResponse = _unitOfWork.Roles.Find(x => x.Name != null && currentRoles.Count() > 0 && x.Name.ToUpper().Trim().Equals(currentRoles.FirstOrDefault())).FirstOrDefault();
            roleId = roleResponse?.Id;
            roleName = roleResponse?.Name;

            if (request.RoleId.HasValue)
            {
                var role = _roleRepository.GetQuery().FirstOrDefault(x => x.Id == request.RoleId.Value);
                if (role != null)
                {

                    if (currentRoles != null && currentRoles.Count() > 0)
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    var roleQuery = await _unitOfWork.Roles.FindOneAsync(x => x.Id == request.RoleId);
                    if (roleQuery != null)
                    {
                        roleId = roleQuery.Id;
                        roleName = roleQuery.Name;
                        await _userManager.AddToRolesAsync(user, new List<String>() { roleName ?? "" }); 
                    }
                }
            }


            if (request.AvatarFile != null)
            {
                using var stream = request.AvatarFile.OpenReadStream();
                var avatarUrl = await _cloudinaryService.UploadImageAsync(stream, request.AvatarFile.FileName);
                if (user.Avatar != null)
                {
                    var isDeletedAvatar = await _cloudinaryService.DeleteImageByUrlAsync(user.Avatar);
                }
                user.Avatar = avatarUrl;
            }

            user.UpdateDate = DateTime.Now;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();


            var listMonitorToken = _redis.GetData<List<MonitorTokenModel>>("ListMonitorToken");
            if (listMonitorToken != null)
            {
                var tokensToRemove = new List<MonitorTokenModel>();

                foreach (var monitorToken in listMonitorToken)
                {
                    if (request.Id != null && monitorToken.UserId == request.Id.Value)
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

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Phone = user.PhoneNumber ?? string.Empty,
                Avatar = user.Avatar ?? string.Empty,
                Address = user.Address,
                Gender = (int)user.Gender,
                Status = (int)user.Status,
                Dob = user.Dob,
                BankAccount = user.BankAccount,
                BankAccountName = user.BankAccountName,
                CreateDate = user.CreateDate,
                UpdateDate = user.UpdateDate,
                RoleId = roleId + "",
                RoleName = roleName
            };
        }
    }
}
