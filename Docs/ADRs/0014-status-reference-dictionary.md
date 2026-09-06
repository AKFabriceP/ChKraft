# ADR-0014: Table `StatusReference` — dictionnaire autonome des valeurs d'enum

## Statut
Différé — candidat pour une version future (ex. v0.6 Facturation, où
le multiplicateur de `RateType` devient réellement utile, ou v0.9
Release Candidate, une fois le besoin réel de dictionnaire consultable
confirmé plutôt qu'anticipé). Conçu et documenté en détail ci-dessous
pour éviter d'avoir à refaire cette analyse si le besoin se confirme
plus tard — mais **non implémenté** dans les fichiers actuels du
projet (`data-model.mermaid`, `data-dictionary.md`,
`v0.1-implementation-guide.md` ne contiennent aucune trace de
`StatusReference`).

## Contexte
Le système compte désormais huit enums (`ProjectStatus`,
`ProjectTaskStatus`, `ResourceCategory`, `UserRole`, `ReviewStatus`,
`InvoiceType`, `BillingUnit`, `RateType` — voir ADR-0009, ADR-0012,
ADR-0013), chacun stocké en texte sur son entité respective. Un besoin
distinct apparaît : disposer d'un dictionnaire consultable de toutes
ces valeurs, notamment pour associer une donnée numérique à certaines
d'entre elles (ex. un multiplicateur de taux par défaut pour chaque
`RateType`), sans coder ces valeurs en dur dans l'application.

## Décision
Introduire une table de référence autonome, **sans clé étrangère entrante
ni sortante** — un pur dictionnaire, jamais utilisée pour contraindre ou
valider les colonnes enum elles-mêmes (qui restent typées et vérifiées à
la compilation, sans changement à l'ADR-0013).

```csharp
// ChronoKraft.Domain/Entities/StatusReference.cs
namespace ChronoKraft.Domain.Entities;

public class StatusReference
{
    public int StatusId { get; set; }
    public required string StatusType { get; set; }   // ex. "RateType", "ProjectStatus"
    public required string StatusLabel { get; set; }  // ex. "Overtime", "Blocked"
    public decimal? Value { get; set; }                // ex. 1.5 pour Overtime
}
```

**Nommée `StatusReference`, pas `Status`** — pour éviter la confusion
avec les nombreux champs déjà nommés `status` ailleurs dans le schéma
(`Project.status`, `ProjectTask.status`, `TimesheetReview.status`),
même principe de clarté que le renommage `Task` → `ProjectTask`
(ADR-0013 / `data-dictionary.md`).

### Synchronisation automatique par seed, pas par duplication manuelle

Sans contrainte FK, rien ne garantit nativement que ce dictionnaire
reste aligné avec les enums réels. Pour éliminer ce risque, les lignes
sont générées **directement à partir des enums C# par réflexion**, au
démarrage :

```csharp
// ChronoKraft.Infrastructure/Identity/StatusReferenceSeeder.cs
using ChronoKraft.Domain.Entities;
using ChronoKraft.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChronoKraft.Infrastructure;

public static class StatusReferenceSeeder
{
    public static async Task SeedAsync(ChronoKraftDbContext db)
    {
        var entries = new List<StatusReference>();

        void AddEnum<TEnum>() where TEnum : struct, Enum
        {
            foreach (var value in Enum.GetValues<TEnum>())
            {
                entries.Add(new StatusReference
                {
                    StatusType = typeof(TEnum).Name,
                    StatusLabel = value.ToString()
                });
            }
        }

        AddEnum<ProjectStatus>();
        AddEnum<ProjectTaskStatus>();
        AddEnum<ResourceCategory>();
        AddEnum<UserRole>();
        AddEnum<ReviewStatus>();
        AddEnum<InvoiceType>();
        AddEnum<BillingUnit>();

        // RateType géré séparément : c'est le seul enum où `Value`
        // porte une donnée métier réelle (multiplicateur de taux)
        entries.Add(new StatusReference { StatusType = nameof(RateType), StatusLabel = nameof(RateType.Standard), Value = 1.0m });
        entries.Add(new StatusReference { StatusType = nameof(RateType), StatusLabel = nameof(RateType.Overtime), Value = 1.5m });
        entries.Add(new StatusReference { StatusType = nameof(RateType), StatusLabel = nameof(RateType.Holiday), Value = 2.0m });
        entries.Add(new StatusReference { StatusType = nameof(RateType), StatusLabel = nameof(RateType.Emergency), Value = 2.5m });

        foreach (var entry in entries)
        {
            bool exists = await db.StatusReferences.AnyAsync(s =>
                s.StatusType == entry.StatusType && s.StatusLabel == entry.StatusLabel);
            if (!exists) db.StatusReferences.Add(entry);
        }

        await db.SaveChangesAsync();
    }
}
```

**Point important :** ce seeder régénère les libellés (`StatusLabel`)
automatiquement à partir de `Enum.GetValues<T>()` — impossible qu'un
libellé diverge du nom réel de la valeur d'enum. Seule la colonne
`Value` (les multiplicateurs de `RateType`) reste saisie à la main,
puisqu'un enum C# ne porte pas nativement de donnée numérique associée.

Contrainte d'unicité `(status_type, status_label)` pour éviter les
doublons :

```csharp
modelBuilder.Entity<StatusReference>()
    .HasIndex(s => new { s.StatusType, s.StatusLabel })
    .IsUnique();
```

## Conséquences
- Aucun impact sur les colonnes enum existantes — elles restent
  typées, vérifiées à la compilation, stockées en texte (ADR-0013)
- `StatusReference` n'apparaît dans aucune relation du schéma ER — une
  table volontairement isolée
- Les valeurs de `Value` pour `RateType` (1.0 / 1.5 / 2.0 / 2.5) sont
  des suppositions initiales, pas des règles métier validées — à
  ajuster dès qu'un besoin réel diverge
- Si un nouveau membre est ajouté à un enum (ex. `BillingUnit.SquareMeter`),
  le seeder l'ajoute automatiquement au prochain démarrage — aucune
  étape manuelle à retenir
- Cette table ne remplace ni ne concurrence les colonnes existantes :
  c'est un dictionnaire de consultation, jamais une source de vérité
  structurelle

## Alternatives considérées
- Clé étrangère depuis les colonnes enum vers `StatusReference.StatusId` :
  rejeté explicitement par le porteur de projet — retirerait la
  sécurité de typage du compilateur au profit d'un entier arbitraire
  vérifié seulement à l'exécution, dans le sens inverse de ce que
  l'ADR-0013 cherche à garantir
- Dupliquer manuellement les libellés dans la table plutôt que de les
  générer par réflexion : rejeté — introduit un risque de désynchronisation
  silencieuse à chaque ajout futur d'une valeur d'enum
