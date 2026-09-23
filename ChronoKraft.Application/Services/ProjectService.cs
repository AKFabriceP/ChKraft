using ChronoKraft.Application.Interfaces;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Application.Services;

public class ProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IProjectTaskRepository _tasks;
    private readonly IProjectManagerAccessResolver _access;

    public ProjectService(
        IProjectRepository projects, IProjectTaskRepository tasks, IProjectManagerAccessResolver access)
    {
        _projects = projects;
        _tasks = tasks;
        _access = access;
    }

    public async Task<IReadOnlyList<Project>> GetProjectsForUserAsync(int userId, bool isAdmin)
    {
        if (isAdmin) return await _projects.GetAllAsync();

        var pm = await _access.GetProjectManagerForUserAsync(userId);
        if (pm == null) return Array.Empty<Project>();

        if (pm.CrossProjectAccessLevel != ProjectManagerAccessLevel.NONE)
            return await _projects.GetAllAsync();

        return await _projects.GetByManagerAsync(pm.Id);
    }

    public async Task<ServiceResult<Project>> GetProjectAsync(int projectId, int userId, bool isAdmin)
    {
        var project = await _projects.GetByIdAsync(projectId);
        if (project == null) return ServiceResult<Project>.NotFound();

        if (!await CanAccessAsync(projectId, userId, isAdmin, ProjectManagerAccessLevel.READ))
            return ServiceResult<Project>.Forbidden();

        return ServiceResult<Project>.Success(project);
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        await _projects.AddAsync(project);
        return project;
    }

    public async Task<ServiceResult<Project>> UpdateProjectAsync(Project project, int userId, bool isAdmin)
    {
        if (!await CanAccessAsync(project.Id, userId, isAdmin, ProjectManagerAccessLevel.WRITE))
            return ServiceResult<Project>.Forbidden();

        await _projects.UpdateAsync(project);
        return ServiceResult<Project>.Success(project);
    }

    public async Task<ServiceResult<Project>> DeleteProjectAsync(int projectId, int userId, bool isAdmin)
    {
        var project = await _projects.GetByIdAsync(projectId);
        if (project == null) return ServiceResult<Project>.NotFound();

        if (isAdmin)
        {
            await _projects.DeleteAsync(project);
            return ServiceResult<Project>.Success(project);
        }

        var pm = await _access.GetProjectManagerForUserAsync(userId);
        if (pm == null) return ServiceResult<Project>.Forbidden();

        // Manager can delete only his own projects
        if (project.ManagerId != pm.Id)
            return ServiceResult<Project>.Forbidden();

        await _projects.DeleteAsync(project);
        return ServiceResult<Project>.Success(project);
    }

    // --- Tasks ---

    public async Task<IReadOnlyList<ProjectTask>> GetTasksForUserAsync(int userId, UserRole role)
    {
        if (role == UserRole.ADMIN) return await _tasks.GetAllAsync();

        if (role == UserRole.MANAGER)
        {
            var pm = await _access.GetProjectManagerForUserAsync(userId);
            if (pm == null) return Array.Empty<ProjectTask>();

            if (pm.CrossProjectAccessLevel != ProjectManagerAccessLevel.NONE)
                return await _tasks.GetAllAsync();

            return await _tasks.GetByManagerAsync(pm.Id);
        }

        return role switch
        {
            UserRole.CONTRACTOR => await _tasks.GetByContractorUserAsync(userId),
            UserRole.WORKER => await _tasks.GetByWorkerUserAsync(userId),
            _ => Array.Empty<ProjectTask>()
        };
    }

    public async Task<ServiceResult<IReadOnlyList<ProjectTask>>> GetTasksForProjectAsync(
        int projectId, int userId, bool isAdmin)
    {
        var project = await _projects.GetByIdAsync(projectId);
        if (project == null) return ServiceResult<IReadOnlyList<ProjectTask>>.NotFound();

        if (!await CanAccessAsync(projectId, userId, isAdmin, ProjectManagerAccessLevel.READ))
            return ServiceResult<IReadOnlyList<ProjectTask>>.Forbidden();

        return ServiceResult<IReadOnlyList<ProjectTask>>.Success(await _tasks.GetByProjectAsync(projectId));
    }

    public async Task<ServiceResult<ProjectTask>> GetTaskAsync(int taskId, int userId, UserRole role)
    {
        var task = role switch
        {
            UserRole.ADMIN => await _tasks.GetByIdAsync(taskId),
            UserRole.MANAGER => await _tasks.GetByIdAsync(taskId),
            UserRole.CONTRACTOR => await _tasks.GetByIdForContractorUserAsync(taskId, userId),
            UserRole.WORKER => await _tasks.GetByIdForWorkerUserAsync(taskId, userId),
            _ => null
        };
        if (task == null) return ServiceResult<ProjectTask>.NotFound();

        if (role == UserRole.ADMIN) return ServiceResult<ProjectTask>.Success(task);

        if (role != UserRole.MANAGER ||
            !await CanAccessAsync(task.ProjectId, userId, false, ProjectManagerAccessLevel.READ))
            return ServiceResult<ProjectTask>.Forbidden();

        return ServiceResult<ProjectTask>.Success(task);
    }

    public async Task<ServiceResult<ProjectTask>> CreateTaskAsync(
        ProjectTask task, int userId, UserRole role)
    {
        if (role != UserRole.ADMIN &&
            (role != UserRole.MANAGER ||
             !await CanAccessAsync(task.ProjectId, userId, false, ProjectManagerAccessLevel.WRITE)))
            return ServiceResult<ProjectTask>.Forbidden();

        await _tasks.AddAsync(task);
        return ServiceResult<ProjectTask>.Success(task);
    }

    public async Task<ServiceResult<ProjectTask>> UpdateTaskAsync(
        ProjectTask task, int userId, UserRole role)
    {
        if (role != UserRole.ADMIN &&
            (role != UserRole.MANAGER ||
             !await CanAccessAsync(task.ProjectId, userId, false, ProjectManagerAccessLevel.WRITE)))
            return ServiceResult<ProjectTask>.Forbidden();

        await _tasks.UpdateAsync(task);
        return ServiceResult<ProjectTask>.Success(task);
    }

    public async Task<ServiceResult<ProjectTask>> DeleteTaskAsync(int taskId, int userId, UserRole role)
    {
        if (role != UserRole.ADMIN && role != UserRole.MANAGER)
            return ServiceResult<ProjectTask>.Forbidden();

        var task = await _tasks.GetByIdAsync(taskId);
        if (task == null) return ServiceResult<ProjectTask>.NotFound();

        if (role != UserRole.ADMIN)
        {
            var pm = await _access.GetProjectManagerForUserAsync(userId);
            if (pm == null) return ServiceResult<ProjectTask>.Forbidden();

            var project = await _projects.GetByIdAsync(task.ProjectId);
            if (project == null || project.ManagerId != pm.Id)
                return ServiceResult<ProjectTask>.Forbidden();
        }

        await _tasks.DeleteAsync(task);
        return ServiceResult<ProjectTask>.Success(task);
    }

    private async Task<bool> CanAccessAsync(
        int projectId, int userId, bool isAdmin, ProjectManagerAccessLevel required)
    {
        if (isAdmin) return true;
        var pm = await _access.GetProjectManagerForUserAsync(userId);
        if (pm == null) return false;
        return await _access.HasSufficientAccessAsync(pm.Id, projectId, required);
    }
}