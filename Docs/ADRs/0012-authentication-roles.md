# ADR-0012: Table User liée aux profils métier via ASP.NET Core Identity

## Statut
Accepté

## Contexte
ChronoKraft distingue déjà quatre types d'acteurs métier : gestionnaire
de projet (`ProjectManager`), entrepreneur (`Contractor`), travailleur
(`Worker`), et un rôle purement administratif sans entité métier propre
(`Admin`). Jusqu'ici, aucune de ces entités n'était liée à un mécanisme
d'authentification — l'accès au système n'était pas encore modélisé.

## Décision
Utiliser **ASP.NET Core Identity** comme fondation d'authentification,
avec une classe `ApplicationUser` étendant `IdentityUser<int>` dans
`ChronoKraft.Infrastructure` :

```csharp
// ChronoKraft.Domain/Enums/UserRole.cs — le rôle est un concept métier,
// vit dans le Domain même si son porteur technique (ApplicationUser)
// vit dans l'Infrastructure
namespace ChronoKraft.Domain.Enums;

public enum UserRole { Admin, Manager, Contractor, Worker }
```

```csharp
// ChronoKraft.Infrastructure/Identity/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public required UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

Le lien avec les entités métier existantes se fait **depuis le profil
vers l'utilisateur** (FK optionnelle sur `ProjectManager`, `Contractor`
et `Worker`), pas l'inverse — `ChronoKraft.Domain` ne référence jamais
`ApplicationUser` (qui vit dans l'Infrastructure), seulement un entier
`UserId` brut :

```csharp
public class ProjectManager
{
    public int Id { get; set; }
    public int? UserId { get; set; }   // ← lien optionnel, pas de navigation property
    // ... reste inchangé
}
```

Même ajout sur `Contractor` et `Worker`. `Admin` n'a **aucun profil
métier associé** — c'est un `ApplicationUser` avec `Role = Admin` et
aucune ligne correspondante dans `ProjectManager`/`Contractor`/`Worker`.

Les rôles ASP.NET Core Identity (`AspNetRoles`) sont peuplés au
démarrage (seed) avec les quatre valeurs de `UserRole`, pour permettre
l'autorisation standard `[Authorize(Roles = "Admin")]` en parallèle du
champ `Role` dénormalisé sur `ApplicationUser` (pratique pour la logique
métier sans dépendre du système de claims).

## Conséquences
- Ajoute les tables standards Identity (`AspNetUsers`, `AspNetRoles`,
  `AspNetUserRoles`, `AspNetUserClaims`, etc.) au schéma — la base passe
  d'environ 15 tables métier à ~22 tables au total
- `ChronoKraft.Domain` reste pur : il connaît le concept de rôle
  (`UserRole`) mais jamais la classe `ApplicationUser` elle-même
- Aucune contrainte de base de données n'empêche un `ApplicationUser`
  de rôle `Contractor` d'être lié par erreur à un `Worker.UserId` — cette
  cohérence (rôle ↔ profil correspondant) reste une validation
  applicative, dans le même esprit que l'ADR-0009 (validation croisée
  catégorie/métier de ressource, également non garantie par le schéma)
- **Simplification assumée** : `Contractor` représente toujours
  l'entreprise avec un seul contact principal (`ContactName`,
  `ContactEmail`) — lier `Contractor.UserId` suppose qu'une seule
  personne se connecte au nom de l'entreprise. Si plusieurs employés
  d'un même entrepreneur doivent un jour se connecter individuellement,
  ce modèle devra évoluer (probablement via une entité de liaison
  many-to-many, sur le même principe que `WorkerSkill` pour ADR-0007)
- `Worker.UserId` est ajouté même si aucun scénario actuel n'exploite
  une connexion travailleur — anticipe un futur libre-service (ex.
  consultation de sa propre feuille de temps), non implémenté à ce jour

## Alternatives considérées
- Rôle unique stocké uniquement comme claim Identity, sans champ `Role`
  dénormalisé : rejeté, complique inutilement les vérifications de rôle
  dans la logique métier courante
- FK obligatoire (`UserId` non-nullable) sur les trois profils : rejeté
  — un `Worker` ou `Contractor` peut exister dans le système (ex. importé
  via Excel, voir v0.4) avant même qu'un compte de connexion lui soit
  créé
