using Capstone.Application.Common.Paging;
using Capstone.Application.Module.Users.Response;
using MediatR;

namespace Capstone.Application.Module.Users.Query
{
    public class GetUserListQuery : PagingQuery, IRequest<PagingResultSP<UsersDto>>
    {
        public string? Search { get; set; } = string.Empty;
        public string? FullName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public string? Address { get; set; } = null;
        public int? Gender { get; set; } = null;
        public int? Status { get; set; } = null;
        public string? RoleName { get; set; } = string.Empty;
        public string? UserName { get; set; } = string.Empty;
        public Guid? RoleId { get; set; } 
        public DateTime? DobFrom { get; set; } = null;
        public DateTime? DobTo { get; set; } = null;

    }
}
