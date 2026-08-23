# Testing and compatibility

## Evidence state

| Area | State |
|---|---|
| 0.3.2 solo runtime | Passed in `devtest` on 2026-08-18; startup/UI buttons/Backpack milestones/reset produced expected logs with no plugin exception. |
| Published 0.4.5 gameplay build | Solo `devtest` log on 2026-08-19 loaded all six 36-key catalogs, resolved Petrification and both pass-out hooks, and observably suppressed vanilla pass-out below the expanded Endurance threshold. The DLL identity remained 0.4.2 by mistake. |
| Corrected 0.4.5 Release compile/package | Passed on 2026-08-23 with zero warnings/errors; manifest, plugin, project, and assembly versions all report 0.4.5. Exact hashes are in `DEVELOPMENT_AND_RELEASE.md`. |
| 0.4.6 Release compile/package/deployment | Passed on 2026-08-23 with zero warnings/errors; exact package installed into `devtest`, prior live package backed up, and config left unchanged. |
| 0.4.6 solo runtime | Passed on 2026-08-23. The log reports 0.4.6, all expected hooks/catalogs healthy, no `BonusSprint` hook, and schema 7 values; the owner confirmed sprinting stops at regular-stamina exhaustion without using bonus/food stamina. |
| Config schema 7 migration | Runtime-verified in the active config; Endurance `20`, sprinting Athletics `0.9`, and the remaining owner values were preserved. |
| 0.4.1 live log | Plugin loaded, but Petrification was missing because Harmony found both `AddPetrify(int)` and the parameterless console-command overload. |
| 0.4.7 Hunger balance | Source and focused math test cover the 10-point gate and one-fifth XP rate. Package validation is recorded in `HANDOFF.md`; gameplay runtime remains pending. |
| 0.4.4 modded-scene eligibility | Implementation no longer requires `Level_*`; TerrainRandomiser 1.1.10 co-loaded without an Elin's PEAK patch exception. A deliberately renamed gameplay-scene comparison remains optional follow-up. |
| Multiplayer | Never runtime-tested. |
| Cold natural warming speed | Open; latest log classified it as `SubtractLocal`. |

The full unit suite is not required for this focused balance restoration. Run the Hunger-focused math test, build the solution to compile both projects, validate package structure, and use the focused live sequence below.

## Focused 0.4.7 live sequence

1. Confirm BepInEx and the plugin line both report `Elin's PEAK 0.4.7`, each locale file reports 36 registered keys, and all patch-health entries remain healthy.
2. Confirm config schema 8 contains `HungerMovementXpPerWork = 0.02` and `HungerTrainingThreshold = 0.10` while retaining other custom values.
3. Move below displayed Hunger 10 and confirm Hunger XP does not change.
4. Move the same approximate distance at Hunger 10, 30, and a higher value. Confirm `HungryMovement` is the work source, XP scales with displayed Hunger at 0.02 XP per point-meter, and teleport/respawn deltas do not award it.
5. Let Hunger increase while stationary and confirm the incoming gain applies resistance but awards no Hunger XP.

## Broader pending sequence

1. Confirm the English ESC layout, then change PEAK's language to French, German, Spanish Spain, Spanish LatAm, Simplified Chinese, Japanese, and Korean. Names, headings, and tooltips should refresh immediately without reopening the game.
2. Check CJK glyphs and tooltip wrapping. Then select at least one untranslated language (for example Italian or Traditional Chinese) and confirm clean English fallback instead of `LOC:` keys.
3. Confirm the previous Resilience level/XP appears under Vitality and the old save/config keys are gone after a save.
4. Test typed storage at one milestone and at +5: Backpack `4+n`, Fanny `2+n`, Jet `1+n` item slices plus exactly one fuel slice, Rocket unchanged.
5. Put an item in a newly exposed high index, reduce levels, reopen, and confirm occupied data remains reachable rather than deleted.
6. Receive Poison/Cold/Heat/Drowsy/Spore/Curse. Confirm actual incoming amount awards the matching Tolerance once. Confirm incoming Hunger does not award XP. Recovery/cleanse should award no XP.
7. Gain Petrification from an amulet and a Citadel surface/collision. Confirm each positive gain is reduced and awards Petrification once; clearing it awards nothing.
8. Compare natural Poison/Heat/Drowsy/Spore removal at level 1 and an elevated level. Record Cold warming separately.

## Log interpretation

- `Exposure:<Status>` means incoming affliction and is eligible Resiliency work.
- `HungryMovement` means accepted movement at or above the configured Hunger threshold; incoming `Exposure:Hunger` is diagnostic only and does not award XP.
- `Exposure:Petrify` is normalized from integer petrification points.
- `NaturalRecovery:<Status>` may receive a speed multiplier but never XP.
- `[UsageSkills:Localization] changed locale=<code>` confirms PEAK's selector event reached the mod; missing locale files log an English fallback warning.
- `SubtractLocal`/`SubtractRPC` are diagnostic removal paths and never XP.
- Inventory logs include the concrete `BackpackType`, logical expansion, Strength level, and source.

## Compatibility boundaries

- Built against the current PEAK assembly hash in `HANDOFF.md`; re-inspect hook signatures after a game update, especially both Endurance pass-out targets.
- BackpackCapacity overlaps the same data/wheel behavior and should remain disabled.
- MoreSlots is not required and is not guaranteed compatible.
- PeakStatsEx can display a cached/rounded Weight even when gameplay Weight has refreshed.
- Jet fuel, Rocket behavior, main inventory, and vanilla network ownership are intentional non-interference boundaries.
- Progression is local and persistent, but that architecture is not a substitute for a two-player runtime pass.
