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

## Pipeline complet — où chaque commande s'exécute

Un point souvent mal compris : **générer** une migration et
**l'appliquer** en production sont deux activités qui ne se produisent
jamais au même endroit ni au même moment.

| Étape | Commande | Environnement | Nécessite le SDK .NET / outils EF ? |
|---|---|---|---|
| **1. Développement** — un modèle C# change | `dotnet ef migrations add <Nom>` | Poste du développeur | Oui, jamais en production |
| **2. Build / CI** — préparation du déploiement | `dotnet ef migrations script <Précédente> <Cible> -o migrate.sql` | Agent CI/CD (GitHub Actions, etc.) | Oui, sur l'agent de build |
| **3. Déploiement** — application réelle | Exécution de `migrate.sql` | Serveur / environnement cible | **Non** — un simple client SQL (`psql`) suffit |

Le fichier `.cs` généré à l'étape 1 est **commité dans le dépôt git**
comme n'importe quel autre fichier de code — il ne "voyage" jamais
séparément vers la production.

**Conséquence directe pour l'environnement de production :** contrairement
à l'alternative écartée (`db.Database.Migrate()` au démarrage, qui
exigerait qu'EF Core soit pleinement fonctionnel dans l'application
déployée elle-même), l'approche par script ne demande **aucune**
capacité EF Core côté production — seulement un accès à la base de
données et un client SQL basique.

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
