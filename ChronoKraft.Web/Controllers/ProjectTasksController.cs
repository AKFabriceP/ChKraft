using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChronoKraft.Application.Services;
using ChronoKraft.Domain.Entities;
using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN,MANAGER,CONTRACTOR,WORKER")]
public class ProjectTasksController : ControllerBase
{
    private readonly ProjectService _service;
    private readonly ILogger<ProjectTasksController> _logger;

    public ProjectTasksController(ProjectService service, ILogger<ProjectTasksController> logger)
    {
        _service = service;
        _logger = logger;
    }

    private int? CurrentUserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    private UserRole? CurrentRole =>
        Enum.TryParse<UserRole>(User.FindFirstValue(ClaimTypes.Role), out var role) ? role : null;

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
        if (CurrentRole is not UserRole role) return Forbid();
        return Ok(await _service.GetTasksForUserAsync(userId, role));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (CurrentUserId is not int userId) return Forbid();
        if (CurrentRole is not UserRole role) return Forbid();
        return ToActionResult(await _service.GetTaskAsync(id, userId, role));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProjectTask task)
    {
        if (CurrentUserId is not int userId || CurrentRole is not UserRole role) return Forbid();
        var result = await _service.CreateTaskAsync(task, userId, role);
        if (result.Outcome != AccessOutcome.Success) return ToActionResult(result);
        _logger.LogInformation("Tâche créée : {TaskId} — {Name}", result.Value!.Id, result.Value.Name);
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProjectTask task)
    {
        if (id != task.Id) return BadRequest();
        if (CurrentUserId is not int userId || CurrentRole is not UserRole role) return Forbid();

        var result = await _service.UpdateTaskAsync(task, userId, role);
        if (result.Outcome == AccessOutcome.Success)
            _logger.LogInformation("Tâche mise à jour : {TaskId}", id);
        return result.Outcome == AccessOutcome.Success ? NoContent() : ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (CurrentUserId is not int userId || CurrentRole is not UserRole role) return Forbid();

        var result = await _service.DeleteTaskAsync(id, userId, role);
        if (result.Outcome == AccessOutcome.Success)
            _logger.LogInformation("Tâche supprimée : {TaskId}", id);
        return result.Outcome == AccessOutcome.Success ? NoContent() : ToActionResult(result);
    }
}