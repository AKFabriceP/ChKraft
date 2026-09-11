# ADR-0017: Serilog comme moteur de journalisation, dès la v0.1

## Statut
Accepté

## Contexte
Aucune stratégie de journalisation n'était définie jusqu'ici — seule
une trace console minimale existait (ex. la commande `create-admin`,
Étape 10). Le besoin de logging a été identifié comme devant être
introduit **au plus tôt**, dès la v0.1, plutôt que différé à la v0.9
(Release Candidate) comme c'était implicitement le cas via la mention
vague "monitoring de base" en v1.0 (`version-plan.md`).

## Décision
Utiliser **Serilog** comme moteur de journalisation dès la v0.1,
plutôt que le logging intégré d'ASP.NET Core seul.

```bash
dotnet add ChronoKraft.Web package Serilog.AspNetCore
dotnet add ChronoKraft.Web package Serilog.Sinks.Console
dotnet add ChronoKraft.Web package Serilog.Sinks.File
```

**Serilog ne remplace pas `ILogger<T>`** — il vient s'y brancher
derrière. Le code applicatif (`ChronoKraft.Application`,
`ChronoKraft.Infrastructure`) continue d'injecter et d'utiliser
`ILogger<T>` normalement ; seule la configuration dans
`ChronoKraft.Web/Program.cs` détermine que Serilog traite ces logs.
Aucun code applicatif ne référence directement les types Serilog.

### Règle confirmée : aucun logging dans ChronoKraft.Domain
Même `Microsoft.Extensions.Logging.Abstractions` (l'abstraction seule,
sans Serilog) n'est **pas référencé** dans `ChronoKraft.Domain` — la
journalisation est une opération d'I/O, incompatible avec la règle déjà
établie (ADR-0003) qu'aucun package externe n'entre dans ce projet. Le
Domain reste testable en xUnit pur, sans effet de bord d'aucune sorte,
y compris le logging.

### Initialisation en deux temps
Serilog nécessite un "bootstrap logger" configuré **avant**
`WebApplication.CreateBuilder`, pour capturer les erreurs qui
surviendraient pendant la configuration elle-même (ex. un problème de
connexion à PostgreSQL au démarrage) :

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));
```

## Conséquences
- Deux systèmes de configuration coexistent dans `appsettings.json` :
  la section `Logging` standard d'ASP.NET Core (utilisée pour les
  logs du framework lui-même avant l'initialisation complète de
  Serilog) et une section `Serilog` distincte (niveaux, sinks) — une
  source de confusion possible si mal documentée, d'où l'exemple
  complet dans `v0.1-implementation-guide.md`
- Prépare directement le terrain pour le "monitoring de base" déjà
  prévu en v1.0 (`version-plan.md`) — Serilog supporte nativement des
  sinks vers des destinations externes (Seq, Elasticsearch, etc.), à
  ajouter plus tard sans reconfiguration majeure
- Poids ajouté dès la v0.1 (trois packages, configuration en deux
  temps) pour un bénéfice qui ne se matérialise pleinement qu'en fin
  de projet — compromis assumé pour éviter une migration future

## Alternatives considérées
- `ILogger<T>` seul en v0.1, migration vers Serilog plus tard (en v0.9
  ou v1.0) : plus simple immédiatement, mais reporte le travail de
  configuration et risque d'oublier des points d'instrumentation
  ajoutés entre-temps sans logging cohérent — écartée au profit d'une
  mise en place unique, dès maintenant
