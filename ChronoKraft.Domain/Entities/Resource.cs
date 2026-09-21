namespace ChronoKraft.Domain.Entities
{
    public class Resource
    {
        public int Id { get; set; }
        public int ContractorId { get; set; }
        public Contractor? Contractor { get; set; }
        public int ResourceTypeId { get; set; }
        public ResourceType? ResourceType { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal BaseRate { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();
        public string? Notes { get; set; }
        public bool Available { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ResourceAssignment> ResourceAssignments { get; set; } = new List<ResourceAssignment>();
    }
}