using Capstone.Application.Module.Report.Response;
using MediatR;

namespace Capstone.Application.Module.Report.Query
{
    public class GetSkillsReportQuery : IRequest<List<SkillReport>>
    {
        public string? Title { get; set; }
        public int? MinimumUsers { get; set; }
        public int? MaximumUsers { get; set; }
        public Guid? UserId { get; set; }

        
    }
}
