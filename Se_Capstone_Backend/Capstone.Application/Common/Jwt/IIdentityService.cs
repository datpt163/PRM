using System;
using System.Threading.Tasks;
using Capstone.Domain.Entities;
using Capstone.Domain.Entities.Common;

public interface IIdentityService
{
    Task<Guid?> GetUserIdFromTokenAsync();
    Task<string?> GetUserNameFromTokenAsync();
    Task SetUpdatedByAsync(BaseEntity entity);  
}
