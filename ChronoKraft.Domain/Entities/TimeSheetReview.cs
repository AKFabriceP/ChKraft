namespace ChronoKraft.Domain.Entities
{
    public enum ReviewStatus
    {
        PENDING,
        RESOLVED,
        IGNORED
    }
    public class TimeSheetReview
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public Worker? Worker { get; set; }
        public DateTime WorkDate { get; set; }
        public decimal ExternalTotal { get; set; }
        public decimal DeclaredTotal { get; set; }
        public decimal Discrepancy { get; set; }
        public ReviewStatus Status { get; set; } = ReviewStatus.PENDING;
        public int ReviewedById { get; set; }
        public DateTime ReviewedAt { get; set; }
        public string? ResolutionNotes { get; set; }
    }
}