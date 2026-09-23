using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Domain.Entities
{
    public class ResourceAssignment
    {
        public int Id { get; set; }
        public int ProjectTaskId { get; set; }
        public ProjectTask? ProjectTask { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
        public int? WorkerId { get; set; }
        public Worker? Worker { get; set; }
        public int QuantityResources { get; set; }
        public decimal EstimatedQuantity { get; set; }
        public decimal EstimatedUnitCost { get; set; }
        public BillingUnit BillingUnit { get; set; } = BillingUnit.HOUR;
        public string? Notes { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ExecutionRecord> ExecutionRecords { get; set; } = new List<ExecutionRecord>();
    }
}