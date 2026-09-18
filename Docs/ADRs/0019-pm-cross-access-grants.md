# ADR-0019: Accès Croisé Global comme Propriété du ProjectManager (résout Q3)

## Statut
Accepté — répond à `open-questions.md` Q3

## Contexte
Un `ProjectManager` a un accès automatique en lecture/écriture à ses
propres projets (déjà garanti structurellement par `Project.ManagerId`).
Q3 (`open-questions.md`) demandait si un `ProjectManager` peut voir les
projets d'un autre `ProjectManager`, même en lecture seule. Réponse
finale du porteur de projet, après deux itérations : un accès accordé
à un PM s'applique **globalement, à tous les projets de tous les autres
PM** — pas par paire de gestionnaires, pas par projet individuel.

**Note de révision** : cet ADR a été conçu deux fois avant cette
version — d'abord comme permission PM-à-PM (table de liaison par paire
de gestionnaires), puis comme permission par projet individuel
(`ProjectAccessGrant`). Aucune des deux versions n'a jamais été
appliquée à aucune base ni migration — chaque itération corrigeait une
incompréhension du besoin réel, jusqu'à cette version, la plus simple
des trois.

## Décision

Pas de table de liaison du tout — un **champ directement sur
`ProjectManager`** :

```csharp
// ChronoKraft.Domain/Entities/ProjectManager.cs — champ ajouté
public class ProjectManager
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public ProjectManagerAccessLevel CrossProjectAccessLevel { get; set; } = ProjectManagerAccessLevel.None;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
```

```csharp
// ChronoKraft.Domain/Enums/ProjectManagerAccessLevel.cs — inchangé
namespace ChronoKraft.Domain.Enums;

public enum ProjectManagerAccessLevel { None, Read, Write }
```

Si `CrossProjectAccessLevel` vaut `Read` ou `Write` pour un PM donné,
il a cet accès sur **tous** les projets de **tous** les autres PM —
une seule valeur, appliquée globalement, sans distinction de
propriétaire ni de projet individuel.

### Vérification d'accès — considérablement simplifiée

```csharp
// ChronoKraft.Infrastructure/Authorization/ProjectManagerAccessChecker.cs
using Microsoft.EntityFrameworkCore;
using ChronoKraft.Domain.Enums;

namespace ChronoKraft.Infrastructure.Authorization;

public class ProjectManagerAccessChecker
{
    private readonly ChronoKraftDbContext _db;

    public ProjectManagerAccessChecker(ChronoKraftDbContext db) => _db = db;

    public async Task<ProjectManagerAccessLevel> GetAccessLevelAsync(int requestingPmId, int targetProjectId)
    {
        var project = await _db.Projects.FindAsync(targetProjectId);
        if (project == null) return ProjectManagerAccessLevel.None;

        // Accès automatique total sur ses propres projets
        if (project.ManagerId == requestingPmId) return ProjectManagerAccessLevel.Write;

        var requestingPm = await _db.ProjectManagers.FindAsync(requestingPmId);
        return requestingPm?.CrossProjectAccessLevel ?? ProjectManagerAccessLevel.None;
    }
}
```

Plus aucune table à consulter — une seule lecture sur `ProjectManager`
lui-même.

**Point de rattachement à l'ADR-0016** (toujours `En étude`) : ce
vérificateur est le genre de composant qu'un `IAuthorizationHandler`
personnalisé (déjà anticipé dans l'ADR-0016) invoquerait — cet ADR
livre la structure de données et la logique de vérification ; le
branchement complet sur `[Authorize(Policy = "...")]` reste dépendant
de l'issue de l'ADR-0016.

## Conséquences
- **Aucune nouvelle table ni entité** — le schéma reste à 16 entités
  métier + Identity, `ProjectManagerAccessGrant`/`ProjectAccessGrant`
  des versions précédentes de cet ADR n'existent plus
- Gérée manuellement par l'`Admin`, directement sur la fiche du PM
  concerné (ex. un champ modifiable sur la page `Edit` déjà scaffoldée
  de `ProjectManagers`, pas une nouvelle interface à construire)
- **Granularité globale uniquement** : impossible d'accorder un accès à
  certains projets d'un tiers et pas à d'autres, ou à certains PM
  propriétaires et pas d'autres — un PM avec `CrossProjectAccessLevel =
  Read` voit tout, sans exception possible
- Un nouveau projet créé par n'importe quel PM est **automatiquement**
  couvert par les accès croisés déjà accordés — contrairement aux deux
  versions précédentes de cet ADR, aucune action supplémentaire n'est
  nécessaire à la création d'un projet
- `Write` reste inclus dans l'enum par cohérence avec "lecture/écriture"
  mentionné dans la demande initiale, mais n'a pas été explicitement
  confirmé comme réellement souhaité au niveau global — à valider avant
  utilisation en production

## Alternatives considérées
- Permission par projet individuel (`ProjectAccessGrant`, version
  précédente de cet ADR) : rejetée — plus de contrôle que nécessaire,
  coût d'administration jugé disproportionné par rapport au besoin réel
- Permission PM-à-PM par paire (première version de cet ADR) : rejetée
  pour la même raison — le besoin exprimé est en réalité plus simple
  qu'un contrôle par paire
- Une seule règle globale fixe (ex. "tous les PM voient tout") : écartée
  — reste une décision par `Admin`, pas une règle automatique non
  configurable
