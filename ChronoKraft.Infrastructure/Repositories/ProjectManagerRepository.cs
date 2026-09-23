using ChronoKraft.Application.Interfaces;
using ChronoKraft.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChronoKraft.Infrastructure.Repositories;

public class ProjectManagerRepository : IProjectManagerRepository
{
    private readonly CKDbContext _db;

    public ProjectManagerRepository(CKDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProjectManager>> GetAllAsync()
        => await _db.ProjectManagers
            .Include(pm => pm.Projects)
            .ToListAsync();

    public async Task<ProjectManager?> GetByIdAsync(int id)
        => await _db.ProjectManagers.FindAsync(id);

    public async Task<ProjectManager?> GetByUserIdAsync(int userId)
        => await _db.ProjectManagers
            .FirstOrDefaultAsync(pm => pm.UserId == userId);

    public async Task AddAsync(ProjectManager projectManager)
    {
        projectManager.CreatedAt = DateTime.UtcNow;
        projectManager.UpdatedAt = DateTime.UtcNow;

        _db.ProjectManagers.Add(projectManager);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProjectManager projectManager)
    {
        projectManager.UpdatedAt = DateTime.UtcNow;
        _db.Entry(projectManager).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ProjectManager projectManager)
    {
        _db.ProjectManagers.Remove(projectManager);
        await _db.SaveChangesAsync();
    }
}