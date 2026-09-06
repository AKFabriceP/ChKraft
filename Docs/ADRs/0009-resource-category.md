# ADR-0009: ResourceType.ResourceCategory contraint par un type énuméré

## Statut
Accepté

## Contexte
`ResourceType` doit distinguer main-d'œuvre, équipement et matériel. Un
champ texte libre permet des incohérences silencieuses (fautes de
frappe, casse incohérente) qui cassent les filtres applicatifs sans
erreur visible à la compilation.

## Décision
Contraindre `ResourceCategory` avec un `enum` C#, converti en texte
lisible en base par EF Core :

```csharp
public enum ResourceCategory { Labor, Equipment, Material }
```

## Conséquences
- Élimine le risque de typo — une valeur invalide ne compile pas
- Rend le code auto-documenté
- N'empêche pas à lui seul une incohérence entre `ResourceCategory` et
  `Trade` (ex. catégorie `Equipment` avec `Trade="Welding"`) — cette
  validation croisée reste à implémenter dans `ChronoKraft.Domain`
  (ADR-0003), pas encore faite en v0.1

## Alternatives considérées
- Validation uniquement côté formulaire, sans contrainte sur l'entité :
  rejeté, moins robuste face aux insertions directes (import Excel,
  scripts de seed)
