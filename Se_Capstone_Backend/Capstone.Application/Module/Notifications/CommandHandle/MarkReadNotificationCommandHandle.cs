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
            if(user == null)
                return new ResponseMediator("Dont have permission", null, 403);

            var notifications = _unitOfWork.Notifications.GetQuery();
            foreach(var i in request.Ids)
            {
                var notification = notifications.FirstOrDefault(x => x.Id == i);
                if(notification != null)
                {
                    if(notification.UserId != user.Id)
                        return new ResponseMediator("Dont have permission", null, 403);
                    notification.HasRead = true;
                }
            }
            _unitOfWork.Notifications.UpdateRange(notifications);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}
