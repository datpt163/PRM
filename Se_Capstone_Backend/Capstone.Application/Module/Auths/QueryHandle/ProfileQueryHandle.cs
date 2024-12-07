using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auths.Query;
using Capstone.Application.Module.Auths.Response;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace Capstone.Application.Module.Auths.QueryHandle
{
    public class ProfileQueryHandle : IRequestHandler<ProfileQuery, ResponseMediator>
    {
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public ProfileQueryHandle(IJwtService jwtService, IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<ResponseMediator> Handle(ProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _jwtService.VerifyTokenAsync(request.Token);

            if (user == null)
            {
                return new ResponseMediator(Messages.user_not_found, null);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault() != null)
            {
                var role = _unitOfWork.Roles.Find(x => x.Name == roles.FirstOrDefault()).FirstOrDefault();
                if (role != null) {
                    return new ResponseMediator("", new RegisterResponse(role.Id, role.Name, user.Status, user.Email ?? "", user.Id, user.UserName ?? "", user.FullName, user.PhoneNumber ?? "", user.Avatar ?? "",
                                              user.Address ?? "", user.Gender, user.Dob, user.BankAccount, user.BankAccountName,
                                              user.CreateDate, user.UpdateDate, user.DeleteDate));
                }
            }

            var responseUser = new RegisterResponse( null, null, user.Status, user.Email ?? "", user.Id, user.UserName ?? "", user.FullName, user.PhoneNumber ?? "", user.Avatar ?? "",
                                          user.Address ?? "", user.Gender, user.Dob, user.BankAccount, user.BankAccountName,
                                          user.CreateDate, user.UpdateDate, user.DeleteDate);
            return new ResponseMediator("", responseUser);
        }
    }
}
