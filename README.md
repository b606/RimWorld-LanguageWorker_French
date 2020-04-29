# RimWorld-LanguageWorker_French
A mod to make the French language in RimWorld as grammatically correct as possible.

Credits:
 - Modified from https://github.com/Elevator89/RimWorld-LanguageWorker_Russian
   (Credit to Elevator89)
 - Include some code snippets from the official LanguageWorker_French and translators work at
   https://github.com/Ludeon/RimWorld-fr/ (Credit to Adirelle for the first regexes)
 - Thanks to Pardeike for his fabulous libHarmony patching library.

#### Goal
 - Maintain the French RimWorld Translation at maximum quality.
 - Make the French LanguageWorker in RimWorld at maximum quality.
 - Testbed for the French LanguageWorker in RimWorld.
 
#### Vérification des résultats

Les modifications faites par ce mod concernent essentiellement la qualité de la traduction, qui sont impossibles à faire sans modifier le code C# du jeu. Ce mod est donc un complément du travail des traducteurs de RimWorld-fr (https://github.com/Ludeon/RimWorld-fr/) pour le jeu actuel, ou en attendant l'intégration de ces modifications dans une prochaine version du jeu.

Le mod est focalisé sur les accords de mots, les règles de grammaire ou de typographie etc. Si vous avez l'œil averti, vous verrez un subtil changement par-ci par-là.

Des captures d'écran les illustrent dans le Wiki (https://github.com/b606/RimWorld-LanguageWorker_French/wiki).

Un rapport de test détaillé montre l'évolution et le travail que fait le mod derrière la scène : https://htmlpreview.github.io/?https://github.com/b606/RimWorld-LanguageWorker_French/blob/master/LanguageWorker_French_Mod/doc/TestReport.html.

N'hésitez pas à remonter les suggestions, les problèmes ou bugs sur ce site.

#### Installation

 1. Télécharger l'archive zip des binaires compilés (https://github.com/b606/RimWorld-LanguageWorker_French/releases/latest) et l'extraire dans le répertoire de Mods de RimWorld.
 
 2. Activer le mod "LanguageWorkerFrench_Test" dans le jeu.
 
    **ATTENTION** : si le mod n'est pas activé avant l'étape suivante, le jeu ne s'executera
    pas correctement et vous serez obligé de fermer RimWorld brutalement (par un `xkill`).
    
    TIP : Çà m'arrive si souvent que j'utilise une branche git spéciale pour le test de ce mod.
    Vous pouvez alternativement modifier ModsConfig.xml pour contenir `<li>b606.languageworkerfrench.mod</li>`
    dans la section `<activeMods>`, ce qui vous évite de relancer le jeu à chaque changement.
    
 3. Modifier le fichier LanguageInfo.xml dans le répertoire `Core/Languages/RimWorld-fr` (ou `Core/Languages/French`) pour contenir
   `<languageWorkerClass>RimWorld_LanguageWorker_French.LanguageWorker_French</languageWorkerClass>`
   au lieu de `<languageWorkerClass>LanguageWorker_French</languageWorkerClass>`.   
    NB: Je cherche actuellement le moyen d'éviter cette étape pour faciliter l'installation (à suivre).
   
 4. Relancer RimWorld pour apprecier quelques améliorations dans le français du jeu :smile:,
    et ne pas hésiter à remonter ici les suggestions.
 
#### Désinstallation

Il faut refaire les étapes précédentes dans l'ordre inverse.

 1. Remettre `<languageWorkerClass>LanguageWorker_French</languageWorkerClass>` dans 
    le fichier LanguageInfo.xml.
 2. Désactiver le mod dans le jeu.
 
#### Changelog

2020/04/29.
  - Correction des genres grammaticaux des espèces d'animaux :
    PawnKind toujours au féminin en français : Boomalope, Gazelle, Megaspider, Ostrich, Tortoise.
    PawnKind toujours au masculin en français : Alphabeaver, Bear_Grizzly, Boomrat, Capybara, Caribou, Cassowary, Chinchilla, Cobra, Cougar, Dromedary, Elk, Emu, Fox_Fennec, GuineaPig, Husky, Iguana, LabradorRetriever, Lynx, Megascarab, Megasloth, Muffalo, Raccoon, Rhinoceros, Spelopede, Squirrel, Thrumbo, Warg, YorkshireTerrier.

2020/04/27.
  - Patch pour corriger les accords de genre des noms d'animaux (et des pawns en général), illustration dans le wiki.
  - Le nom du mod est officiellement LanguageWorker_French_Mod (b606.languageworkerfrench.mod).

2020/04/21.
  - Tous les noms dans le jeu sont catégorisés correctement, avec une première version de mise en majuscules ou minuscules comme il se doit (amélioration à suivre).
  - Les titres des quêtes ne sont plus tout en majuscule.
  - Complète la liste des mots avec exception aux règles.
  - En interne, cartographie du code de RimWorld.

2020/04/05.
  - Mise en place des fichiers pour utiliser Monodevelop et NUnit testing framework.
  - Corrige les élisions y compris avec les tags de soulignement de noms :
    - Corrige les listes de voyelles.
    - Modification des Regex.
    - Prend en compte les h muets et h aspirés (liste de mots limitée à ceux contenus
      dans RimWorld, à mettre à jour si besoin).
  - Corrige les X_possessive [mst]a et [mst]on/[mst]a en [mst]on avant une voyelle et h muet.
  
#### Travaux en cours

  - ~~Corriger les noms d'espèces au masculin ou féminin dans les textes du jeu (si la prochaine version de RW ne le corrige pas).~~
  - Pouvoir activer le mod sans toucher au fichier LanguageInfo.xml : nécessite de se brancher dans le code de démarrage.
  - Développer d'autres patches là où il y aurait besoin (les Tales, les textes des interfaces etc).
 
---
[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z51KQ21)
