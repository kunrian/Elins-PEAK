# Elin's PEAK

Elin's PEAK adds persistent, use-based progression to PEAK. Train skills by performing their related actions, retain progress between sessions, and build a character without allowing the host to choose everyone else's level.

## Highlights

- 22 skills with a default maximum level of 999.
- Player-owned local progression with safe saves and rotating backups.
- Strength, Endurance, Athletics, Agility, Resilience, and separate wall, rope, and vine climbing.
- Wet Grip for slippery climbing and Climbing Tenacity below 20% stamina.
- Separate Resistance and natural Recovery skills for Poison, Cold, Heat, Drowsy, and Spores.
- Hunger Tolerance that trains while moving at 30 or more Hunger.
- Pack Rat inventory progression with built-in main and backpack slot expansion.
- Pause-menu panels with skill levels, hover explanations, and current bonuses.
- No XP gain in the Airport/lobby.

## Inventory progression

Pack Rat unlocks up to four additional main-inventory slots and five additional backpack slots. Carrying items beyond the vanilla limits adds weight, movement, and stamina penalties; Pack Rat progressively mitigates those penalties.

Do not use MoreSlots or BackpackCapacity alongside Elin's PEAK. Their functionality is built into this mod.

## Progression and saves

Levels use `XP(next) = round(100 * level^1.21)`. Progress is stored per player at:

```text
%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json
```

The mod keeps five rotating backups.

## Compatibility notes

- Built for PEAK `2.1.a`.
- PeakStatsEx may continue to show its cached rounded weight number even though gameplay weight is recalculated immediately.
- Hunger keeps the game's 2.5-point display increments.
- Final multiplayer runtime validation is still pending.
- Anti-farming and diminishing-return systems are intentionally deferred.

## Source and issues

The source code is public at https://github.com/kunrian/Elins-PEAK.
