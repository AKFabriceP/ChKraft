# ADR-0005: Le schéma ER documenté est la source de vérité du modèle de données

## Statut
Accepté

## Contexte
Le modèle de données de ChronoKraft compte 15 entités couvrant la
gestion de projet (Project, Task), les ressources matérielles
(ResourceType, Resource), la main-d'œuvre nominative (Worker,
WorkerSkill), le pointage (ExternalTimeTotal, TimesheetEntry,
TimesheetReview) et la facturation (Invoice, InvoiceLine). Ce modèle
doit rester la référence unique pour toute implémentation, discussion ou
document dérivé.

## Décision
Le fichier `data-model.mermaid` fait foi pour la structure des 15
entités et leurs relations. Toute documentation narrative (guides,
contexte de conversation) qui diverge de ce fichier doit être corrigée
en faveur du diagramme, pas l'inverse.

Entités couvertes : `ProjectManager`, `Contractor`, `Worker`,
`ResourceType`, `WorkerSkill`, `Resource`, `Project`, `Task`,
`ResourceAssignment`, `TimesheetEntry`, `ExecutionRecord`,
`ExternalTimeTotal`, `TimesheetReview`, `Invoice`, `InvoiceLine`.

`TaskDependency` (dépendances entre tâches, nécessaire au moteur Gantt)
n'existe pas encore dans ce schéma — son introduction est prévue à la
v0.3 (voir ADR-0004), au moment où la logique qui la nécessite est
réellement implémentée, pas avant.

## Conséquences
- Toute question sur "quel champ existe sur quelle entité" se résout en
  consultant le diagramme, pas en se fiant à la mémoire d'une
  conversation
- Le schéma de la v0.1 n'inclut aucune dépendance entre tâches.
