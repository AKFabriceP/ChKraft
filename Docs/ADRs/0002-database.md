# ADR-0002: PostgreSQL comme base de données

## Statut
Accepté

## Contexte
ChronoKraft nécessite le stockage de données structurées (projets,
tâches, ressources, main-d'œuvre) combiné à des attributs flexibles et
évolutifs sans migration lourde (métadonnées de projet, attributs de
ressource, certifications de travailleur). Ces champs doivent pouvoir
évoluer sans réécrire le schéma à chaque nouvel attribut métier.

## Décision
Utiliser **PostgreSQL** comme base de données, avec **Npgsql** comme
pilote Entity Framework Core (`Npgsql.EntityFrameworkCore.PostgreSQL`).
Les champs à attributs dynamiques utilisent le type natif **`jsonb`**,
mappé directement sur des propriétés `Dictionary<string, object>` :

```csharp
modelBuilder.Entity<Contractor>()
    .Property(c => c.Metadata)
    .HasColumnType("jsonb");
```

## Conséquences
- Aucune sérialisation manuelle requise pour les champs JSON — Npgsql
  gère la conversion nativement
- Développement local nécessite une instance PostgreSQL réelle (pas de
  base embarquée légère type LocalDB) — installation identique quel que
  soit le poste (Windows/Linux/macOS)
- Les colonnes `jsonb` ne sont validées par aucune contrainte de schéma
  — la validation reste une responsabilité applicative, portée par la
  couche Domaine (voir ADR-0003)

## Alternatives considérées
- SQL Server : plus proche de l'écosystème .NET par défaut (outillage
  Microsoft intégré), mais écarté ici car PostgreSQL offre un support
  JSON plus direct et reste pertinent si l'hébergement n'est pas
  exclusivement Azure/Windows. À reconsidérer si l'environnement cible
  impose SQL Server spécifiquement.
