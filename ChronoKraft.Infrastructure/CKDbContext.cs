using ChronoKraft.Domain.Entities;
using ChronoKraft.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChronoKraft.Infrastructure;

public class CKDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public CKDbContext(DbContextOptions<CKDbContext> options) : base(options) {}

    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<ExecutionRecord> ExecutionRecords => Set<ExecutionRecord>();
    public DbSet<ExternalTimeTotal> ExternalTimeTotals => Set<ExternalTimeTotal>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectManager> ProjectManagers => Set<ProjectManager>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceAssignment> ResourceAssignments => Set<ResourceAssignment>();
    public DbSet<ResourceType> ResourceTypes => Set<ResourceType>();
    public DbSet<TimeSheetEntry> TimeSheetEntries => Set<TimeSheetEntry>();
    public DbSet<TimeSheetReview> TimeSheetReviews => Set<TimeSheetReview>();
    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<WorkerSkill> WorkerSkills => Set<WorkerSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Definir les contraintes de clé unique pour les entités
        // WorkerSkill => (WorkerId, ResourceTypeId) a worker may have multiple skills
        modelBuilder.Entity<WorkerSkill>()
            .HasIndex(ws => new { ws.WorkerId, ws.ResourceTypeId })
            .IsUnique();
        // ExternalTimeTotal => (WorkerId, work_date)
        modelBuilder.Entity<ExternalTimeTotal>()
            .HasIndex(ett => new { ett.WorkerId, ett.WorkDate })
            .IsUnique();
        // ResourceAssignment => (ResourceId, ProjectTaskId)
        // Une resource ne peut etre attribuée qu'à une seule tâche mais le nombre peut varier.
        modelBuilder.Entity<ResourceAssignment>()
            .HasIndex(ra => new { ra.ResourceId, ra.ProjectTaskId })
            .IsUnique();

        // Colonnes jsonb
        modelBuilder.Entity<Contractor>()
            .Property(c => c.Metadata)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Project>()
            .Property(p => p.Metadata)
            .HasColumnType("jsonb");
        modelBuilder.Entity<ProjectTask>()
            .Property(t => t.Attributes)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Resource>()
            .Property(r => r.Attributes)
            .HasColumnType("jsonb");
        modelBuilder.Entity<ResourceType>()
            .Property(rt => rt.StandardAttributes)
            .HasColumnType("jsonb");
        modelBuilder.Entity<WorkerSkill>()
            .Property(ws => ws.Certifications)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Invoice>()
            .Property(i => i.Metadata)
            .HasColumnType("jsonb");
        modelBuilder.Entity<ResourceAssignment>()
            .Property(ra => ra.Attributes)
            .HasColumnType("jsonb");

        // Index pour la recherche de profil par UserId
        modelBuilder.Entity<ProjectManager>().HasIndex(pm => pm.UserId);
        modelBuilder.Entity<Contractor>().HasIndex(c => c.UserId);
        modelBuilder.Entity<Worker>().HasIndex(w => w.UserId);

        // Enums stockés en texte, pas en entier — permet d'ajouter une valeur future 
        // sans migration et sans risque de décalage silencieux des données existantes
        // (voir ADR-0013)
        modelBuilder.Entity<ApplicationUser>().Property(u => u.Role).HasConversion<string>();
        modelBuilder.Entity<ResourceType>().Property(rt => rt.ResourceCategory).HasConversion<string>();
        modelBuilder.Entity<ResourceType>().Property(rt => rt.DefaultBillingUnit).HasConversion<string>();
        modelBuilder.Entity<TimeSheetReview>().Property(tr => tr.Status).HasConversion<string>();
        modelBuilder.Entity<Invoice>().Property(i => i.InvoiceType).HasConversion<string>();
        modelBuilder.Entity<Project>().Property(p => p.Status).HasConversion<string>();
        modelBuilder.Entity<ProjectTask>().Property(t => t.Status).HasConversion<string>();
        modelBuilder.Entity<ResourceAssignment>().Property(ra => ra.BillingUnit).HasConversion<string>();
        modelBuilder.Entity<ExecutionRecord>().Property(er => er.RateType).HasConversion<string>();
    }
}
