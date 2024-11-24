using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Notifications.Command;
using Capstone.Infrastructure.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Notifications.CommandHandle
{
    public class DeleteNotificationCommandHandle : IRequestHandler<DeleteNotificationCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        public DeleteNotificationCommandHandle(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = _unitOfWork.Notifications.FindOne(x => x.Id == request.Id);
            if (notification == null)
                return new ResponseMediator("Notification not found", null, 404);

            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new ResponseMediator("Token wrong", null, 403);
            if(notification.UserId != user.Id)
                return new ResponseMediator("User dont have permission to delete this notification", null, 403);

            _unitOfWork.Notifications.Remove(notification);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}
