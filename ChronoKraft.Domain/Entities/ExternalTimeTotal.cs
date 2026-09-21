namespace ChronoKraft.Domain.Entities
{
    public class ExternalTimeTotal
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public DateOnly WorkDate { get; set; }
        public decimal TotalHours { get; set; }
        public string? SourceSystem { get; set; }
        public string? ExternalReferenceId { get; set; }
        public DateTime SyncedAt { get; set; }
    }
}