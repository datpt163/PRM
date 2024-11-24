namespace Capstone.Api.Module.Users.Models
{
    public class GetActiveUsersRequest
    {
        public List<Guid> UserInProject { get; set; } = new List<Guid>();

    }
}
