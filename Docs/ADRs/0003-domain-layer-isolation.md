# ADR-0003: Couche Domaine isolée pour respecter SOLID et permettre le TDD

## Statut
Accepté

## Contexte
Le calcul de planification Gantt (dépendances entre tâches selon
plusieurs types — Finish-to-Start, Start-to-Start, etc. — avec
pourcentage de complétion et détection de cycles) est la logique métier
la plus critique et la plus risquée du projet. Coupler cette logique
directement à Entity Framework Core rendrait :
- l'ajout d'un nouveau type de dépendance nécessairement invasif
  (violation du principe ouvert/fermé)
- les tests de cette logique dépendants d'une base de données réelle,
  ralentissant considérablement le TDD sur la partie qui en a le plus
  besoin (violation du principe d'inversion de dépendance)

## Décision
Isoler toute logique de calcul pure dans `ChronoKraft.Domain`, sans
aucune dépendance à EF Core ni ASP.NET (garanti par la structure de
projets de l'ADR-0001). Utiliser le **Strategy Pattern** pour les types
de dépendances Gantt :

```csharp
public interface IDependencyStrategy
{
    DateOnly CalculateStart(TaskWindow predecessor, int lagDays, int successorDuration);
}

public record TaskWindow(int TaskId, DateOnly Start, int DurationDays)
{
    public DateOnly End => Start.AddDays(DurationDays);
}

public class FinishToStartStrategy : IDependencyStrategy
{
    public DateOnly CalculateStart(TaskWindow predecessor, int lagDays, int successorDuration)
        => predecessor.End.AddDays(lagDays);
}
```

Les services de `ChronoKraft.Application` (ex. `GanttService`,
`TimesheetValidationService`) restent de simples adaptateurs : ils
lisent/écrivent via les repositories, puis délèguent tout calcul à
`ChronoKraft.Domain`.

Le même principe s'applique à la validation des feuilles de temps
(comparaison entre total externe et ventilation déclarée) et à la
validation des attributs de ressource par catégorie.

## Conséquences
- Les tests sur la logique Gantt et la validation des feuilles de temps
  s'exécutent en millisecondes via xUnit, sans base de données
- Ajouter un nouveau type de dépendance = ajouter une nouvelle classe
  implémentant `IDependencyStrategy`, sans modifier le code existant
- Introduit un aller-retour supplémentaire (service → domaine → service)
  pour chaque opération de calcul — compromis accepté pour la
  testabilité et l'extensibilité
