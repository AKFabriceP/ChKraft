# ADR-0016: Auth0 et RBAC hiérarchique par portée métier

## Statut
En étude — évaluation de faisabilité, aucune décision prise. Si cette
étude aboutit à une acceptation, cet ADR **remplacerait ADR-0012**
(authentification ASP.NET Core Identity, rôle plat sans portée).

## Contexte
Deux besoins distincts, discutés séparément puis rapprochés :

1. **Authentification déléguée à Auth0** plutôt que gérée localement
   par ASP.NET Core Identity
2. **RBAC hiérarchique par portée**, plus riche que le rôle plat actuel
   de l'ADR-0012 (`[Authorize(Roles = "Manager")]` autorise *tout*
   `Manager`, sans distinguer *lequel* de ses projets il tente
   d'accéder)

## Flux de provisionnement proposé
1. L'utilisateur crée son compte via Auth0 (self-service)
2. Le compte local correspondant est créé automatiquement au premier
   login, avec un statut **"en attente"** — aucun rôle, aucun accès
3. Un `Admin` ou un `ProjectManager` (selon le rôle à attribuer)
   assigne explicitement le rôle et le lien vers le profil métier
   correspondant (`Worker`, `Contractor`, etc.)

## Hiérarchie de rôles proposée

| Rôle | Portée d'accès | Peut attribuer |
|---|---|---|
| `Admin` | Tout le système | N'importe quel rôle |
| `ProjectManager` | Tout ce qui concerne **ses propres projets** uniquement | Rôle `Contractor` — nouveau ou déjà existant — sur son projet |
| `Contractor` | Tout ce qui concerne **son entreprise**, ses ressources, ses travailleurs | Rôle `Worker`, pour ses propres travailleurs |
| `Worker` | Ses tâches, ses feuilles de temps, et un accès **restreint** à certaines informations du projet/contractor (portée exacte non définie — voir `open-questions.md`, Q2) | Rien |

**Différence fondamentale avec l'ADR-0012 :** ce n'est plus "quel rôle
avez-vous" mais "quel rôle avez-vous, **sur quelle ressource précise**"
— un `ProjectManager` n'a pas un accès global à tous les projets, un
`Contractor` n'a pas un accès global à toutes les entreprises.

## Ce qui doit être étudié techniquement, sans être tranché ici

### 1. L'autorisation par rôle seul ne suffit plus
`[Authorize(Roles = "Manager")]` répond à "quel est votre rôle", jamais
à "êtes-vous le gestionnaire *de ce projet précis*". Ça nécessite une
**autorisation par politique** (`IAuthorizationHandler` personnalisé
dans ASP.NET Core), vérifiant la relation entre l'utilisateur et la
ressource demandée à chaque requête — un changement d'architecture
d'autorisation, pas juste une configuration de rôles.

### 2. Attribution du rôle Contractor par un ProjectManager — pas de lien métier requis
Clarifié : l'action d'un `ProjectManager` qui "attribue le rôle
Contractor" à un utilisateur est une **assignation RBAC** (sur le
compte utilisateur), pas la création d'une relation métier
`Project`↔`Contractor`. Aucune nouvelle entité de liaison n'est
nécessaire pour cette raison — l'hypothèse initiale de ce point est
retirée. Le lien réel entre un projet et un entrepreneur continue de
passer par `ResourceAssignment`, comme aujourd'hui. Une question
connexe subsiste néanmoins sur la traçabilité de cette assignation de
rôle — voir `open-questions.md` Q4.

### 3. Statut "en attente" sur le compte utilisateur
Nécessite un champ de statut sur la table utilisateur locale (`Pending`,
`Active`, potentiellement `Suspended`), absent du modèle `AppUser`
esquissé précédemment.

### 4. Portée exacte de l'accès `Worker`
Non définie — dépend de la réponse du client, voir `open-questions.md`
Q2.

### 5. Visibilité croisée entre `ProjectManager`
Un `ProjectManager` peut-il voir les projets d'un autre `ProjectManager`,
même en lecture seule ? Non tranché — voir `open-questions.md` Q3. La
réponse change directement la conception du point 1 (l'autorisation par
politique doit-elle être stricte ou permettre un mode lecture élargi).

## Conséquences si cette étude aboutit à une acceptation
- Remplacerait entièrement ADR-0012
- `AppUser` remplacerait `ApplicationUser` (voir la conception déjà
  esquissée dans la conversation précédente — POCO simple avec
  `Auth0UserId`, plus d'héritage `IdentityUser<int>`)
- Nécessiterait des `IAuthorizationHandler` personnalisés pour chaque
  type de vérification de portée (projet, entreprise, tâche)
- Impact significatif sur `v0.1-implementation-guide.md`,
  `data-model.mermaid`, `architecture.md`/`.html` — pas un changement
  mineur

## Alternatives à considérer pendant l'étude
- Garder le rôle plat de l'ADR-0012 et gérer la restriction de portée
  entièrement au niveau des requêtes (filtrer les projets par
  `manager_id` dans chaque service) sans couche d'autorisation dédiée —
  plus simple à implémenter, mais répartit la logique de sécurité dans
  chaque service plutôt que de la centraliser, avec un risque d'oubli
- Système de permissions par claims plutôt que par rôle hiérarchique
  fixe — plus flexible à long terme, plus complexe à mettre en place
  pour la portée actuelle du projet
