namespace ChronoKraft.Domain.Entities
{
    public class ProjectManager
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public required string Name { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}