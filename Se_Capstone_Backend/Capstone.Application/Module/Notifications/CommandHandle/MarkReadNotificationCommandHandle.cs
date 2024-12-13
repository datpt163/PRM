using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Notifications.Command;
using Capstone.Application.Resources;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Notifications.CommandHandle
{
    public class MarkReadNotificationCommandHandle : IRequestHandler<MarkReadNotificationCommand, ResponseMediator>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        public MarkReadNotificationCommandHandle(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(MarkReadNotificationCommand request, CancellationToken cancellationToken)
        {
            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new ResponseMediator(Messages.user_no_permission_delete_notification, null, 403);

            var notification = _unitOfWork.Notifications.FindOne(x => x.Id == request.Id);

            if (notification != null)
            {
                if (notification.UserId != user.Id)
                    return new ResponseMediator(Messages.user_no_permission_delete_notification, null, 403);
                notification.HasRead = true;
                _unitOfWork.Notifications.Update(notification);
                await _unitOfWork.SaveChangesAsync();
                return new ResponseMediator("", null);
            }

            return new ResponseMediator(Messages.notification_not_found, null, 404);
        }
    }
}
