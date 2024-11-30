using Capstone.Domain.Entities.Common;
using Capstone.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone.Domain.Entities
{
    [Table("projects")]
    public class Project : BaseEntity

    {
        public Project()
        {
        }

        public Project(string name, string code, string description, DateTime? startDate, DateTime? endDate, Guid? leadId, bool isVisible)
        {
            Name = name;
            Code = code;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            Status = ProjectStatus.NotStarted;
            CreatedAt = DateTime.Now;
            LeadId = leadId;
            IsVisible = isVisible;
        }

        public Guid Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;
        public float? TotalEffort { get; set; } = 0;

        [MaxLength(100)]
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.NotStarted;
        public bool IsVisible { get; set; } = false;
        public Guid? LeadId { get; set; }
        public User? Lead { get; set; }
        public ICollection<Phase> Phases { get; set; } = new List<Phase>();
        public ICollection<Status> Statuses { get; set; } = new List<Status>();
        public ICollection<Label> Labels { get; set; } = new List<Label>(); 
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();

        public (PhaseStatus status, Phase? phaseRunning, Phase? phaseAfterPhaseRunning) GetStatusPhaseOfProject()
        {
            Phases = Phases.OrderBy(c => c.ExpectedStartDate).ToList();
            var phaseFirst = Phases.FirstOrDefault();
            if (phaseFirst == null)
                return (PhaseStatus.NoPhase, null, null);

            if (!phaseFirst.ActualStartDate.HasValue)
                return (PhaseStatus.NoPhaseRunning, null, null);


            for (int i = 0; i < Phases.Count; i++)
            {
                var phase = Phases.ToList()[i];
                if (phase.ActualStartDate.HasValue && !phase.ActualEndDate.HasValue)
                {
                    var nextPhase = (i + 1 < Phases.Count) ? Phases.ToList()[i + 1] : null;
                    if(nextPhase == null)
                        return (PhaseStatus.Complete, phase, null);
                    return (PhaseStatus.Running, phase, nextPhase);
                }
            }

            return (PhaseStatus.Other, null, null);
        }
    }
}
