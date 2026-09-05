# ChronoKraft

Système de gestion de projet avec planification Gantt avancée,
destiné à un contexte de construction/minier/entrepreneuriat.

## Ce que le système fait

- Gestion de projets, tâches, entrepreneurs et ressources
- Planification automatique de type Gantt, avec dépendances entre
  tâches (y compris des dépendances basées sur un pourcentage de
  complétion)
- Double saisie de données : interface web et fichiers Excel
  (import/export)
- Suivi d'exécution quotidienne et génération automatique de factures
- Réconciliation des feuilles de temps entre un système de pointage
  externe et la déclaration du travailleur, avec file de révision
  humaine non bloquante
- Authentification par rôles (Admin, Manager, Contractor, Worker)
- Support bilingue français/anglais

## Pile technique

ASP.NET Core (.NET 10), Entity Framework Core, PostgreSQL, ASP.NET Core
Identity. Architecture en Clean Architecture — voir `architecture.md`.

## Structure de ce dossier

```
chronokraft/
├── README.md                       	 — ce fichier
├── Docs/architecture.md                 — architecture complète, diagrammes inclus
├── Docs/architecture.html               — version visuelle autonome (ouvrir dans un navigateur)
├── Docs/version-plan.md                 — plan de livraison, v0.1 à v1.0
├── Docs/data-model.mermaid              — schéma de données source (15 entités + Identity)
└── Docs/ADRs/                           — Architecture Decision Records (source de vérité pour toute décision)
```

## Par où commencer

1. Lire `architecture.md` pour une vue d'ensemble du système
2. Consulter `Docs/ADRs/README.md` pour comprendre le raisonnement derrière chaque décision
3. Se référer à `version-plan.md` pour la suite

## Statut actuel

Version 0.1 en cours d'implémentation. Voir `version-plan.md` pour le
détail des versions suivantes et `Docs/ADRs/README.md` pour l'ensemble des
décisions prises à ce jour.

## Points en Suspens

Voir la section correspondante dans `version-plan.md` — notamment la
conformité aux lois sur la protection des renseignements personnels
(ADR-0010), volontairement différée et documentée plutôt qu'ignorée.
