namespace ChronoKraft.Domain.Entities
{
    public enum InvoiceType
    {
        PARTIAL,
        FINAL
    }
    public class Invoice
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public int ContractorId { get; set; }
        public Contractor? Contractor { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public InvoiceType InvoiceType { get; set; } = InvoiceType.PARTIAL;
        public DateOnly IssueDate { get; set; }
        public DateOnly DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    }
}