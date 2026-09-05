namespace ChronoKraft.Domain.Entities
{
    public class Resource
    {
        public int Id { get; set; }
        public int ContractorId { get; set; }
        public int ResourceTypeId { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal BaseRate { get; set; }
        public string? Attributes { get; set; }
        public string? Notes { get; set; }
        public bool Available { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}