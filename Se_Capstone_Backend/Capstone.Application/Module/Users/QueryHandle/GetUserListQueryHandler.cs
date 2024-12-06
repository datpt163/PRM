using Capstone.Application.Common.Paging;
using Capstone.Application.Module.Users.Query;
using Capstone.Application.Module.Users.Response;
using Capstone.Domain.Entities;
using Capstone.Domain.Helpers;
using Capstone.Infrastructure.Helpers;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Capstone.Application.Module.Users.QueryHandle
{
    public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, PagingResultSP<UsersDto>>
    {
        private readonly IRepository<User> _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<Role> _roleManager;

     public GetUserListQueryHandler(IRepository<User> userRepository, UserManager<User> userManager, IUnitOfWork unitOfWork, RoleManager<Role> roleManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager; 
        }

        public async Task<PagingResultSP<UsersDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
        {
            var usersQuery = _userRepository.GetQueryNoTracking().Where(x => x.UserName != null);

            
            if (!string.IsNullOrEmpty(request.Phone))
            {
                //if (!PhoneNumberValidator.Validate(request.Phone))
                //{
                //    throw new ArgumentException("Invalid phone number format.");
                //}

                usersQuery = usersQuery.Where(user => user.PhoneNumber != null && user.PhoneNumber.Contains(request.Phone));
            }
            if (!string.IsNullOrEmpty(request.Email))
            {
                //if (!EmailHelper.IsValidEmail(request.Email))
                //{
                //    throw new ArgumentException("Invalid email format.");
                //}
                usersQuery = usersQuery.Where(user => user.Email != null && user.Email.ToLower().Contains(request.Email.ToLower()));
            }
            if (!string.IsNullOrEmpty(request.UserName))
            {
                usersQuery = usersQuery.Where(user => user.UserName != null && user.UserName.ToLower().Contains(request.UserName.ToLower()));
            }
            if (!string.IsNullOrEmpty(request.Search))
            {
                usersQuery = usersQuery.Where(user =>
                    (user.PhoneNumber != null && user.PhoneNumber.Contains(request.Search)) ||
                    (user.FullName.ToLower().Contains(request.Search.ToLower())) ||
                    (request.Email != null && user.Email != null && user.Email.ToLower().Contains(request.Email.ToLower()))
                );

            }
            if (!string.IsNullOrEmpty(request.FullName))
            {
                usersQuery = usersQuery.Where(user => user.FullName.ToLower().Contains(request.FullName.ToLower()));
            }
            
            if (!string.IsNullOrEmpty(request.RoleName))
            {
                var role = _unitOfWork.Roles.FindOne(x => x.Name == request.RoleName.Trim().ToUpper());

                if (role != null)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                    usersQuery = usersQuery.Where(u => usersInRole.Select(ur => ur.Id).Contains(u.Id));
                }
                else
                {
                    usersQuery = Enumerable.Empty<User>().AsQueryable();
                }
            }
            if (request.RoleId != null)
            {
                var role = _unitOfWork.Roles.FindOne(x => x.Id == request.RoleId);

                if (role != null)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                    usersQuery = usersQuery.Where(u => usersInRole.Select(ur => ur.Id).Contains(u.Id));
                }
                else
                {
                    usersQuery = Enumerable.Empty<User>().AsQueryable();
                }
            }


            if (!string.IsNullOrEmpty(request.Address))
            {
                usersQuery = usersQuery.Where(user => user.Address != null && user.Address.Contains(request.Address));
            }

            if (request.Gender.HasValue)
            {
                usersQuery = usersQuery.Where(user => (int)user.Gender == request.Gender.Value);
            }
            if (request.Status.HasValue)
            {
                usersQuery = usersQuery.Where(user => (int)user.Status == request.Status.Value);
            }
            if (request.DobFrom.HasValue)
            {
                usersQuery = usersQuery.Where(user => user.Dob >= request.DobFrom.Value);
            }

            if (request.DobTo.HasValue)
            {
                usersQuery = usersQuery.Where(user => user.Dob <= request.DobTo.Value);
            }

            var pagedUsersQuery = usersQuery.OrderByAndPaginate(request).ToList();

            foreach (var user in pagedUsersQuery)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles != null && roles.Count > 0)
                {
                    var role = await _unitOfWork.Roles.FindOneAsync(x => x.Name != null && x.Name.Equals(roles.FirstOrDefault()));
                    user.Roles = new List<Role>() { role };
                }
            }

            var userList = pagedUsersQuery
                .Select(user => new UsersDto
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
                    RoleId = user.Roles.Count() == 0 ? "" : user.Roles.Select(r => r.Id.ToString()).FirstOrDefault(),
                    RoleName = user?.Roles?.Count() == 0 ? "" : user?.Roles?.Select(r => r?.Name?.ToString()).FirstOrDefault() ?? "",
                    UserName = user?.UserName,
                    RoleColor = user?.Roles?.Count() == 0 ? "" : user?.Roles?.Select(r => r?.Color?.ToString()).FirstOrDefault() ?? "",
                }).ToList();


            int totalCount = await usersQuery.CountAsync(cancellationToken);

            var result = new PagingResultSP<UsersDto>(userList, totalCount, request.PageIndex, request.PageSize);

            return result;
        }
    }
}
