# ADR-0004: Livraison incrémentale par versions utilisables (v0.1 → v1.0)

## Statut
Accepté

## Contexte
Un découpage par couches techniques complètes (tous les modèles, puis
toute l'API, puis toute l'interface) retarde la première démonstration
utilisable et concentre le risque en fin de projet.

## Décision
Découper le développement en versions numérotées, où **chaque version
produit un système démontrable et utilisable**, avec un scénario de
démonstration concret et des critères de sortie explicites :

```
v0.1 Fondations → v0.2 API Alpha → v0.3 Moteur Gantt → v0.4 Pont Excel →
v0.5 Web Beta → v0.6 Facturation → v0.7 Gantt Visuel → v0.8 Bilingue →
v0.9 Release Candidate → v1.0 Production
```

Le moteur Gantt (v0.3) est positionné volontairement tôt, avant
l'interface web complète, pour valider la logique la plus risquée avant
d'y construire une UI.

## Conséquences
- Chaque version doit être accompagnée d'un scénario de démonstration
  concret et d'une checklist de sortie (definition of done)
- Le périmètre d'une version peut être révisé rétroactivement si une
  décision produit l'exige (voir ADR-0006, où v0.1 a été élargie pour
  intégrer la gestion de la main-d'œuvre)
- Le séquencement n'a pas encore été validé avec de vraies parties
  prenantes métier — reste un point à confirmer

## Référence
Voir `PLAN_LIVRAISON_DOTNET.md` pour le détail complet des versions.
