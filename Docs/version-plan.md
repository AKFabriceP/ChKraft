# ChronoKraft — Plan de Livraison Incrémentale

Chaque version produit un système démontrable et utilisable, avec un
scénario de démonstration concret et des critères de sortie explicites.
Voir ADR-0004 pour le raisonnement de cette approche.

```
v0.1 Fondations → v0.2 API → v0.3 Moteur Gantt → v0.4 Pont Excel →
v0.5 Web Beta → v0.6 Facturation → v0.7 Gantt Visuel → v0.8 Bilingue →
v0.9 Release Candidate → v1.0 Production
```

Durée totale estimée : 18-22 semaines.

---

## v0.1 — Fondations

**Objectif :** base de données fonctionnelle et administrable, avec
authentification par rôles.

- Modèle de données complet (15 entités métier + Identity)
- CRUD via Razor Pages scaffoldées
- Authentification ASP.NET Core Identity, 4 rôles (Admin, Manager,
  Contractor, Worker)

**Scénario de démonstration :** un scénario complet (gestionnaire →
projet → tâche → ressource → exécution → facture, plus un travailleur
avec deux compétences et une réconciliation de feuille de temps) saisi
entièrement via l'interface, sans passer par une console ou un script.

**Guide complet :** `v0.1-implementation-guide.md`

---

## v0.2 — API

**Objectif :** exposer les données via une API REST.

- ASP.NET Core Web API (Controllers ou Minimal API)
- Documentation OpenAPI auto-générée (Swashbuckle)
- Authentification par JWT Bearer, en complément de l'authentification par cookie des Razor Pages

**Scénario de démonstration :** création complète d'un projet via
Postman/Insomnia, sans passer par les Razor Pages.

---

## v0.3 — Moteur Gantt

**Objectif :** calcul automatique des plannings — le cœur de valeur du
produit.

- `IDependencyStrategy` (Strategy Pattern) pour les types de dépendance
  Finish-to-Start, Start-to-Start, Finish-to-Finish, Start-to-Finish
- Détection des dépendances circulaires
- Calcul du chemin critique
- Tests xUnit purs sur `ChronoKraft.Domain`, sans base de données

**Scénario de démonstration :** un projet de dépendances mixtes
(incluant une dépendance basée sur un pourcentage de complétion) est
planifié automatiquement via un appel API, chemin critique inclus.

**Critère de sortie propre à cette version :** `dotnet list
ChronoKraft.Domain package` ne doit retourner aucun package EF Core ou
ASP.NET, même après l'ajout de toute la logique de calcul Gantt.

---

## v0.4 — Pont Excel

**Objectif :** permettre le travail hors-ligne via Excel.

- Génération de templates (ClosedXML ou EPPlus)
- Import avec validation structurelle et métier, transaction atomique
- Export d'un projet existant

**Scénario de démonstration :** un aller-retour complet
(export → modification → import) ne perd aucune donnée ; un fichier
volontairement invalide échoue proprement avec un message d'erreur
précis par ligne.

---

## v0.5 — Web Beta

**Objectif :** interface complète pour utilisateurs finaux.

- Razor Pages étendues avec logique métier réelle (au-delà du CRUD
  scaffoldé de la v0.1)
- Tableau de bord, filtres, recherche

**Scénario de démonstration :** un gestionnaire de projet réalise un
cycle complet (création → assignation → suivi) sans jamais passer par
une API brute.

---

## v0.6 — Facturation

**Objectif :** fermer la boucle métier — de l'exécution à la facture.

- Génération automatique à partir des `ExecutionRecord`
- Numérotation unique des factures
- Export PDF (QuestPDF) ou HTML imprimable

**Scénario de démonstration :** une facture générée reflète exactement
la somme des exécutions enregistrées sur la période couverte.

---

## v0.7 — Gantt Visuel

**Objectif :** rendre le moteur Gantt (v0.3) visible et interactif.

- Bibliothèque de diagramme Gantt côté client (choix non tranché à ce
  jour — DHTMLX, Bryntum, ou composant Blazor commercial)
- Mise en évidence du chemin critique

**Scénario de démonstration :** un projet de 15+ tâches s'affiche
visuellement, tâches critiques en évidence, correspondant exactement
aux données retournées par le moteur de calcul.

---

## v0.8 — Bilingue

**Objectif :** support complet français/anglais.

- Fichiers `.resx` (`IStringLocalizer`)
- Sélecteur de langue fonctionnel

**Scénario de démonstration :** bascule de langue testée sur l'ensemble
des écrans, sans texte non traduit visible.

---

## v0.9 — Release Candidate

**Objectif :** solidifier avant la mise en production.

- Couverture de tests ≥ 80 % (`dotnet test --collect:"XPlat Code Coverage"`)
- `dotnet format` + analyseurs Roslyn
- Optimisation des requêtes EF Core (élimination des N+1)

---

## v1.0 — Production

**Objectif :** mise en ligne officielle.

- Conteneurisation Docker (image `mcr.microsoft.com/dotnet/aspnet`)
- CI/CD (GitHub Actions ou Azure DevOps Pipelines)
- Hébergement PostgreSQL (auto-hébergé ou Azure Database for PostgreSQL)
- Backups automatiques, monitoring de base
- Stratégie de rollback des migrations (ADR-0015, différé) — voir note ci-dessous

**Note sur le rollback de migration (ADR-0015) :** la stratégie est
arrêtée (script de migration généré, pas de migration automatique au
démarrage), mais **peut être implémentée et testée avant** que le
reste de la v1.0 soit complété — ce n'est pas une dépendance stricte,
juste une bonne occasion de valider la procédure (notamment sur une
migration ayant supprimé une colonne contenant des données) avant que
ça devienne critique en production réelle.

---

## Points en Suspens

- Granularité de comparaison des feuilles de temps (jour vs période de paie)
- Mécanisme d'ingestion de l'API externe de pointage — non défini
- Bibliothèque frontend pour le Gantt visuel (v0.7) — non tranchée
- Conformité vie privée (ADR-0010) — en suspens
- Séquencement des versions — pas encore validé avec de vraies parties prenantes métier
- Test concret de la procédure de rollback de migration (ADR-0015) — à faire avant la v1.0
