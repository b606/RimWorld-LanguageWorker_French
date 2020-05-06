# RimWorld-LanguageWorker_French
![]( https://raw.githubusercontent.com/wiki/b606/RimWorld-LanguageWorker_French/images/LWKR_French_Mod_banner.png)

A mod to make the French language in RimWorld as grammatically correct as possible.

Credits:
 - Modified from https://github.com/Elevator89/RimWorld-LanguageWorker_Russian
   (Credit to Elevator89)
 - Include some code snippets from the official LanguageWorker_French and translators work at
   https://github.com/Ludeon/RimWorld-fr/ (Credit to Adirelle for the first regexes)
 - Thanks to Pardeike for his fabulous libHarmony patching library.

### Goal
 - Maintain the French RimWorld Translation at maximum quality.
 - Make the French LanguageWorker in RimWorld at maximum quality.
 - Testbed for the French LanguageWorker in RimWorld.
 
### Vérification des résultats

Les modifications faites par ce mod concernent essentiellement la qualité de la traduction, qui sont impossibles à faire sans modifier le code C# du jeu. Ce mod est donc un complément du travail des traducteurs de RimWorld-fr (https://github.com/Ludeon/RimWorld-fr/) pour le jeu actuel, ou en attendant l'intégration de ces modifications dans une prochaine version du jeu.

Le mod est focalisé sur les accords de mots, les règles de grammaire ou de typographie etc. Si vous avez l'œil averti, vous verrez un subtil changement par-ci par-là. J'espère que vous apprecierez les quelques améliorations dans le français du jeu :smile:

Des captures d'écran les illustrent dans le Wiki (https://github.com/b606/RimWorld-LanguageWorker_French/wiki).

Un rapport de test détaillé montre l'évolution et le travail que fait le mod derrière la scène : https://htmlpreview.github.io/?https://github.com/b606/RimWorld-LanguageWorker_French/blob/master/Archives/doc/TestReport.html.

N'hésitez pas à remonter les suggestions, les problèmes ou bugs sur ce site en ouvrant une nouvelle page dans l'onglet Issue.

### Installation

À partir de la version 1.1.0, le mod est sur Steam (https://steamcommunity.com/sharedfiles/filedetails/?id=2081845369). Sinon, il suffit de télécharger l'archive zip des binaires compilés (https://github.com/b606/RimWorld-LanguageWorker_French/releases/latest), de l'extraire dans le répertoire de Mods de RimWorld, et d'activer ou désactiver le mod "LanguageWorkerFrench_Test" dans le jeu :

 - sous Windows : `C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\LanguageWorker_French_Mod`
 - sous Linux : `~/.steam/steam/steamapps/common/RimWorld/Mods/LanguageWorker_French_Mod`
 - sous Mac : aller dans le Finder, mettre `~/Library`, Application Support, Steam, SteamApps, Common, Rimworld, RimWorldMac, clic-droit et "Show package contents" (https://ludeon.com/forums/index.php?topic=3549.msg468403#msg468403).

![]( https://raw.githubusercontent.com/wiki/b606/RimWorld-LanguageWorker_French/images/LWKR_French_Mod_folders.png)
 
#### Installation alternative pour les versions avant 1.1.0

EDIT 2020/05/01: la procédure pour les versions avant le 1.1.0 est toujours valable mais non nécessaire.

  1. Télécharger l'archive zip des binaires compilés (https://github.com/b606/RimWorld-LanguageWorker_French/releases/latest) et l'extraire dans le répertoire de Mods de RimWorld.
 
  2. Activer le mod "LanguageWorkerFrench_Test" dans le jeu.
    
  3. Modifier le fichier LanguageInfo.xml dans le répertoire `Core/Languages/RimWorld-fr` (ou `Core/Languages/French`) pour contenir `<languageWorkerClass>RimWorld_LanguageWorker_French.LanguageWorker_French</languageWorkerClass>` au lieu de `<languageWorkerClass>LanguageWorker_French</languageWorkerClass>`.
   
  4. Relancer RimWorld.
 
  **Désinstallation** : refaire les étapes précédentes dans l'ordre inverse et désactiver le mod dans le jeu.
 
### Changelog

2020/05/05.
  - Corrections pour fixer le genre des mots considérés neutre en anglais (patchs des RulesForDef et 
    RulesForBodyPartRecord). Plusieurs marqueurs "x_possessive" sont donc fonctionnels pour les traducteurs de
    RimWorld-fr, au lieu de "son/sa" (ou "le/la") jusqu'ici.
 - Les labels toujours au pluriel sont détectés. Comme il se doit, ils se résoudront en "ses" pour les tags 
   "x_possessive", "les x" pour "x_definite" et "des x" pour "x_indefinite".
  - Développement complet de la fonction de pluralisation pour presque tous les labels dans le jeu (1555 labels),
    et correction de certains d'entre eux dans la traduction RimWorld-fr.

2020/05/01.
  - Depuis la version 1.1.0, le mod paramètre automatiquement le module de traitement pour le langage français (languageWorkerClass). L'activation est basée sur le nom interne du langage ("friendly name: French") et ne concerne pas les autres langages qui n'ont pas ce nom interne. Il n'y a plus besoin de toucher au fichier LanguageInfo.xml.

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
  
### Travaux en cours

  - ~~Corriger les noms d'espèces au masculin ou féminin dans les textes du jeu (si la prochaine version de RW ne le corrige pas).~~
  - ~~Pouvoir activer le mod sans toucher au fichier LanguageInfo.xml : nécessite de se brancher dans le code de démarrage.~~
  - Développer d'autres patches là où il y aurait besoin (les Tales, les textes des interfaces etc).
 
---
[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z51KQ21)
