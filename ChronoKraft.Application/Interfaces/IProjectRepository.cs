using ChronoKraft.Domain.Entities;

namespace ChronoKraft.Application.Interfaces;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> GetAllAsync();
    Task<IReadOnlyList<Project>> GetByManagerAsync(int managerId);
    Task<Project?> GetByIdAsync(int id);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
}