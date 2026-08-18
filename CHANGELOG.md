# Changelog

## 0.4.1

- Renamed Resilience to Vitality while preserving existing progression and settings.
- Shortened the eight Resiliency skill names in the pause menu.

## 0.4.0

- Merged Poison/Cold/Heat/Drowsy/Spore Resistance and Recovery into single Tolerance skills. Existing Recovery progression is converted into its matching Tolerance without discarding accumulated XP or work.
- Renamed the blue condition panel to Resiliency, moved it below Main Skills on the left, removed the green Recovery panel, and moved the two runtime test buttons to the right.
- Added Curse Tolerance and Petrification Resistance. Petrification covers the shared amulet/Citadel gain path; neither skill has a recovery-speed effect.
- Tolerances train only when the matching affliction increases. Hunger movement training and all Recovery XP were removed.
- Halved both condition-resistance and recovery-speed bonuses to 0.15% per level now that each Tolerance owns both effects.
- Added Strength item-slot milestones to Fanny Packs and Jet Packs while preserving Jet Pack fuel and excluding Rocket Packs. Backpacks retain their existing behavior.
- Rebalanced XP: Endurance 3x; wall/rope/vine climbing 2x; Wet Grip and Climbing Tenacity 3x; Athletics and Agility 20% lower.
- Removed stale Pack Rat, recovery-XP, and Hunger-movement configuration entries.
- Documented the sprint-to-jump momentum finding and candidate Gliding/Aeronautics and Throwing skills.
- Updated package/source version to 0.4.0.

## 0.3.2

- Removed the bugged main-inventory slot expansion. Your normal inventory stays vanilla-sized.
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
