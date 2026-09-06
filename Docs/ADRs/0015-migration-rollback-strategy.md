# ADR-0015: Rollback en production via script de migration généré

## Statut
Différé — candidat pour implémentation et tests **avant** la v1.0
(Production), pas strictement bloqué jusque-là. Voir `version-plan.md`
pour la note correspondante.

## Contexte
EF Core Migrations génère automatiquement, pour chaque migration, une
méthode `Up()` (application) et une méthode `Down()` (annulation). Deux
approches existent pour appliquer des migrations en production :

1. **Automatique au démarrage** (`db.Database.Migrate()` dans
   `Program.cs`) — l'application applique les migrations en attente à
   chaque démarrage, sans étape manuelle
2. **Script généré, exécuté séparément**
   (`dotnet ef migrations script`) — un fichier `.sql` est produit,
   revu, puis exécuté explicitement, hors du cycle de démarrage de
   l'application

Cette même mécanique `Up()`/`Down()` permet aussi un **rollback** —
revenir à un schéma antérieur en cas de problème après un déploiement.

## Décision
Retenir l'**approche par script généré**, pour l'application des
migrations comme pour leur annulation :

```bash
# Génération du script d'application (une version vers une autre)
dotnet ef migrations script <MigrationPrecedente> <MigrationCible> \
  --project ChronoKraft.Infrastructure --startup-project ChronoKraft.Web \
  -o migrate.sql

# Génération du script de rollback (ordre inverse)
dotnet ef migrations script <MigrationCible> <MigrationPrecedente> \
  --project ChronoKraft.Infrastructure --startup-project ChronoKraft.Web \
  -o rollback.sql
```

Le script est **revu avant exécution**, puis appliqué explicitement
(manuellement ou via une étape dédiée de la CI/CD) — jamais déclenché
automatiquement au démarrage de l'application.

## Limitations connues, à valider pendant la phase de test

- **Perte de données irréversible** sur les colonnes/tables supprimées
  par un rollback — `Down()` fait un `DropColumn`/`DropTable` pur, sans
  préserver les données insérées entre-temps
- **Coordination obligatoire avec le déploiement applicatif** — un
  rollback de schéma sans rollback simultané du code casse
  l'application (le code référence des colonnes/entités qui n'existent
  plus)
- **Fiabilité variable du `Down()` auto-généré** pour des migrations
  complexes (transformations de données, renommages avec conversion) —
  à vérifier manuellement au cas par cas, pas à supposer correct par
  défaut

## Conséquences
- Aucun code ni script n'existe encore dans le projet à ce jour —
  seule la stratégie est arrêtée
- Avant la v1.0, prévoir un test concret : appliquer une migration,
  générer son script de rollback, l'exécuter, et vérifier l'état de la
  base et de l'application après coup — en particulier sur une
  migration ayant supprimé une colonne contenant des données
- La procédure de coordination (quel ordre entre rollback de BD et
  rollback de déploiement applicatif) reste à documenter précisément
  une fois testée, pas seulement énoncée en principe

## Alternatives considérées
- Migration automatique au démarrage (`db.Database.Migrate()`) :
  écartée — risque de conflit si plusieurs instances de l'application
  démarrent simultanément et tentent la migration en même temps, et
  aucune fenêtre de revue avant application en production
