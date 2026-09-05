using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Domain.Entities
{
    public enum ResourceCategory
    {
        LABOR,
        EQUIPMENT,
        MATERIALS,
    }
    public class ResourceType
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ResourceCategory ResourceCategory { get; set; }
        public string? Trade { get; set; }
        public BillingUnit DefaultBillingUnit { get; set; } = BillingUnit.HOUR;
        public decimal SuggestedRateMin { get; set; }
        public decimal SuggestedRateMax { get; set; }
        public string? Description { get; set; }
        public string? StandardAttributes { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}