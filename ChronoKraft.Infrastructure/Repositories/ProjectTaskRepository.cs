using Microsoft.EntityFrameworkCore;
using ChronoKraft.Application.Interfaces;
using ChronoKraft.Domain.Entities;

namespace ChronoKraft.Infrastructure.Repositories;

public class ProjectTaskRepository : IProjectTaskRepository
{
    private readonly CKDbContext _db;

    public ProjectTaskRepository(CKDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProjectTask>> GetAllAsync()
        => await _db.ProjectTasks.ToListAsync();

    public async Task<IReadOnlyList<ProjectTask>> GetByManagerAsync(int managerId)
        => await _db.ProjectTasks.Where(t => t.Project!.ManagerId == managerId).ToListAsync();

    public async Task<IReadOnlyList<ProjectTask>> GetByProjectAsync(int projectId)
        => await _db.ProjectTasks.Where(t => t.ProjectId == projectId).ToListAsync();

    public async Task<ProjectTask?> GetByIdAsync(int id)
        => await _db.ProjectTasks.FindAsync(id);

    public async Task AddAsync(ProjectTask task)
    {
        task.CreatedAt = DateTime.UtcNow;
        _db.ProjectTasks.Add(task);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProjectTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _db.Entry(task).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ProjectTask task)
    {
        _db.ProjectTasks.Remove(task);
        await _db.SaveChangesAsync();
    }
}