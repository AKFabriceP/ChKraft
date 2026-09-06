# ADR-0008: Réconciliation des feuilles de temps — acceptée mais flaguée, jamais bloquante

## Statut
Accepté

## Contexte
Un système externe de pointage (badge/API tierce) fournit un total
d'heures par travailleur/jour, mais ne sait jamais sous quel rôle
(compétence) ces heures ont été effectuées — un badge identifie *qui*,
jamais *sous quelle casquette*. Le travailleur déclare lui-même la
ventilation par rôle via une feuille de temps. Ces deux sources peuvent
diverger.

## Décision
Modéliser deux sources de données strictement séparées :
- `ExternalTimeTotal` — total d'heures poussé par le système externe,
  source de vérité pour le total
- `TimesheetEntry` — ventilation par rôle déclarée par le travailleur,
  liée à un `WorkerSkill` précis

Le système ne détermine **jamais** automatiquement le rôle. Il compare
uniquement les deux totaux. En cas d'écart : la donnée est **acceptée
telle quelle** (jamais bloquante pour la paie ou la facturation) et
**marquée pour révision humaine** via `TimesheetReview` (statuts :
`Pending`, `Resolved`, `Ignored`).

La comparaison est extraite en fonction pure dans `ChronoKraft.Domain`,
testable sans base de données (voir ADR-0003).

## Conséquences
- Aucune interruption de flux métier en cas de divergence
- Nécessite une révision humaine périodique de la file
  `TimesheetReview`, sans quoi les écarts s'accumulent sans être traités

## Points volontairement laissés ouverts
- Granularité de comparaison (jour vs période de paie)
- Mécanisme exact d'ingestion de l'API externe (endpoint,
  authentification, fréquence) — information insuffisante à ce jour

## Alternatives considérées
- Bloquer la facturation tant que l'écart n'est pas résolu : rejeté,
  jugé trop rigide pour un usage terrain où les écarts mineurs sont
  courants
