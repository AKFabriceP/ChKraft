using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ChronoKraft.Infrastructure.Identity;

namespace ChronoKraft.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        var result = await _signInManager.PasswordSignInAsync(Email, Password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Connection succeeded for {Email}", Email);
            return LocalRedirect(returnUrl ?? "/");
        }

        _logger.LogWarning("Connection failed for {Email}", Email);
        ErrorMessage = "Incorrect username or password.";
        return Page();
    }
}