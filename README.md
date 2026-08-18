# Elin's PEAK

Elin's PEAK adds persistent, use-based skill progression to PEAK. Skills improve by performing their related actions, and each player's levels are stored locally rather than being selected by the host.

Current release: `0.3.0`

Target game version: PEAK `2.1.a`

## Features

- 22 player-owned skills with a default maximum level of 999.
- Persistent progression with atomic saves and rotating backups.
- Strength, Endurance, Athletics, Agility, Resilience, and specialized climbing skills.
- Separate resistance and natural-recovery progression for Poison, Cold, Heat, Drowsy, and Spores.
- Pack Rat progression that unlocks additional main-inventory and backpack slots.
- Wet Grip and Climbing Tenacity for slippery surfaces and low-stamina climbing.
- A three-section pause-menu display with hover descriptions and current bonuses.
- No experience gain in the Airport/lobby.

## Progression

Experience required for the next level is calculated as:

```text
XP(next) = round(100 * level^1.21)
```

Reduction effects use an anchored curve that reaches the original level-999 target at level 500, then approaches 99.9% reduction at level 999. Positive capacity, regeneration, speed, impulse, air-control, momentum-retention, and natural-recovery bonuses scale linearly.

## Skill overview

- **Strength:** Trains while moving with weight and reduces effective carried weight.
- **Endurance:** Trains from stamina use, increases maximum stamina, and improves regeneration.
- **Wall Climbing:** Improves wall-climbing speed and stamina efficiency.
- **Rope Climbing:** Improves rope-climbing speed and stamina efficiency.
- **Vine Climbing:** Improves vine movement, stamina efficiency, and momentum retention.
- **Athletics:** Trains from walking and sprinting; improves movement, sprint speed, and sprint efficiency.
- **Agility:** Trains from jumping; improves jump impulse, jump efficiency, and light air control.
- **Resilience:** Trains from qualifying fall injury and reduces future fall injury.
- **Pack Rat:** Unlocks inventory capacity and mitigates overflow weight, movement, and stamina penalties.
- **Wet Grip:** Reduces slippery downward pull and climbing drain without modifying Cold.
- **Climbing Tenacity:** Restores control and efficiency while climbing below 20% stamina.
- **Resistance skills:** Reduce incoming Poison, Cold, Heat, Drowsy, and Spore buildup.
- **Recovery skills:** Improve natural recovery for those five conditions. Direct cleansing does not grant recovery XP.
- **Hunger Tolerance:** Trains from movement at 30 or more displayed Hunger.

## Built-in inventory capacity

- Main inventory gains +1/+2/+3/+4 slots at Pack Rat levels 10/50/100/200.
- Backpacks gain +1/+2/+3/+4/+5 slots at levels 20/40/70/120/200.
- Every occupied slot beyond the vanilla 3 main and 4 backpack slots begins with +10% weight, -5% movement force, and +5% stamina cost.
- Pack Rat mitigates all three overflow penalties.

The separate MoreSlots and BackpackCapacity mods should remain disabled when using this built-in implementation.

## Installation

Install with a Thunderstore-compatible mod manager. The package declares BepInEx, PEAKLib Core, and PEAKLib UI as dependencies.

For a local package test in Gale or r2modman, import `dist/Elins_PEAK-0.3.0.zip` as a local mod.

## Save data

Progression is stored at:

```text
%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json
```

Five rotating backups are kept in the adjacent `backups` directory.

## Compatibility notes

- PeakStatsEx currently owns and caches the rendered weight number. Gameplay weight recalculates immediately, but exact one-decimal HUD presentation remains a known compatibility issue.
- Hunger intentionally retains the game's 2.5-point display increments.
- Multiplayer is designed around player-owned local progression, but final multiplayer runtime validation is still pending.
- Anti-farming and diminishing-return systems beyond basic action validity are intentionally deferred.

## Build and test

Copy `Config.Build.user.props.template` to `Config.Build.user.props`, then set the local PEAK installation and Gale profile paths.

```powershell
dotnet build .\PEAKUsageSkills.slnx -c Release
dotnet test .\PEAKUsageSkills.slnx -c Release
.\scripts\Build-Package.ps1
```

The build does not deploy to Gale unless `DeployToDevtest` is explicitly set to `true`.

## Source

Source code and issue tracking: https://github.com/kunrian/Elins-PEAK
