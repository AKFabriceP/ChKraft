# ADR-0007: WorkerSkill comme relation many-to-many pour compétences multiples

## Statut
Accepté

## Contexte
Un `Worker` peut exercer plusieurs corps de métier (ex. une même
personne travaillant tantôt comme soudeur, tantôt comme mécanicien),
chacun avec son propre taux horaire et ses propres certifications. Une
simple référence de `Worker` vers `ResourceType` obligerait à dupliquer
la fiche d'identité pour chaque compétence — aucune façon fiable de
savoir qu'il s'agit de la même personne physique.

## Décision
Introduire `WorkerSkill` comme entité de liaison many-to-many entre
`Worker` et `ResourceType`, avec ses propres attributs : `HourlyRate`,
`Certifications` (jsonb), `IsPrimary`. Contrainte d'unicité sur
`(WorkerId, ResourceTypeId)` pour empêcher un doublon de compétence pour
la même personne :

```csharp
modelBuilder.Entity<WorkerSkill>()
    .HasIndex(ws => new { ws.WorkerId, ws.ResourceTypeId })
    .IsUnique();
```

## Conséquences
- Toute exécution ou entrée de feuille de temps référence un
  `WorkerSkill` précis, pas seulement un `Worker`, pour connaître le
  rôle et le taux applicables
- Complexifie légèrement les requêtes (jointure supplémentaire) en
  échange d'une identité non dupliquée et d'une intégrité garantie par
  la base de données

## Alternatives considérées
- Champ de rôle libre sur l'enregistrement d'exécution, sans lien
  structurel à `ResourceType` : rejeté, aucune garantie de cohérence
  entre le rôle déclaré et le catalogue de compétences réel
