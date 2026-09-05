namespace ChronoKraft.Domain.Entities
{
    public class TimeSheetEntry
    {
        public int Id { get; set; }
        public int WorkerSkillId { get; set; }
        public WorkerSkill? WorkerSkill { get; set; }
        public int TaskId { get; set; }
        public ProjectTask? ProjectTask { get; set; }
        public DateTime WorkDate { get; set; }
        public decimal Hours { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

