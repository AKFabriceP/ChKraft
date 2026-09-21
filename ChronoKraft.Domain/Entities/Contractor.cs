namespace ChronoKraft.Domain.Entities
{
    public class Contractor
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public required string Name { get; set; }
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? BusinessNumber { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Worker> Workers { get; set; } = new List<Worker>();
        public ICollection<Resource> ContractorResources { get; set; } = new List<Resource>();
    }
}