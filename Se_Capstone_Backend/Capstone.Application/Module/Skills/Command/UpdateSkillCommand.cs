using Capstone.Application.Module.Skills.Response;
using MediatR;

namespace Capstone.Application.Module.Skills.Command
{
    public class UpdateSkillCommand : IRequest<SkillDto?>
    {
        public Guid Id { get; set; }
        public string? Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? Level { get; set; } = string.Empty;
        public bool? Isdeleted { get; set; } 
    }
}
