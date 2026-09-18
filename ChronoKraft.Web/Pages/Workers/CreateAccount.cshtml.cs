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

namespace ChronoKraft.Web.Pages.Workers;

[Authorize(Roles = "ADMIN, CONTRACTOR")]
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
    public string? ResetLink { get; set; }
    public string? ErrorMessage { get; set; }
    
    public async Task<IActionResult> OnGetAsync (int workerId)
    {
        var worker = await _dbContext.Workers.FindAsync(workerId);
        return worker == null? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int workerId)
    {
        var worker = await _dbContext.Workers.FindAsync(workerId);
        if (worker == null) return NotFound();

        (IdentityResult result, string? token, ApplicationUser? user) = await _userAccountService.CreateAccountWithResetLinkAsync(Email, UserRole.WORKER);
        if (!result.Succeeded || user == null || token == null)
        {
            ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
            return Page();
        }

        await _userAccountService.LinkWorkerAsync(worker, user.Id);

        // Affiché à l'écran pour transmission manuelle — voir ADR-0018,
        // aucun envoi de courriel automatisé à ce stade
        ResetLink = Url.Page("/Account/SetPassword", null,
            new { userId = user.Id, token }, Request.Scheme);

        return Page();
    }
}