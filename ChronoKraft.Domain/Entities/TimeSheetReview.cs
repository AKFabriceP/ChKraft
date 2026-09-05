namespace ChronoKraft.Domain.Entities
{
    public class TimeSheetReview
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public Worker? Worker { get; set; }
        public DateTime WorkDate { get; set; }
        public decimal ExternalTotal { get; set; }
        public decimal DeclaredTotal { get; set; }
        public decimal Discrepancy { get; set; }
        public string Status { get; set; } = "pending";
        public int ReviewedById { get; set; }
        public DateTime ReviewedAt { get; set; }
        public string? ResolutionNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}