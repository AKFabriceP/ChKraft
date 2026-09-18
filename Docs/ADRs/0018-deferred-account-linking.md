# ADR-0018: Liaison User/Profil Métier par Réinitialisation Différée

## Statut
Accepté

## Contexte
Un profil métier (`ProjectManager`, `Contractor`, `Worker`) créé par un
`Admin` doit ensuite être associé à un compte de connexion, sans que
l'`Admin` connaisse jamais un mot de passe fonctionnel de la personne —
contrainte de sécurité posée explicitement plutôt qu'un compromis
accepté par défaut. Trois approches ont été comparées :

1. L'`Admin` choisit et transmet un mot de passe temporaire, changé
   plus tard par l'utilisateur
2. Un jeton d'invitation envoyé par courriel, l'utilisateur crée lui-même
   son profil
3. L'`Admin` crée le compte **sans mot de passe**, un jeton de
   réinitialisation est généré et transmis manuellement, l'utilisateur
   définit son propre mot de passe à la première connexion

L'option 2 a été écartée : elle nécessite un système d'envoi de
courriels, absent du projet à ce jour, et duplique l'interface de
création de profil déjà scaffoldée. L'option 3 est retenue.

## Décision

### Création sans mot de passe

```csharp
// ChronoKraft.Infrastructure/Identity/UserAccountService.cs — méthode ajoutée
public async Task<(IdentityResult Result, string? ResetToken, ApplicationUser? User)> CreateAccountWithResetLinkAsync(
    string email, UserRole role)
{
    var user = new ApplicationUser { UserName = email, Email = email, Role = role };

    // Surcharge SANS mot de passe — CreateAsync(user) plutôt que
    // CreateAsync(user, password). PasswordHash reste NULL.
    var result = await _userManager.CreateAsync(user);
    if (!result.Succeeded)
    {
        _logger.LogWarning("Échec de création de compte (sans mot de passe) pour {Email} : {Errors}",
            email, string.Join("; ", result.Errors.Select(e => e.Description)));
        return (result, null, null);
    }

    await _userManager.AddToRoleAsync(user, role.ToString());
    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

    _logger.LogInformation("Compte créé sans mot de passe pour {Email} (rôle {Role}), jeton généré", email, role);
    return (result, token, user);
}
```

### Consommation du jeton — Razor Page `/Account/SetPassword`

```csharp
// ChronoKraft.Web/Pages/Account/SetPassword.cshtml.cs
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
```

### Liaison SSO — code écrit, non activé par défaut

Le retrait du mot de passe après liaison SSO (discuté séparément) est
**volontairement laissé en commentaire**, pas exécuté automatiquement.
Raison : les comptes de test créés avec des emails factices
(`@chronokraft-test.local`) ne peuvent **jamais** se lier à un vrai
fournisseur SSO (bloqué par tout fournisseur réel — voir plus bas) —
retirer leur mot de passe les rendrait définitivement inaccessibles,
sans filet de secours.

```csharp
public async Task LinkExternalLoginAsync(ApplicationUser user, ExternalLoginInfo loginInfo)
{
    await _userManager.AddLoginAsync(user, loginInfo);
    _logger.LogInformation("Connexion externe liée pour {Email} ({Provider})", user.Email, loginInfo.LoginProvider);

    // Volontairement en commentaire — voir ADR-0018. À activer
    // explicitement seulement pour de vrais comptes de production,
    // jamais pour des comptes de test à email factice.
    // await _userManager.RemovePasswordAsync(user);
    // _logger.LogInformation("Mot de passe local retiré pour {Email} après liaison SSO", user.Email);
}
```

## Conséquences
- L'`Admin` ne connaît jamais de mot de passe fonctionnel — même pas
  temporairement, contrairement aux approches 1 et à la proposition
  initiale (mot de passe temporaire + réinitialisation forcée)
- Le lien de réinitialisation doit être transmis manuellement
  (affiché à l'écran de l'`Admin`) tant qu'aucun envoi de courriel
  automatisé n'existe — limitation assumée, pas cachée
- Un compte créé sans mot de passe et jamais activé reste indéfiniment
  en attente — aucune expiration du jeton n'est gérée explicitement au-delà
  du comportement par défaut d'Identity (généralement quelques heures,
  configurable via `DataProtectionTokenProviderOptions`)
- Le retrait du mot de passe après liaison SSO reste du code mort tant
  qu'il n'est pas explicitement décommenté — décision consciente de le
  garder inactif par défaut, pas un oubli

## Alternatives considérées
- Mot de passe temporaire choisi par l'Admin (proposition initiale) :
  rejeté — fenêtre de risque où un mot de passe fonctionnel est connu
  par quelqu'un d'autre que l'utilisateur final, même temporairement
- Jeton d'invitation par courriel (option 2) : rejeté pour cette
  version, faute d'infrastructure d'envoi de courriels ; pourrait être
  reconsidéré si cette infrastructure est ajoutée plus tard
