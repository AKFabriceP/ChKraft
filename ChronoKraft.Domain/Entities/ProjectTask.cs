using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Domain.Entities
{
    public class ProjectTask
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string? RequiredTrade { get; set; }
        public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.PENDING;
        public int Order { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ResourceAssignment> ResourceAssignments { get; set; } = new List<ResourceAssignment>();
    }
}