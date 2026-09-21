namespace ChronoKraft.Domain.Entities
{
    public class WorkerSkill
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public int ResourceTypeId { get; set; }
        public ResourceType? ResourceType { get; set; }
        public decimal? HourlyRate { get; set; }
        public Dictionary<string, object>? Certifications { get; set; } = new();
        public bool IsPrimary { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}