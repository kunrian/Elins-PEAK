# Changelog

## Unreleased

- Added a canonical project handoff covering implemented behavior, runtime evidence, failed/retired approaches, compatibility findings, pending validation, future skill options, and the next test sequence.
- Added exact skill/balance, architecture/hook, development/release, reference-mod, roadmap, and file-index documentation.
- Documented that the current `devtest` installation/logs are still 0.3.1 and cannot validate the final 0.3.2 Strength-backpack, Cold Recovery, or XP-balance changes.
- Added repository-level agent guidance so future work preserves local-player progression, vanilla main inventory, retired Pack Rat behavior, save safety, and current scope decisions.

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
