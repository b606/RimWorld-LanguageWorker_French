# RimWorld-LanguageWorker_French
A mod to test RimWorld-LanguageWorker_French

Modified from https://github.com/Elevator89/RimWorld-LanguageWorker_Russian
(Credit to Elevator89)

#### Goal
 - Maintain the French RimWorld Translation at maximum quality.
 - Make the French LanguageWorker in RimWorld at maximum quality.

#### Installation

 1. Télécharger l'archive zip des binaires compilés (https://github.com/b606/RimWorld-LanguageWorker_French/releases/latest) et l'extraire dans le répertoire de Mods de RimWorld.
 2. Activer le mod "LanguageWorkerFrench_Test" dans le jeu.
 
    **ATTENTION** : si le mod n'est pas activé avant l'étape suivante, le jeu ne s'executera
    pas correctement et vous serez obligé de fermer RimWorld brutalement (par un `xkill`).
    
    TIP : Çà m'arrive si souvent que j'utilise une branche git spéciale pour le test de ce mod.
    Vous pouvez alternativement modifier ModsConfig.xml pour contenir `<li>b606.languageworkerfrench.test</li>`
    dans la section `<activeMods>`, ce qui vous évite de relancer le jeu à chaque changement.
 3. Modifier le fichier LanguageInfo.xml dans le répertoire `Core/Languages/RimWorld-fr` (ou `Core/Languages/French`) pour contenir
   `<languageWorkerClass>RimWorld_LanguageWorker_French.LanguageWorker_French</languageWorkerClass>`
   au lieu de `<languageWorkerClass>LanguageWorker_French</languageWorkerClass>`.
 4. Relancer RimWorld pour apprecier quelques améliorations dans le français du jeu :smile:,
    et ne pas hésiter à remonter ici les suggestions.
 
#### Désinstallation

Il faut refaire les étapes précédentes dans l'ordre inverse.

 1. Remettre `<languageWorkerClass>LanguageWorker_French</languageWorkerClass>` dans 
    le fichier LanguageInfo.xml.
 2. Désactiver le mod dans le jeu.
 
#### Changelog

2020/04/05.
  - Mise en place des fichiers pour utiliser Monodevelop et NUnit testing framework.
  - Corrige les élisions y compris avec les tags de soulignement de noms :
    - Corrige les listes de voyelles.
    - Modification des Regex.
    - Prend en compte les h muets et h aspirés (liste de mots limitée à ceux contenus
      dans RimWorld, à mettre à jour si besoin).
  - Corrige les X_possessive [mst]a et [mst]on/[mst]a en [mst]on avant une voyelle et h muet.
 
---
[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z51KQ21)
