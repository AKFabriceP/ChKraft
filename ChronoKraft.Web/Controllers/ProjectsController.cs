using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChronoKraft.Application.Services;
using ChronoKraft.Domain.Entities;

namespace ChronoKraft.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN, MANAGER")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _service;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(ProjectService service, ILogger<ProjectsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private int? CurrentUserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    private bool IsAdmin => User.IsInRole("ADMIN");

    private IActionResult ToActionResult<T>(ServiceResult<T> result) => result.Outcome switch
    {
        AccessOutcome.Success => Ok(result.Value),
        AccessOutcome.NotFound => NotFound(),
        _ => Forbid()
    };

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (CurrentUserId is not int userId) return Forbid();
        return Ok(await _service.GetProjectsForUserAsync(userId, IsAdmin));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (CurrentUserId is not int userId) return Forbid();
        return ToActionResult(await _service.GetProjectAsync(id, userId, IsAdmin));
    }

    [HttpGet("{id:int}/tasks")]
    public async Task<IActionResult> GetTasks(int id)
    {
        if (CurrentUserId is not int userId) return Forbid();
        return ToActionResult(await _service.GetTasksForProjectAsync(id, userId, IsAdmin));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Project project)
    {
        var created = await _service.CreateProjectAsync(project);
        _logger.LogInformation("Project created : {ProjectId} — {Name}", created.Id, created.Name);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Project project)
    {
        if (id != project.Id) return BadRequest();
        if (CurrentUserId is not int userId) return Forbid();

        var result = await _service.UpdateProjectAsync(project, userId, IsAdmin);
        if (result.Outcome == AccessOutcome.Success)
            _logger.LogInformation("Project updated : {ProjectId}", id);
        return result.Outcome == AccessOutcome.Success ? NoContent() : ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (CurrentUserId is not int userId) return Forbid();

        var result = await _service.DeleteProjectAsync(id, userId, IsAdmin);
        if (result.Outcome == AccessOutcome.Success)
            _logger.LogInformation("Project deleted : {ProjectId}", id);
        return result.Outcome == AccessOutcome.Success ? NoContent() : ToActionResult(result);
    }
}