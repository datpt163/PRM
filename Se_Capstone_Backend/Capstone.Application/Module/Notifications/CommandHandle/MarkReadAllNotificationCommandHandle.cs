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
    public class MarkReadAllNotificationCommandHandle : IRequestHandler<MarkReadAllNotificationCommand, ResponseMediator>
    {
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;

        public MarkReadAllNotificationCommandHandle(IJwtService jwtService, IUnitOfWork unitOfWork)
        {
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMediator> Handle(MarkReadAllNotificationCommand request, CancellationToken cancellationToken)
        {
            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new ResponseMediator("Some thing wrong", null);

            var notifications = _unitOfWork.Notifications.Find(x => x.UserId == user.Id);
            foreach(var n in notifications)
                n.HasRead = true;
            _unitOfWork.Notifications.UpdateRange(notifications);
            await _unitOfWork.SaveChangesAsync();
            return new ResponseMediator("", null);
        }
    }
}
