using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Infrastructure.Authorization;

public class ProjectManagerAccessChecker
{
    private readonly CKDbContext _db;

    public ProjectManagerAccessChecker(CKDbContext db) => _db = db;

    public async Task<ProjectManagerAccessLevel> GetAccessLevelAsync(int requestingPmId, int targetProjectId)
    {
        var project = await _db.Projects.FindAsync(targetProjectId);
        if (project == null) return ProjectManagerAccessLevel.NONE;

        // Automatic full access to own projects
        if (project.ManagerId == requestingPmId) return ProjectManagerAccessLevel.WRITE;

        var requestingPm = await _db.ProjectManagers.FindAsync(requestingPmId);
        return requestingPm?.CrossProjectAccessLevel ?? ProjectManagerAccessLevel.NONE;
    }
}