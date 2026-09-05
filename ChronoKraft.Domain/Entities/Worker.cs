namespace ChronoKraft.Domain.Entities
{
    public class Worker
    {
        public int Id { get; set; }
        public int ContractorId { get; set; }
        public int? UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Phone { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WorkerSkill> Skills { get; set; } = new List<WorkerSkill>();
    }
}