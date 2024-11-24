using Capstone.Application.Module.Users.Response;
using MediatR;


namespace Capstone.Application.Module.Users.Query
{
    public class GetActiveUsersQuery : IRequest<List<UserStatisticsResponse>>
    {
        public List<Guid> UserInProject { get; set; } = new List<Guid>();
    }
}
