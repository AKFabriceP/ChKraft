using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Domain.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public ProjectManager? Manager { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required string? ClientName { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.PLANNING;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime ActualStartDate { get; set; }
        public DateTime ActualEndDate { get; set; }
        public decimal EstimatedBudget { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}