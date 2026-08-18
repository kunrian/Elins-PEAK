# Changelog

## 0.4.2

- Added translated skill names and tooltips for Simplified Chinese, Japanese, Korean, German, Spanish, and French. The skill panel now follows PEAK's language setting and refreshes when it changes; untranslated languages fall back to English.
- Fixed Petrification so real gains from status effects, amulets, and Citadel hazards receive the skill reduction and award EXP correctly.

## 0.4.1
### Please delete your current config file and regenerate by starting the game.

- Renamed Resilience to Vitality while preserving existing progression and settings.
- Shortened the eight Resiliency skill names in the pause menu.
- Further adjusted EXP values.

## 0.4.0

- Combined condition Resistance and Recovery into eight blue Resiliency skills, including new Curse Tolerance and Petrification Resistance.
- Tolerances now train only from receiving their matching affliction; existing Recovery progression is merged into its Tolerance.
- Fanny Packs and Jet Packs now receive Strength's extra item slots. Jet fuel remains separate, and Rocket Packs are excluded.
- Rebalanced Endurance, climbing, Athletics, Agility, Wet Grip, and Climbing Tenacity XP.
- Moved the Resiliency panel below Main Skills and the test controls to the right.
- Removed stale configurations and cleaned up a bit

## 0.3.2

- Removed the bugged main-inventory slot expansion.
- Retired Pack Rat and its overflow Weight, movement, and stamina penalties.
- Moved backpack-slot unlocks to Strength at levels 20, 40, 70, 120, and 200.
- Fixed Cold Recovery so passive environmental warming counts, while status-changing items still do not grant Recovery XP.
- Reduced Athletics XP by 30% and Hunger Tolerance XP by 90% after runtime testing.
- Added the new package icon.

## 0.3.1

- Updated Readme's and manifest. Sounded *too* robotic. I need to at least give it some personality.

## 0.3.0

- Initial public package.
- Added 22 persistent, player-owned usage skills.
- Added scalable stamina, movement, climbing, resistance, recovery, and inventory effects.
- Added built-in Pack Rat inventory expansion and overflow penalties.
- Added pause-menu skill panels and hover explanations.
- Added atomic saves, rotating backups, Airport XP exclusion, and diagnostic logging.
