# ADR-0001: ASP.NET Core / C# avec Clean Architecture comme pile technique

## Statut
Accepté

## Contexte
ChronoKraft est un système de gestion de projet avec planification
Gantt, double saisie web/Excel, suivi d'exécution et facturation,
destiné à s'intégrer à un environnement organisationnel standardisé sur
l'écosystème .NET (C#, Entity Framework, IIS/Azure).

## Décision
Utiliser **ASP.NET Core (.NET 10 LTS) / C#** comme pile applicative,
structurée en **Clean Architecture** : une solution découpée en projets
distincts avec des références unidirectionnelles imposées par le
compilateur.

```
ChronoKraft.Domain          — logique métier pure, aucune dépendance externe
ChronoKraft.Application     — services, orchestration, interfaces de repository
ChronoKraft.Infrastructure  — EF Core, accès base de données, Identity, Excel
ChronoKraft.Web             — Razor Pages, API Controllers
ChronoKraft.Tests           — xUnit
```

## Conséquences
- La séparation entre logique de calcul pure et logique d'accès aux
  données (voir ADR-0003) est vérifiable à la compilation : une
  dépendance interdite entre projets provoque une erreur de build, pas
  seulement une revue de code manquée
- Nécessite le SDK .NET 10 sur chaque poste de développement
- Alignement direct avec un environnement organisationnel .NET existant
