# ChronoKraft — Questions Ouvertes (à valider avec le client)

Ce document liste des **questions métier** qui doivent être répondues
par le client avant qu'une décision technique puisse être étudiée et,
le cas échéant, formalisée en ADR.

**Différence avec les autres documents de suivi :**

| Document | Contenu |
|---|---|
| `adr/` | Décisions déjà **prises** (acceptées, différées, ou remplacées) |
| `version-plan.md` (Points en Suspens) | Détails **techniques** internes non tranchés (granularité, bibliothèque à choisir, etc.) |
| **`open-questions.md`** (ce document) | Questions **métier** nécessitant une réponse du client avant même de pouvoir formuler une décision technique |

Une entrée passe de ce document vers un ADR une fois répondue, jamais
avant — répondre ici change la nature de la question (de "qu'est-ce que
le client veut ?" à "comment l'implémenter ?").

---

## Q1 — TimesheetReview : plusieurs révisions par travailleur/jour ?

**Statut :** En attente de réponse

### Contexte
`TimesheetReview` (voir ADR-0008) est créée automatiquement en cas
d'écart entre le total externe (`ExternalTimeTotal`) et la somme
déclarée (`TimesheetEntry`) pour un travailleur, à une date donnée.
Actuellement, aucune contrainte d'unicité n'existe sur
`(worker_id, work_date)`.

### Question précise à poser au client
Si les données sont corrigées après une première divergence détectée
(ex. le travailleur ajoute une entrée de feuille de temps oubliée), et
qu'un **nouvel** écart apparaît ensuite à la **même date** — doit-on :

- **(A)** Créer une nouvelle ligne `TimesheetReview` distincte pour
  cette nouvelle divergence (historique complet de chaque écart
  détecté) ?
- **(B)** Mettre à jour la ligne existante (une seule révision "active"
  par travailleur/jour, écrasant l'écart précédent) ?

### Impact technique de chaque réponse
| Réponse | Impact |
|---|---|
| **(A)** | Aucune contrainte d'unicité sur `(worker_id, work_date)` — confirmerait l'état actuel du schéma tel quel |
| **(B)** | Nécessiterait une contrainte unique `(worker_id, work_date)`, et une logique de mise à jour plutôt que de création dans `TimesheetValidationService` |

### Pourquoi ce n'est pas tranché ici
Le choix a une implication directe sur la capacité d'audit du système
(pouvoir retracer chaque écart historique vs ne garder que l'état
courant) — une décision qui dépend de contraintes de conformité ou de
processus internes du client, pas d'une préférence technique.

---

## Q2 — Worker : portée exacte de l'accès restreint au projet/contractor

**Statut :** En attente de réponse

### Contexte
Dans la hiérarchie de rôles proposée (ADR-0016, en étude), un `Worker`
a accès à ses propres tâches et feuilles de temps, plus "certaines
informations" du projet et de son entreprise entrepreneur — sans que
cette portée soit précisée.

### Question précise à poser au client
Concrètement, un `Worker` devrait-il pouvoir voir :
- Le nom et l'avancement général du projet sur lequel il travaille ?
- La liste des autres tâches du même projet (pas seulement les
  siennes) ?
- Des informations financières du projet (budget, factures) ?
- Des informations sur les autres travailleurs de son entreprise
  entrepreneur (ex. qui d'autre est assigné au même projet) ?

### Impact technique de chaque réponse
Détermine directement la conception des `IAuthorizationHandler` pour le
rôle `Worker` (ADR-0016, point 1) — une portée très restreinte
(uniquement ses propres données) est nettement plus simple à
implémenter et à auditer qu'une portée élargie avec des exceptions
partielles (ex. voir le nom du projet mais pas son budget).

### Pourquoi ce n'est pas tranché ici
Question de confidentialité et de politique interne du client vis-à-vis
de ses travailleurs, pas une préférence technique.

---

## Q3 — ProjectManager : visibilité croisée entre gestionnaires

**Statut :** Répondu — voir ADR-0019

### Contexte
Dans la hiérarchie proposée (ADR-0016), un `ProjectManager` a accès à
"tout ce qui concerne ses propres projets" — mais rien n'indique s'il
peut consulter les projets gérés par **d'autres** `ProjectManager`.

### Question précise à poser au client
Un `ProjectManager` peut-il voir les projets d'un autre
`ProjectManager`, **même en lecture seule** (ex. pour une vue
d'ensemble organisationnelle, un remplacement temporaire, ou une
coordination inter-projets) ? Ou l'isolation doit-elle être stricte —
chaque `ProjectManager` ne voit que ses propres projets, sans
exception ?

### Impact technique de chaque réponse
| Réponse | Impact |
|---|---|
| **Isolation stricte** | Le filtre par `manager_id` suffit partout — la vérification d'autorisation reste simple (comparer l'utilisateur au propriétaire) |
| **Lecture croisée autorisée** | Nécessite une distinction entre droits de **lecture** et d'**écriture** dans chaque `IAuthorizationHandler` (un PM peut lire un projet qui n'est pas le sien, mais pas le modifier) — complexifie chaque vérification d'accès, pas seulement celles liées aux projets |

### Pourquoi ce n'est pas tranché ici
Dépend de la structure organisationnelle réelle du client (gestionnaires
indépendants vs équipe collaborative avec visibilité partagée) — pas
une évidence technique à décider ici.

### Réponse retenue
Accès **global**, pas par projet ni par paire — un champ
`CrossProjectAccessLevel` directement sur `ProjectManager`
(`None`/`Read`/`Write`), géré par l'`Admin`. Si accordé, s'applique à
tous les projets de tous les autres PM, y compris les futurs. Voir
ADR-0019 pour le détail complet (troisième et dernière itération de ce
design).

---

## Q4 — Traçabilité de l'attribution du rôle Contractor par un ProjectManager

**Statut :** En attente de réponse

### Contexte
Il est confirmé qu'il n'existe aucun lien métier direct entre `Project`
et `Contractor` (voir ADR-0016, point 2, corrigé). Quand un
`ProjectManager` attribue le rôle `Contractor` à un utilisateur, c'est
une action RBAC pure — elle ne crée aucune relation vers un projet
précis dans le schéma de données.

### Question précise à poser au client
Quand un `ProjectManager` crée/promeut un compte `Contractor` :

- **(A)** Est-ce une action globale, sans besoin de traçabilité
  particulière — n'importe quel PM peut le faire, l'entreprise devient
  ensuite disponible pour tout projet via `ResourceAssignment`, comme
  aujourd'hui ?
- **(B)** Faut-il conserver une trace de **qui** a créé ce compte (à
  des fins d'audit ou de responsabilité), même sans relation métier
  formelle vers un projet ?

### Impact technique de chaque réponse
| Réponse | Impact |
|---|---|
| **(A)** | Aucun changement nécessaire au schéma actuel |
| **(B)** | Ajout d'un champ simple (ex. `CreatedByUserId`) sur le compte utilisateur — pas une nouvelle relation `Project`↔`Contractor`, juste une trace d'audit |

### Pourquoi ce n'est pas tranché ici
Dépend des besoins d'audit et de gouvernance interne du client, pas
d'une contrainte technique.

---

## Comment ajouter une question

1. Numéroter séquentiellement (`Q2`, `Q3`, ...)
2. Statut initial : `En attente de réponse`
3. Une fois répondue : marquer `Répondu — voir ADR-00XX`, ne jamais
   supprimer l'entrée (garde la trace de la question posée et de la
   réponse obtenue)
