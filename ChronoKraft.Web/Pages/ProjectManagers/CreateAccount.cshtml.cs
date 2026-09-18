using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronoKraft.Domain.Enums;
using ChronoKraft.Infrastructure;
using ChronoKraft.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ChronoKraft.Web.Pages.ProjectManagers;

[Authorize(Roles = "ADMIN")]
public class CreateAccountModel : PageModel
{
    private readonly UserAccountService _userAccountService;
    private readonly CKDbContext _dbContext;

    public CreateAccountModel(UserAccountService userAccountService, CKDbContext dbContext)
    {
        _userAccountService = userAccountService;
        _dbContext = dbContext;
    }

    [BindProperty] public string Email { get; set; } = string.Empty;
    public string? ResetLink { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; } = string.Empty;
    
    public async Task<IActionResult> OnGetAsync (int projectManagerId)
    {
        var projectManager = await _dbContext.ProjectManagers.FindAsync(projectManagerId);
        return projectManager == null? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int projectManagerId)
    {
        var projectManager = await _dbContext.ProjectManagers.FindAsync(projectManagerId);
        if (projectManager == null) return NotFound();

        (IdentityResult result, string? token, ApplicationUser? user) = await _userAccountService.CreateAccountWithResetLinkAsync(Email, UserRole.MANAGER);
        if (!result.Succeeded || user == null || token == null)
        {
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return Page();
        }

        await _userAccountService.LinkProjectManagerAsync(projectManager, user.Id);

        // Affiché à l'écran pour transmission manuelle — voir ADR-0018,
        // aucun envoi de courriel automatisé à ce stade
        ResetLink = Url.Page("/Account/SetPassword", null,
            new { userId = user.Id, token }, Request.Scheme);

        return Page();
    }
}