using ChronoKraft.Application.Interfaces;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Infrastructure.Authorization;

public class ProjectManagerAccessResolver
{
    private readonly IProjectManagerRepository _projectManagerRepository;
    private readonly CKDbContext _db;

    public ProjectManagerAccessResolver(
        CKDbContext db,
        IProjectManagerRepository projectManagerRepository)
    {
        _db = db;
        _projectManagerRepository = projectManagerRepository;
    }

    public async Task<ProjectManager?> GetProjectManagerForUserAsync(int userId)
        => await _projectManagerRepository.GetByUserIdAsync(userId);

    public async Task<ProjectManagerAccessLevel> GetAccessLevelAsync(int requestingPmId, int targetProjectId)
    {
        var project = await _db.Projects.FindAsync(targetProjectId);
        if (project == null) return ProjectManagerAccessLevel.NONE;

        if (project.ManagerId == requestingPmId)
            return ProjectManagerAccessLevel.WRITE;

        var requestingPm = await _projectManagerRepository.GetByIdAsync(requestingPmId);
        return requestingPm?.CrossProjectAccessLevel ?? ProjectManagerAccessLevel.NONE;
    }
}