# ChronoKraft — Architecture

Système de gestion de projet avec planification Gantt, double saisie
web/Excel, suivi d'exécution et facturation. Construit sur ASP.NET Core
(.NET 10) avec Entity Framework Core et PostgreSQL.

Chaque section renvoie vers les ADRs (`adr/`) qui justifient les choix
présentés — ce document décrit le système tel qu'il est, les ADRs
expliquent pourquoi.

---

## Structure de Solution — Clean Architecture

La séparation entre logique de calcul pure et logique d'accès aux
données est imposée par le compilateur, via des projets .NET distincts
avec des références unidirectionnelles.

```mermaid
graph TB
    subgraph SLN["ChronoKraft.sln"]
        DOMAIN["ChronoKraft.Domain<br/>Entités POCO, Strategy Pattern Gantt,<br/>validation pure — AUCUNE dépendance externe"]
        APPLICATION["ChronoKraft.Application<br/>Services (TaskManagementService, etc.)<br/>Interfaces de repository"]
        INFRA["ChronoKraft.Infrastructure<br/>DbContext EF Core, Identity,<br/>Implémentations de repository"]
        WEB["ChronoKraft.Web<br/>Razor Pages / API Controllers"]
        TESTS["ChronoKraft.Tests<br/>xUnit — Domain testé sans DB"]
    end

    APPLICATION -->|référence| DOMAIN
    INFRA -->|référence| APPLICATION
    INFRA -->|référence| DOMAIN
    WEB -->|référence| APPLICATION
    WEB -->|référence| INFRA
    TESTS -->|référence| DOMAIN
    TESTS -->|référence| APPLICATION
    TESTS -->|référence| INFRA
    TESTS -->|référence| WEB

    style DOMAIN fill:#c8e6c9
    style TESTS fill:#e1f5fe
```

**Règle clé :** `ChronoKraft.Domain` ne référence aucun package externe
(pas d'EF Core, pas d'ASP.NET, pas d'Identity). Toute tentative d'y
importer une de ces dépendances provoque une erreur de compilation.

📄 ADR-0001, ADR-0003

---

## Architecture Globale

```mermaid
graph TB
    subgraph EXT["Systèmes Externes"]
        BADGE[Système de Pointage Externe]
    end

    subgraph USERS["Utilisateurs"]
        PM[Gestionnaire de Projet]
        CR[Représentant Entrepreneur]
        WK[Travailleur]
    end

    subgraph WEBPROJ["ChronoKraft.Web"]
        RAZOR[Razor Pages<br/>CRUD scaffoldé]
        API[API Controllers<br/>+ Swashbuckle/Swagger]
        EXCELCTRL[Excel Controllers]
    end

    subgraph APPPROJ["ChronoKraft.Application"]
        SVC_TASK[TaskManagementService]
        SVC_INVOICE[InvoicingService]
        SVC_TIMESHEET[TimesheetValidationService]
        SVC_GANTT[GanttService — adaptateur mince]
    end

    subgraph DOMPROJ["ChronoKraft.Domain — sans dépendance externe"]
        DOM_GANTT["GanttCalculator<br/>IDependencyStrategy (FS/SS/FF/SF)"]
        DOM_TS["TimesheetValidator.Validate()"]
        DOM_RES["ResourceAttributeValidator"]
        ENTITIES["Entités POCO<br/>Worker, ProjectTask, Project, etc."]
    end

    subgraph INFRAPROJ["ChronoKraft.Infrastructure"]
        DBCONTEXT[ChronoKraftDbContext<br/>EF Core + Identity]
        REPOS[Implémentations Repository]
        EXCELGEN[ClosedXML — Générateurs/Importeurs]
    end

    subgraph DB["PostgreSQL"]
        PGSQL[(PostgreSQL<br/>colonnes JSONB natives via Npgsql)]
    end

    BADGE -->|API push| API
    WK --> RAZOR
    PM --> RAZOR
    CR --> RAZOR

    RAZOR --> SVC_TASK
    RAZOR --> SVC_INVOICE
    API --> SVC_TIMESHEET
    EXCELCTRL --> EXCELGEN

    SVC_TASK --> SVC_GANTT
    SVC_GANTT -.->|délègue| DOM_GANTT
    SVC_TIMESHEET -.->|délègue| DOM_TS
    SVC_TASK -.->|délègue| DOM_RES

    SVC_TASK --> REPOS
    SVC_INVOICE --> REPOS
    SVC_TIMESHEET --> REPOS
    REPOS --> DBCONTEXT
    DBCONTEXT --> PGSQL

    style DOM_GANTT fill:#c8e6c9
    style DOM_TS fill:#c8e6c9
    style DOM_RES fill:#c8e6c9
    style BADGE fill:#ffe0b2
```

📄 ADR-0002, ADR-0003

---

## Séparation Domaine / Application

Les services de `ChronoKraft.Application` restent des adaptateurs
minces : ils lisent/écrivent via les repositories, puis délèguent tout
calcul à `ChronoKraft.Domain`.

```mermaid
graph LR
    subgraph APP["ChronoKraft.Application — testé avec xUnit + PostgreSQL réel"]
        SVC["GanttService.CalculateTaskDates(task)"]
        SVC2["TimesheetValidationService.ValidateWorkerDay(worker, date)"]
    end

    subgraph DOM["ChronoKraft.Domain — testé avec xUnit pur, sans DB"]
        DOM1["IDependencyStrategy<br/>FinishToStartStrategy,<br/>StartToStartStrategy, etc."]
        DOM2["TimesheetValidator.Validate(externalTotal,<br/>declaredHours) → résultat"]
        DOM3["TaskWindow record<br/>(immuable, pas de DbContext,<br/>pas de .Include())"]
    end

    SVC -->|"1. lit ProjectTask via IRepository"| SVC
    SVC -->|"2. construit TaskWindow"| DOM3
    SVC -->|"3. délègue le calcul"| DOM1
    DOM1 -->|"4. retourne une DateOnly pure"| SVC
    SVC -->|"5. SaveChangesAsync() — seule étape avec I/O"| SVC

    SVC2 -->|"1. lit ExternalTimeTotal, TimesheetEntry"| SVC2
    SVC2 -->|"2. délègue la comparaison"| DOM2
    DOM2 -->|"3. retourne IsValid + Discrepancy"| SVC2
    SVC2 -->|"4. crée TimesheetReview si invalide"| SVC2

    style DOM1 fill:#c8e6c9
    style DOM2 fill:#c8e6c9
    style DOM3 fill:#c8e6c9
    style SVC fill:#fff3e0
    style SVC2 fill:#fff3e0
```

**Bénéfices concrets :**
- Ajouter un type de dépendance Gantt = ajouter une classe
  `IDependencyStrategy`, aucune modification du code existant
- `ChronoKraft.Domain.Tests` s'exécute en millisecondes, sans base de
  données

📄 ADR-0003

---

## Authentification et Rôles

Quatre rôles : `Admin`, `Manager`, `Contractor`, `Worker`. Trois d'entre
eux se lient à une entité métier existante via une FK optionnelle
(`ProjectManager.UserId`, `Contractor.UserId`, `Worker.UserId`) ; `Admin`
n'a aucun profil métier associé.

```mermaid
graph TD
    subgraph LOGIN["Connexion"]
        USER["Utilisateur saisit<br/>email + mot de passe"]
    end

    subgraph IDENTITY["ASP.NET Core Identity"]
        AUTH["SignInManager.PasswordSignInAsync()"]
        ROLECHECK["Rôle déterminé :<br/>ApplicationUser.Role"]
    end

    subgraph ROLES["Quatre rôles possibles"]
        ADMIN["Admin<br/>aucun profil métier associé"]
        MANAGER["Manager<br/>lié à ProjectManager.UserId"]
        CONTRACTOR["Contractor<br/>lié à Contractor.UserId"]
        WORKER["Worker<br/>lié à Worker.UserId"]
    end

    subgraph ACCESS["Accès accordé selon le rôle (attribut Authorize)"]
        ADMINPAGES["Administration système,<br/>gestion des comptes"]
        MANAGERPAGES["Création/gestion de projets,<br/>tâches, facturation"]
        CONTRACTORPAGES["Gestion des ressources<br/>et travailleurs de l'entreprise"]
        WORKERPAGES["Consultation de sa propre<br/>feuille de temps (futur)"]
    end

    USER --> AUTH
    AUTH --> ROLECHECK
    ROLECHECK --> ADMIN
    ROLECHECK --> MANAGER
    ROLECHECK --> CONTRACTOR
    ROLECHECK --> WORKER

    ADMIN --> ADMINPAGES
    MANAGER --> MANAGERPAGES
    CONTRACTOR --> CONTRACTORPAGES
    WORKER --> WORKERPAGES

    style ADMIN fill:#fce4ec
    style MANAGER fill:#e1f5fe
    style CONTRACTOR fill:#fff3e0
    style WORKER fill:#e8f5e9
```

**Limitation assumée :** un seul compte de connexion par entreprise
entrepreneur (`Contractor`), pas un compte par employé individuel.

📄 ADR-0012

---

## Modèle de Données — 15 Entités Métier + Identity

Schéma canonique — voir `data-model.mermaid` pour le fichier source
complet, reproduit ici pour référence directe.

```mermaid
erDiagram
    ApplicationUser |o--o| ProjectManager : "authenticates_as"
    ApplicationUser |o--o| Contractor : "authenticates_as"
    ApplicationUser |o--o| Worker : "authenticates_as"

    ProjectManager ||--o{ Project : "manages"
    Project ||--o{ ProjectTask : "contains"
    Project ||--o{ Invoice : "generates"

    Contractor ||--o{ Resource : "provides"
    Contractor ||--o{ Invoice : "invoiced_for"
    Contractor ||--o{ Worker : "employs"

    ResourceType ||--o{ Resource : "defines"
    ResourceType ||--o{ WorkerSkill : "qualifies_for"

    Resource ||--o{ ResourceAssignment : "assigned_in"
    ProjectTask ||--o{ ResourceAssignment : "requires"
    ResourceAssignment ||--o{ ExecutionRecord : "tracks"

    Worker ||--o{ WorkerSkill : "has"
    Worker ||--o{ ExternalTimeTotal : "clocked"
    Worker ||--o{ TimesheetReview : "flagged_for"

    WorkerSkill ||--o{ TimesheetEntry : "logged_under"
    ProjectTask ||--o{ TimesheetEntry : "worked_on"
    TimesheetEntry ||--o| ExecutionRecord : "generates"

    Invoice ||--o{ InvoiceLine : "contains"
    ProjectTask ||--o{ InvoiceLine : "referenced_in"

    ProjectManager {
        int id PK
        int user_id FK
        string name
        string phone
        datetime created_at
    }

    Project {
        int id PK
        int manager_id FK
        string name
        text description
        string client_name
        string status
        date planned_start_date
        date planned_end_date
        date actual_start_date
        date actual_end_date
        decimal estimated_budget
        jsonb metadata
        datetime created_at
        datetime updated_at
    }

    ProjectTask {
        int id PK
        int project_id FK
        string name
        text description
        string required_trade
        string status
        int order
        jsonb attributes
        datetime created_at
        datetime updated_at
    }

    Contractor {
        int id PK
        int user_id FK
        string name
        string contact_name
        string contact_email
        string contact_phone
        text address
        string business_number
        jsonb metadata
        boolean active
        datetime created_at
    }

    ResourceType {
        int id PK
        string name UK
        string resource_category "choices: labor/equipment/material"
        string trade
        string default_billing_unit
        decimal suggested_rate_min
        decimal suggested_rate_max
        text description
        jsonb standard_attributes
        boolean active
        datetime created_at
    }

    Resource {
        int id PK
        int contractor_id FK
        int resource_type_id FK
        int available_quantity
        decimal base_rate
        jsonb attributes
        text notes
        boolean available
        datetime created_at
        datetime updated_at
    }

    ResourceAssignment {
        int id PK
        int task_id FK
        int resource_id FK
        int quantity_resources
        decimal estimated_quantity
        decimal estimated_unit_cost
        string billing_unit
        text notes
        jsonb attributes
        datetime created_at
    }

    ExecutionRecord {
        int id PK
        int assignment_id FK
        int created_by_id FK
        int source_timesheet_entry_id FK "nullable, si généré depuis une TimesheetEntry"
        date execution_date
        int resources_used
        decimal quantity_per_resource
        decimal applied_rate
        string rate_type
        decimal multiplier
        text notes
        datetime created_at
    }

    Worker {
        int id PK
        int contractor_id FK
        int user_id FK "nullable - voir ADR-0012"
        string first_name
        string last_name
        string phone "optionnel"
        boolean active
        datetime created_at
    }

    WorkerSkill {
        int id PK
        int worker_id FK
        int resource_type_id FK
        decimal hourly_rate
        jsonb certifications
        boolean is_primary
        boolean active
        datetime created_at
    }

    ExternalTimeTotal {
        int id PK
        int worker_id FK
        date work_date
        decimal total_hours
        string source_system
        string external_reference_id
        datetime synced_at
    }

    TimesheetEntry {
        int id PK
        int worker_skill_id FK
        int task_id FK
        date work_date
        decimal hours
        text notes
        datetime created_at
    }

    TimesheetReview {
        int id PK
        int worker_id FK
        date work_date
        decimal external_total
        decimal declared_total
        decimal discrepancy
        string status "pending/resolved/ignored"
        int reviewed_by_id FK
        datetime reviewed_at
        text resolution_notes
        datetime created_at
    }

    Invoice {
        int id PK
        int project_id FK
        int contractor_id FK
        string invoice_number UK
        string invoice_type
        date issue_date
        date due_date
        decimal total_amount
        text notes
        jsonb metadata
        datetime created_at
    }

    InvoiceLine {
        int id PK
        int invoice_id FK
        int task_id FK
        string description
        decimal quantity
        decimal unit_price
        decimal total_amount
        int order
    }

    ApplicationUser {
        int id PK
        string email UK
        string password_hash
        string user_role "Admin/Manager/Contractor/Worker - voir ADR-0012"
        datetime created_at
    }
```

Les champs auparavant susceptibles d'attributs dynamiques
(`metadata`, `attributes`, `standard_attributes`, `certifications`)
utilisent le type natif PostgreSQL `jsonb`, mappé directement sur des
`Dictionary<string, object>` en C# — aucune sérialisation manuelle
requise.

📄 ADR-0002, ADR-0005, ADR-0006, ADR-0007, ADR-0009, ADR-0012

---

## Flux de Réconciliation des Feuilles de Temps

Un système externe de pointage fournit un total d'heures par
travailleur/jour ; le travailleur déclare lui-même la ventilation par
rôle. Le système compare les deux, sans jamais bloquer en cas d'écart.

```mermaid
graph TD
    A["Système externe pousse<br/>ExternalTimeTotal via API"] --> C{"TimesheetValidationService<br/>.ValidateWorkerDay()"}
    B["Travailleur saisit sa<br/>TimesheetEntry ventilée par rôle"] --> C

    C --> D["TimesheetValidator.Validate()<br/>ChronoKraft.Domain, sans DB"]
    D --> E{"IsValid ?"}

    E -->|Oui| F["Aucune action supplémentaire<br/>Données acceptées"]
    E -->|Non| G["Créer TimesheetReview<br/>Status = Pending"]

    G --> H["Donnée quand même acceptée<br/>— jamais bloquant"]
    F --> I["TimesheetEntry validée peut<br/>générer un ExecutionRecord"]
    H --> I

    I --> J["ExecutionRecord alimente<br/>InvoicingService"]

    G -.->|Plus tard| K["Révision humaine<br/>Status → Resolved / Ignored"]

    style G fill:#fce4ec
    style H fill:#fff3e0
    style D fill:#c8e6c9
```

**Règles :**
- Le système ne détermine jamais automatiquement le rôle
- Une divergence est toujours acceptée, jamais bloquante
- Toute divergence est toujours tracée via `TimesheetReview`

📄 ADR-0008

---

## Interface Utilisateur

L'interface v0.1 utilise des **Razor Pages scaffoldées**, générées
directement à partir des entités Entity Framework Core
(`dotnet aspnet-codegenerator`). Une API REST (ASP.NET Core Web API +
Swashbuckle) est prévue en v0.2 pour découpler la logique métier de
l'interface de présentation.

📄 ADR-0001, ADR-0004
