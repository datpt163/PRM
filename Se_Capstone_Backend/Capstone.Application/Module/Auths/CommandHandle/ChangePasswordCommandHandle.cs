using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auths.Command;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace Capstone.Application.Module.Auths.CommandHandle
{
    public class ChangePasswordCommandHandle : IRequestHandler<ChangePasswordCommand, ResponseMediator>
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        public ChangePasswordCommandHandle(IJwtService jwtService, IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ResponseMediator> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {   
            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if(user == null)
                return new ResponseMediator(Messages.invalid_token, null);

            if (!await _userManager.CheckPasswordAsync(user, request.OldPassword))
                return new ResponseMediator(Messages.old_password_not_correct, null);
            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, request.NewPassword);
            return new ResponseMediator("", null);
        }
    }
}
