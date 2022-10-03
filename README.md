## Environnement

 - complètement observable
 - stochastique
 - séquentiel
 - continu
 - dynamique
 - agent unique
## Agent
L'agent est un agent basé sur les buts
Capteurs : performance, objets
### État mental BDI
Beliefs : liste des cases où poussières || bijoux == true avec la distance
Desires : liste des cases où aller ordonnées selon la distance
Intentions : Pile d'actions (aspirer, ramasser, droite, ...)
### Fonction d'agent
|A |B |
|--|--|
|C |D |
L'agent est sur A :
|**Percepts\Actions**|Aspirer|Ramasser|Droite|Gauche|Haut|Bas|
|--|--|--|--|--|--|--|
|[A, poussière]|X||||||
|[A, propre]|||X||||
|[A, bijoux]||X|||||
|[B, poussière]|X||||||
|[B, propre]||||X|||
|[B, bijoux]||X|||||
|[C, poussière]||||||X|


