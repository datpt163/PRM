using Capstone.Application.Module.Projects.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Application.Module.Projects.Query
{
    public class SuggestProjectQuery : IRequest<List<SuggestMapping>>
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDetail { get; set; } = string.Empty;
        public List<UserStatistic> UserStatistics { get; set; } = new List<UserStatistic>();

        public int TotalUsersNeed { get; set; } = 3;
    }
    public class UserStatistic
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public int ActiveProjectCount { get; set; }
    }
}
