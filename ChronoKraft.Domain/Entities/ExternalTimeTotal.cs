namespace ChronoKraft.Domain.Entities
{
    public class ExternalTimeTotal
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public DateTime WorkDate { get; set; }
        public decimal TotalHours { get; set; }
        public string SourceSystem { get; set; } = string.Empty;
        public string ExternalReferenceId { get; set; } = string.Empty;
        public DateTime SyncedAt { get; set; }
    }
}