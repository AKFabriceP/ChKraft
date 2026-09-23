using Microsoft.EntityFrameworkCore;
using ChronoKraft.Application.Interfaces;
using ChronoKraft.Domain.Entities;

namespace ChronoKraft.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly CKDbContext _db;

    public ProjectRepository(CKDbContext db) => _db = db;

    public async Task<IReadOnlyList<Project>> GetAllAsync()
        => await _db.Projects.ToListAsync();

    public async Task<IReadOnlyList<Project>> GetByManagerAsync(int managerId)
        => await _db.Projects.Where(p => p.ManagerId == managerId).ToListAsync();

    public async Task<Project?> GetByIdAsync(int id)
        => await _db.Projects.FindAsync(id);

    public async Task AddAsync(Project project)
    {
        project.CreatedAt = DateTime.UtcNow;
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _db.Entry(project).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
    }
}