using ChronoKraft.Domain.Entities;

namespace ChronoKraft.Application.Interfaces;

public interface IProjectTaskRepository
{
    Task<IReadOnlyList<ProjectTask>> GetAllAsync();
    Task<IReadOnlyList<ProjectTask>> GetByManagerAsync(int managerId);
    Task<IReadOnlyList<ProjectTask>> GetByProjectAsync(int projectId);
    Task<ProjectTask?> GetByIdAsync(int id);
    Task AddAsync(ProjectTask task);
    Task UpdateAsync(ProjectTask task);
    Task DeleteAsync(ProjectTask task);
}