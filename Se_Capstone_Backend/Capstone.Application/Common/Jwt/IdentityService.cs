// Application/Services/IdentityService.cs
using System.Threading.Tasks;
using Capstone.Application.Common.Jwt;  // Dịch vụ JWT của bạn
using Capstone.Domain.Entities;
using Capstone.Domain.Entities.Common;
using Microsoft.AspNetCore.Http;

public class IdentityService : IIdentityService
{
    private readonly IJwtService _jwtService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityService(IJwtService jwtService, IHttpContextAccessor httpContextAccessor)
    {
        _jwtService = jwtService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid?> GetUserIdFromTokenAsync()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var user = await _jwtService.VerifyTokenAsync(token);
        return user?.Id;
    }

    public async Task<string?> GetUserNameFromTokenAsync()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var user = await _jwtService.VerifyTokenAsync(token);
        return user?.FullName;
    }

    public async Task SetUpdatedByAsync(BaseEntity entity)
    {
        var userId = await GetUserIdFromTokenAsync();
        if (userId.HasValue)
        {
            entity.UpdatedBy = userId.Value;
        }
    }
}
