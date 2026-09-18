using ChronoKraft.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChronoKraft.Web.Pages.Account;
public class SetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public SetPasswordModel(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    [BindProperty] public string NewPassword { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        var result = await _userManager.ResetPasswordAsync(user, token, NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        return RedirectToPage("/Account/Login");
    }
}