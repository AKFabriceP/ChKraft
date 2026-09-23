using ChronoKraft.Domain.Entities;
using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Application.Interfaces;

public interface IProjectManagerAccessResolver
{
    Task<ProjectManagerAccessLevel> GetAccessLevelAsync(int requestingPmId, int targetProjectId);
    Task<ProjectManager?> GetProjectManagerForUserAsync(int userId);
    Task<bool> HasSufficientAccessAsync(int requestingPmId, int targetProjectId, ProjectManagerAccessLevel required);
}