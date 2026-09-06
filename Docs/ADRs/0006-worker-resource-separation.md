# ADR-0006: Séparer Worker (identité individuelle) de Resource (pool fongible)

## Statut
Accepté

## Contexte
`Resource`/`ResourceType` modélisent un pool fongible (ex. "3 soudeurs
disponibles chez Béton Laurentides à 45$/h"), adapté à l'équipement (un
camion est interchangeable avec un autre du même type) mais insuffisant
pour la main-d'œuvre nominative, qui nécessite de savoir *qui
précisément* a travaillé — traçabilité, certifications individuelles,
responsabilité.

## Décision
Introduire `Worker` comme entité distincte représentant une personne
physique (une seule fiche par personne, liée à un `Contractor`).
`Resource` reste utilisé pour l'estimation (`ResourceAssignment`, au
moment de la planification, où l'identité précise n'est pas encore
nécessaire). `Worker` intervient au moment de l'exécution réelle et du
pointage.

Champs de `Worker` volontairement minimisés dès la conception :
`FirstName`, `LastName`, `Phone` (optionnel), `Active` — sans date de
naissance ni adresse, aucun usage concret n'ayant été identifié pour ces
champs (voir ADR-0010 sur la protection des renseignements personnels).

## Conséquences
- Le modèle de données compte 15 entités dès la v0.1 — ce n'est pas la
  version la plus minimale possible, c'est un compromis assumé pour
  couvrir dès le départ un besoin métier confirmé (traçabilité
  individuelle de la main-d'œuvre)
- `Resource` et `Worker` coexistent délibérément — aucune tentative de
  les fusionner en un modèle unique

## Alternatives considérées
- Étendre `Resource` avec des champs d'identité optionnels : rejeté, car
  ça mélangerait deux concepts (pool fongible vs identité individuelle)
  dans un seul modèle, rendant les requêtes et validations ambiguës
