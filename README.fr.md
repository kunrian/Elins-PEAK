# Elin's PEAK

**Languages:** [English](README.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md)

## Faites progresser absolument tout !

PEAK vous donne déjà des ascensions presque mortelles, des sacs trop chargés, du poison, du froid, de la chaleur, de l'épuisement et quelques chutes parfaitement évitables. **Elin's PEAK fait en sorte que votre personnage apprenne réellement de tout ça !**

Le principe est simple : **plus vous faites quelque chose, meilleur vous devenez.** Se déplacer avec du Weight entraîne la Force ; grimper entraîne le type d'escalade utilisé ; marcher, sprinter, sauter, survivre aux chutes et subir des afflictions deviennent une progression permanente.

Le système est fortement inspiré de **Elin**. Il n'y a pas un niveau unique qui décide de toutes vos capacités, ni une réserve de points génériques à distribuer après une partie. Les compétences progressent parce que vous les avez réellement utilisées.

Le mod propose actuellement **18 compétences persistantes**, avec un niveau maximum par défaut de **999**. La progression appartient à votre joueur, est sauvegardée localement et continue entre les sessions.

## Compétences

### Physique
- **Force** : progresse lorsque vous vous déplacez avec du Weight. Réduit le Weight effectif et débloque des emplacements de sac.
- **Endurance** : progresse en dépensant de l'endurance. Augmente la véritable endurance maximale et sa régénération.
- **Athlétisme** : la marche et le sprint la font progresser. Améliore le déplacement, le sprint et son efficacité.
- **Agilité** : progresse en sautant. Améliore l'impulsion, l'efficacité du saut et légèrement le contrôle aérien.
- **Vitalité** : progresse avec de vraies blessures de chute. Réduit les futures Injury de chute.

### Escalade
- **Escalade de paroi**, **à la corde** et **de lianes** améliorent chacune leur vitesse et leur efficacité.
- **Adhérence humide** progresse sur les parois glissantes et réduit la traction vers le bas et les coûts associés.
- **Ténacité d'escalade** progresse en continuant à grimper sous 20% d'endurance normale et réduit les pénalités de cette situation.

### Résistance
Le panneau bleu **Résistance** contient huit compétences :
**Poison, Froid, Chaleur, Somnolence, Spores, Faim, Malédiction et Pétrification.**

Elles ne gagnent de l'EXP que lorsque l'affliction correspondante augmente réellement. Poison, Froid, Chaleur, Somnolence et Spores améliorent aussi les voies que PEAK marque comme récupération naturelle.

## La Force agrandit aussi les sacs

| Niveau de Force | Emplacements supplémentaires |
|---:|---:|
| 20 | +1 |
| 40 | +2 |
| 70 | +3 |
| 120 | +4 |
| 200 | +5 |

L'inventaire principal reste à la taille vanilla. Backpack, Fanny Pack et Jet Pack reçoivent les mêmes paliers ; l'emplacement de carburant du Jet Pack n'est pas modifié.

> **BackpackCapacity** doit rester désactivé car les deux mods modifient les mêmes données de sac.

## Votre progression vous appartient
La progression est sauvegardée localement. Rejoindre le lobby de quelqu'un d'autre ne permet pas à l'hôte d'écraser vos niveaux. Aucune EXP n'est gagnée dans l'Airport/lobby. L'EXP en partie personnalisée est désactivée par défaut.

## Langue
L'interface du mod suit automatiquement la langue sélectionnée dans PEAK et se met à jour immédiatement lorsqu'elle change. Les langues ou clés non traduites reviennent automatiquement à l'anglais.

---

# Détails techniques

## Courbe d'EXP
```text
XP(next) = round(100 * level^1.21)
```
Maximum par défaut : 999.

## Sources d'EXP
| Compétence | Source |
|---|---|
| Endurance | 10 EXP par point normalisé d'endurance brute demandée |
| Force | 2 EXP par Weight brut × mètre parcouru |
| Paroi/corde/liane | 8 EXP par mètre valide |
| Marche | 0,22 EXP par mètre |
| Sprint | 1,05 EXP par mètre |
| Agilité | 8 EXP par saut valide |
| Vitalité | 100 EXP par point normalisé d'Injury de chute |
| Résistances | 100 EXP par point réellement appliqué de l'affliction correspondante |
| Adhérence humide | 20 EXP par mètre glissant, pondéré par le niveau de glisse |
| Ténacité | 40 EXP par mètre de paroi sous 20% d'endurance normale |

Les bonus positifs sont principalement linéaires : endurance maximale +0,5%/niveau, régénération +0,1%, vitesse d'escalade +0,3%, déplacement au sol +0,1%, sprint supplémentaire +0,2%, saut +0,15%, contrôle aérien +0,025%.

Les réductions utilisent une **courbe réciproque ancrée** et approchent **99,9% de réduction** au niveau 999.

## Sauvegarde
```text
%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json
```
Écriture atomique et cinq sauvegardes tournantes.

Source et issues :
https://github.com/kunrian/Elins-PEAK
