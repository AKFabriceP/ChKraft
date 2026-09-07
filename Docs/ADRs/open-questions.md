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

Une autre question: est ce que le travailleur peut avoir 2 feuille de temps dans la meme journee (atin et apres midi apres la pause)?

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

## Comment ajouter une question

1. Numéroter séquentiellement (`Q2`, `Q3`, ...)
2. Statut initial : `En attente de réponse`
3. Une fois répondue : marquer `Répondu — voir ADR-00XX`, ne jamais
   supprimer l'entrée (garde la trace de la question posée et de la
   réponse obtenue)
