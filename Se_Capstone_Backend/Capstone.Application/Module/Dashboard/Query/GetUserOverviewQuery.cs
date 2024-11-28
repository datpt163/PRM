using Capstone.Application.Module.Dashboard.Response;
using MediatR;

namespace Capstone.Application.Module.Dashboard.Query
{
    public class GetUserOverviewQuery : IRequest<UserOverviewResponse>
    {
        public Guid UserId { get; set; }
    }
}
