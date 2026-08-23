# Testing and compatibility

## Evidence state

| Area | State |
|---|---|
| 0.3.2 solo runtime | Passed in `devtest` on 2026-08-18; startup/UI buttons/Backpack milestones/reset produced expected logs with no plugin exception. |
| Published 0.4.5 gameplay build | Solo `devtest` log on 2026-08-19 loaded all six 36-key catalogs, resolved Petrification and both pass-out hooks, and observably suppressed vanilla pass-out below the expanded Endurance threshold. The DLL identity remained 0.4.2 by mistake. |
| Corrected 0.4.5 Release compile/package | Passed on 2026-08-23 with zero warnings/errors; manifest, plugin, project, and assembly versions all report 0.4.5. Exact hashes are in `DEVELOPMENT_AND_RELEASE.md`. |
| Config schema 6 migration | Compile/package verified; fresh-config and prior-default migration remain runtime-pending. |
| 0.4.1 live log | Plugin loaded, but Petrification was missing because Harmony found both `AddPetrify(int)` and the parameterless console-command overload. |
| Corrected 0.4.5 identity startup | Pending; the corrected DLL/package was not deployed during source realignment. |
| 0.4.4 modded-scene eligibility | Implementation no longer requires `Level_*`; TerrainRandomiser 1.1.10 co-loaded without an Elin's PEAK patch exception. A deliberately renamed gameplay-scene comparison remains optional follow-up. |
| Multiplayer | Never runtime-tested. |
| Cold natural warming speed | Open; latest log classified it as `SubtractLocal`. |

The full unit suite is intentionally not required for this metadata/source-reconciliation pass. Build the solution to compile both projects, validate package structure, and use the focused live sequence below.

## Focused 0.4.5 live sequence

1. When the corrected artifact is intentionally deployed, confirm BepInEx and the plugin line both report `Elin's PEAK 0.4.5`, each locale file reports 36 registered keys, and all patch-health entries remain healthy.
2. Confirm the English ESC layout, then change PEAK's language to French, German, Spanish Spain, Spanish LatAm, Simplified Chinese, Japanese, and Korean. Names, headings, and tooltips should refresh immediately without reopening the game.
3. Check CJK glyphs and tooltip wrapping. Then select at least one untranslated language (for example Italian or Traditional Chinese) and confirm clean English fallback instead of `LOC:` keys.
4. Confirm the previous Resilience level/XP appears under Vitality, the old save/config keys are gone after a save, and the generated config reports schema 6 with the documented XP defaults.
5. Test typed storage at one milestone and at +5: Backpack `4+n`, Fanny `2+n`, Jet `1+n` item slices plus exactly one fuel slice, Rocket unchanged.
6. Put an item in a newly exposed high index, reduce levels, reopen, and confirm occupied data remains reachable rather than deleted.
7. Receive Poison/Cold/Heat/Drowsy/Spore/Hunger/Curse. Confirm actual incoming amount awards the matching Tolerance once. Recovery/cleanse should award no XP.
8. Gain Petrification from an amulet and a Citadel surface/collision. Confirm each positive gain is reduced and awards Petrification once; clearing it awards nothing.
9. Compare natural Poison/Heat/Drowsy/Spore removal at level 1 and an elevated level. Record Cold warming separately.
10. At a low non-level-1 Endurance threshold, fill vanilla affliction to 1.0 and confirm the player does not pass out until the expanded threshold is reached. One focused pass is sufficient; do not repeat this across dozens of levels.

## Log interpretation

- `Exposure:<Status>` means incoming affliction and is eligible Resiliency work.
- `Exposure:Petrify` is normalized from integer petrification points.
- `NaturalRecovery:<Status>` may receive a speed multiplier but never XP.
- `[UsageSkills:Localization] changed locale=<code>` confirms PEAK's selector event reached the mod; missing locale files log an English fallback warning.
- `SubtractLocal`/`SubtractRPC` are diagnostic removal paths and never XP.
- Inventory logs include the concrete `BackpackType`, logical expansion, Strength level, and source.

## Compatibility boundaries

- Built against PEAK `2.1.a` and the exact assembly hash in `HANDOFF.md`; re-inspect hook signatures after a game update, especially both Endurance pass-out targets.
- BackpackCapacity overlaps the same data/wheel behavior and should remain disabled.
- MoreSlots is not required and is not guaranteed compatible.
- PeakStatsEx can display a cached/rounded Weight even when gameplay Weight has refreshed.
- Jet fuel, Rocket behavior, main inventory, and vanilla network ownership are intentional non-interference boundaries.
- Progression is local and persistent, but that architecture is not a substitute for a two-player runtime pass.
