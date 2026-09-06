# ADR-0010: Conformité Loi 25 mise en suspens, avec minimisation appliquée dès maintenant

## Statut
Accepté (mise en suspens explicite, pas un oubli)

## Contexte
`Worker` stocke des renseignements personnels au sens de la Loi 25 du
Québec (nom, potentiellement contact). Aucune configuration du modèle ne
peut exempter le projet de la loi tant que des personnes physiques
restent identifiables — même un identifiant apparemment anonyme reste
couvert si quelqu'un dans l'organisation peut le relier à une personne
réelle.

## Décision
Ne pas implémenter les mécanismes de conformité complets (consentement
tracé, politique de rétention automatisée, journal d'accès, registre
d'incidents, restrictions d'exposition par rôle) dans l'immédiat. Ce
chantier est documenté et reporté, pas oublié.

Appliquer dès maintenant le principe de **minimisation des données**, à
coût nul sur la fonctionnalité : `Worker` ne contient que `FirstName`,
`LastName`, `Phone` (optionnel), `Active` — sans date de naissance ni
adresse (voir ADR-0006).

## Conséquences
- Le projet reste, en l'état, non conforme à la Loi 25 pour une mise en
  production réelle impliquant des travailleurs identifiés — risque
  connu et documenté
- Avant toute mise en production : désignation d'un responsable de la
  protection des renseignements personnels, évaluation des facteurs
  relatifs à la vie privée, validation par un conseiller juridique
  qualifié
- Si un futur système externe de pointage est hébergé hors Québec, une
  évaluation des facteurs relatifs à la vie privée est obligatoire avant
  toute intégration

## Avertissement
Ce résumé est une traduction technique raisonnable des obligations
générales de la loi, pas un avis juridique.
