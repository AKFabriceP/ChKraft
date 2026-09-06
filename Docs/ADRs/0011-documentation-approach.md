# ADR-0011: Documentation "docs as code" avec ADRs

## Statut
Accepté

## Contexte
Un projet logiciel évolutif nécessite un suivi fiable de ses décisions
d'architecture — pas seulement de son état actuel. Une documentation
narrative unique, mise à jour en continu, rend difficile de répondre
rapidement à "pourquoi ce choix a-t-il été fait ?" longtemps après coup.

## Décision
Adopter une approche "docs as code" : documentation en texte brut
(Markdown), versionnée dans le même dépôt que le code, structurée en
couches :
- **ADRs** (`docs/adr/`) — une décision par fichier, immuable une fois
  acceptée ; toute décision renversée est documentée par un nouvel ADR
  qui remplace l'ancien, jamais par une édition en place
- **Diagrammes** en Mermaid par défaut (diffable, rendu natif sur
  GitHub/GitLab)
- **Référence API** auto-générée via Swashbuckle/OpenAPI
- **Documentation narrative** (plans de version, guides d'installation)
  en Markdown simple

Un site de documentation dédié (ex. DocFX, MkDocs) est reporté tant que
l'équipe reste réduite.

## Conséquences
- Toute nouvelle décision architecturale significative doit produire un
  ADR numéroté séquentiellement au moment où elle est prise
- Nécessite une discipline continue — un ADR non écrit au moment de la
  décision perd l'essentiel de sa valeur

## Alternatives considérées
- Confluence/Notion : rejeté pour ce projet — documentation externe au
  dépôt, risque de désynchronisation avec le code, pas de diff par
  commit
