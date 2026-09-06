# ADR-0013: Project, ProjectTask, unités de facturation et types de taux en types énumérés

## Statut
Accepté

## Contexte
Quatre champs restaient en `string` libre malgré un ensemble de valeurs
relativement fermé, avec le même risque de typo silencieuse déjà
signalé pour `ResourceCategory` (ADR-0009) :

- `Project.status`
- `ProjectTask.status`
- `ResourceType.default_billing_unit` et `ResourceAssignment.billing_unit`
- `ExecutionRecord.rate_type`

Contrainte importante posée dès la conception : ces listes de valeurs
ne sont **pas figées définitivement** — de nouvelles unités de
facturation ou de nouveaux types de taux pourront être ajoutés au fil
du temps, sans qu'on veuille pour autant renoncer à la sécurité de
typage d'un enum.

## Décision
Définir quatre `enum` C# :

```csharp
// ChronoKraft.Domain/Enums/ProjectStatus.cs
namespace ChronoKraft.Domain.Enums;

public enum ProjectStatus { Planning, InProgress, Completed, Cancelled }
```

```csharp
// ChronoKraft.Domain/Enums/ProjectTaskStatus.cs
namespace ChronoKraft.Domain.Enums;

public enum ProjectTaskStatus { Pending, InProgress, Completed, Blocked }
```

```csharp
// ChronoKraft.Domain/Enums/BillingUnit.cs
namespace ChronoKraft.Domain.Enums;

public enum BillingUnit { Hour, Day, Week, Unit, Kilometer, Kg }
```

```csharp
// ChronoKraft.Domain/Enums/RateType.cs
namespace ChronoKraft.Domain.Enums;

public enum RateType { Standard, Overtime, Holiday, Emergency }
```

**`ProjectStatus` et `ProjectTaskStatus` restent deux enums distincts**
plutôt qu'un seul partagé : les cycles de vie diffèrent réellement — un
projet n'est jamais `Blocked` (état propre à une tâche en attente d'une
dépendance ou d'une ressource), et `Cancelled` s'applique au projet
entier, pas à une tâche isolée.

**`BillingUnit` est partagé** entre `ResourceType.default_billing_unit`
et `ResourceAssignment.billing_unit` — les deux champs représentent le
même concept, une unité de facturation, pas deux concepts différents.

### Stockage en texte, pas en entier — pour permettre l'extension sans migration

Pour répondre directement à la contrainte d'évolutivité, ces quatre
enums sont mappés en colonnes texte plutôt qu'en entier par défaut :

```csharp
// ChronoKraft.Infrastructure/ChronoKraftDbContext.cs — dans OnModelCreating
modelBuilder.Entity<Project>()
    .Property(p => p.Status)
    .HasConversion<string>();

modelBuilder.Entity<ProjectTask>()
    .Property(t => t.Status)
    .HasConversion<string>();

modelBuilder.Entity<ResourceType>()
    .Property(rt => rt.DefaultBillingUnit)
    .HasConversion<string>();

modelBuilder.Entity<ResourceAssignment>()
    .Property(ra => ra.BillingUnit)
    .HasConversion<string>();

modelBuilder.Entity<ExecutionRecord>()
    .Property(er => er.RateType)
    .HasConversion<string>();
```

**Pourquoi ça compte concrètement :** un enum mappé par défaut en entier
(`0, 1, 2...`) lie l'ordre des valeurs à leur position dans le code —
insérer une nouvelle valeur au milieu décale tout ce qui suit et corrompt
silencieusement les données existantes. Mappé en texte (`"Hour"`,
`"Overtime"`), ajouter une nouvelle valeur (ex. `SquareMeter` à
`BillingUnit`) ne nécessite qu'une modification du code C# et un
redéploiement — **aucune migration de base de données**, aucun risque
sur les lignes déjà enregistrées.

## Conséquences
- Les quatre champs deviennent vérifiés à la compilation, plus des
  chaînes libres
- **Portée étendue rétroactivement** : cette décision de stockage en
  texte s'applique aussi aux enums déjà établis avant cet ADR
  (`ResourceCategory` — ADR-0009, `UserRole` — ADR-0012, `ReviewStatus`
  et `InvoiceType`, définis sans préciser le mode de stockage à
  l'origine). Le `DbContext` documenté dans `v0.1-implementation-guide.md`
  applique `HasConversion<string>()` de façon uniforme à tous les enums
  du système, pas seulement aux quatre introduits ici.
- Ajouter une valeur future (nouvelle unité de facturation, nouveau
  type de taux) reste un changement de code mineur, pas une migration
- Les valeurs initiales proposées (`BillingUnit`, `RateType`) sont des
  points de départ raisonnables pour un contexte construction, **pas
  une liste validée avec un expert métier** — à ajuster dès qu'un
  besoin réel diverge de ces suppositions
- `data-model.mermaid` documente ces valeurs en commentaire, sur le
  modèle déjà établi pour `ResourceCategory` (ADR-0009), sans que
  PostgreSQL ne contraigne la colonne elle-même — la validation reste
  applicative

## Alternatives considérées
- Mapping en entier (comportement par défaut d'EF Core pour les enums) :
  rejeté explicitement à cause de la contrainte d'évolutivité — le
  risque de décalage silencieux des valeurs existantes lors d'un ajout
  futur est jugé inacceptable
- Un seul enum `Status` partagé entre `Project` et `ProjectTask` :
  rejeté — les valeurs valides ne se recouvrent pas complètement
- Garder ces quatre champs en `string` libre indéfiniment : rejeté —
  même raisonnement que l'ADR-0009, le risque de typo silencieuse est
  jugé plus coûteux que la rigidité relative d'un enum
