using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Auths.Command;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Capstone.Application.Resources;



namespace Capstone.Application.Module.Auths.CommandHandle
{
    public class ChangePassUserCommanHandle : IRequestHandler<ChangePassUserCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        public ChangePassUserCommanHandle(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ResponseMediator> Handle(ChangePassUserCommand request, CancellationToken cancellationToken)
        {
            var user = _unitOfWork.Users.Find(x => x.Id == request.UserId).FirstOrDefault();
            if (user == null)
                return new ResponseMediator(Messages.user_not_found, "", 404);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(error => error.Description).ToList();
                var errorMessage = string.Join(", ", errors);
                return new ResponseMediator(errorMessage, null,400);
            }
            return new ResponseMediator("", null);
        }
    }
}
