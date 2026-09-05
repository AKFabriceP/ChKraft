using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Domain.Entities
{
    public class ExecutionRecord
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int CreatedById { get; set; }
        public int? SourceTimesheetEntryId { get; set; }
        public DateTime ExecutionDate { get; set; }
        public int ResourcesUsed { get; set; }
        public decimal QuantityPerResource { get; set; }
        public decimal AppliedRate { get; set; }
        public RateType RateType { get; set; } = RateType.STANDARD;
        public decimal Multiplier { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}