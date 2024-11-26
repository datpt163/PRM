using Capstone.Application.Common.Jwt;
using Capstone.Application.Common.Paging;
using Capstone.Application.Common.ResponseMediator;
using Capstone.Application.Module.Notifications.Query;
using Capstone.Application.Module.Projects.Response;
using Capstone.Domain.Entities;
using Capstone.Infrastructure.Repository;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Notifications.QueryHandle
{
    public class GetListNotificationQueryHandle : IRequestHandler<GetListNotificationQuery, PagingResultSP<Notification>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        public GetListNotificationQueryHandle(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagingResultSP<Notification>> Handle(GetListNotificationQuery request, CancellationToken cancellationToken)
        {
            var user = await _jwtService.VerifyTokenAsync(request.Token);
            if (user == null)
                return new PagingResultSP<Notification>() { ErrorMessage = "Wrong Token" };

            var notifications = _unitOfWork.Notifications.Find(x => x.UserId == user.Id).OrderByDescending(x => x.CreatedAt).ToList();
            var unReadCount = notifications.Where(x => x.HasRead == false).Count();
            if (request.PageIndex.HasValue && request.PageSize.HasValue)
            {
                if (request.PageIndex.Value < 1 || request.PageSize.Value < 0)
                    return new PagingResultSP<Notification>() { ErrorMessage = "PageIndex, PageSize must >= 0" };

                int skip = (request.PageIndex.Value - 1) * request.PageSize.Value;
                var notificationPaging = notifications.Skip(skip).Take(request.PageSize.Value).ToList();
                var totalCount = notifications.Count();
                var result = new PagingResultSP<Notification>(notificationPaging, totalCount, request.PageIndex.Value, request.PageSize.Value) { Count = unReadCount };
                return result;
            }
            return new PagingResultSP<Notification>() { Data = notifications };

        }
    }
}
