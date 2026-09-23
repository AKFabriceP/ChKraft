using ChronoKraft.Domain.Entities;

namespace ChronoKraft.Application.Interfaces;

public interface IProjectManagerRepository
{
    Task<IReadOnlyList<ProjectManager>> GetAllAsync();
    Task<ProjectManager?> GetByIdAsync(int id);
    Task<ProjectManager?> GetByUserIdAsync(int userId);
    Task AddAsync(ProjectManager projectManager);
    Task UpdateAsync(ProjectManager projectManager);
    Task DeleteAsync(ProjectManager projectManager);
}