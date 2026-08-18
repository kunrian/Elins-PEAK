# Testing and compatibility

## Evidence state

| Area | State |
|---|---|
| 0.3.2 solo runtime | Passed in `devtest` on 2026-08-18; startup/UI buttons/Backpack milestones/reset produced expected logs with no plugin exception. |
| 0.4.2 Release compile | Passed with zero warnings/errors. |
| 0.4.2 package structure/localization catalogs | Passed; all six 36-key catalogs and localized READMEs are present in the exact artifact recorded in `DEVELOPMENT_AND_RELEASE.md`. |
| Config schema 6 migration | Compile/package verified; fresh-config and prior-default migration remain runtime-pending. |
| 0.4.1 live log | Plugin loaded, but Petrification was missing because Harmony found both `AddPetrify(int)` and the parameterless console-command overload. |
| 0.4.2 live runtime | Pending the user's next run. |
| Multiplayer | Never runtime-tested. |
| Cold natural warming speed | Open; latest log classified it as `SubtractLocal`. |

The full unit suite is intentionally not required for this balance/runtime pass. Build the solution to compile both projects, validate package structure, and use the focused live sequence below.

## Focused 0.4.2 live sequence

1. Confirm `Elin's PEAK 0.4.2 loaded`, each locale file reports 36 registered keys, and Petrification is healthy at `CharacterAfflictions.AddPetrify(Int32)`.
2. Confirm the English ESC layout, then change PEAK's language to French, German, Spanish Spain, Spanish LatAm, Simplified Chinese, Japanese, and Korean. Names, headings, and tooltips should refresh immediately without reopening the game.
3. Check CJK glyphs and tooltip wrapping. Then select at least one untranslated language (for example Italian or Traditional Chinese) and confirm clean English fallback instead of `LOC:` keys.
4. Confirm the previous Resilience level/XP appears under Vitality, the old save/config keys are gone after a save, and the generated config reports schema 6 with the documented XP defaults.
5. Test typed storage at one milestone and at +5: Backpack `4+n`, Fanny `2+n`, Jet `1+n` item slices plus exactly one fuel slice, Rocket unchanged.
6. Put an item in a newly exposed high index, reduce levels, reopen, and confirm occupied data remains reachable rather than deleted.
7. Receive Poison/Cold/Heat/Drowsy/Spore/Hunger/Curse. Confirm actual incoming amount awards the matching Tolerance once. Recovery/cleanse should award no XP.
8. Gain Petrification from an amulet and a Citadel surface/collision. Confirm each positive gain is reduced and awards Petrification once; clearing it awards nothing.
9. Compare natural Poison/Heat/Drowsy/Spore removal at level 1 and an elevated level. Record Cold warming separately.
10. Compare standing and full-run jumps. Sprint horizontal velocity should carry through the vertical jump impulse.

## Log interpretation

- `Exposure:<Status>` means incoming affliction and is eligible Resiliency work.
- `Exposure:Petrify` is normalized from integer petrification points.
- `NaturalRecovery:<Status>` may receive a speed multiplier but never XP in 0.4.2.
- `[UsageSkills:Localization] changed locale=<code>` confirms PEAK's selector event reached the mod; missing locale files log an English fallback warning.
- `SubtractLocal`/`SubtractRPC` are diagnostic removal paths and never XP.
- Inventory logs include the concrete `BackpackType`, logical expansion, Strength level, and source.

## Compatibility boundaries

- Built against PEAK `2.1.a` and the exact assembly hash in `HANDOFF.md`; re-inspect hook signatures after a game update.
- BackpackCapacity overlaps the same data/wheel behavior and should remain disabled.
- MoreSlots is not required and is not guaranteed compatible.
- PeakStatsEx can display a cached/rounded Weight even when gameplay Weight has refreshed.
- Jet fuel, Rocket behavior, main inventory, and vanilla network ownership are intentional non-interference boundaries.
- Progression is local and persistent, but that architecture is not a substitute for a two-player runtime pass.
