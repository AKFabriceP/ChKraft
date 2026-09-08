# Architecture Decision Records — ChronoKraft

Ce dossier documente les décisions d'architecture du projet, une par
fichier, numérotées séquentiellement. Une fois **acceptée**, une
décision n'est jamais modifiée en place — si elle est renversée, un
nouvel ADR la remplace et référence l'ancien comme "Remplacé par
ADR-00XX".

**Statuts possibles :**

| Statut | Sens |
|---|---|
| `Accepté` | En vigueur, implémenté dans les fichiers du projet |
| `Remplacé par ADR-00XX` | Une décision différente l'a remplacée |
| `Différé` | Analysée et conçue en détail, mais **non implémentée** — candidate explicite pour une version future, pas un rejet définitif ni un abandon |

## Index

| # | Titre | Statut |
|---|---|---|
| [0001](0001-technology-stack.md) | ASP.NET Core / C# avec Clean Architecture comme pile technique | Accepté |
| [0002](0002-database.md) | PostgreSQL comme base de données | Accepté |
| [0003](0003-domain-layer-isolation.md) | Couche Domaine isolée pour SOLID et TDD | Accepté |
| [0004](0004-versioning-strategy.md) | Livraison incrémentale par versions utilisables (v0.1 → v1.0) | Accepté |
| [0005](0005-data-model.md) | Le schéma de données est la source de vérité | Accepté |
| [0006](0006-worker-resource-separation.md) | Séparer Worker (identité) de Resource (pool fongible) | Accepté |
| [0007](0007-worker-skill.md) | WorkerSkill pour compétences multiples | Accepté |
| [0008](0008-timesheet-reconciliation.md) | Réconciliation des feuilles de temps non bloquante | Accepté |
| [0009](0009-resource-category.md) | ResourceType.ResourceCategory en type énuméré | Accepté |
| [0010](0010-privacy-compliance.md) | Conformité vie privée mise en suspens, avec minimisation | Accepté |
| [0011](0011-documentation-approach.md) | Documentation "docs as code" avec ADRs | Accepté |
| [0012](0012-authentication-roles.md) | Authentification et rôles via ASP.NET Core Identity | Accepté |
| [0013](0013-project-status-enums.md) | Statuts de Project et ProjectTask en types énumérés | Accepté |
| [0014](0014-status-reference-dictionary.md) | Table `StatusReference` — dictionnaire autonome des valeurs d'enum | Différé |
| [0015](0015-migration-rollback-strategy.md) | Rollback en production via script de migration généré | Différé |
| [0016](0016-auth0-hierarchical-rbac.md) | Auth0 et RBAC hiérarchique par portée métier | En étude |

## Comment ajouter un ADR

1. Copier le format des ADRs existants (Statut / Contexte / Décision /
   Conséquences / Alternatives considérées le cas échéant)
2. Numéroter séquentiellement (`0013-titre-court.md`)
3. Ajouter une ligne dans le tableau ci-dessus
4. Ne jamais éditer un ADR déjà accepté — créer un nouvel ADR qui le
   remplace si la décision change
