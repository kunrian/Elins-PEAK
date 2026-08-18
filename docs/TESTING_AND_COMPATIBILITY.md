# Testing and compatibility

## Evidence state

| Area | State |
|---|---|
| 0.3.2 solo runtime | Passed in `devtest` on 2026-08-18; startup/UI buttons/Backpack milestones/reset produced expected logs with no plugin exception. |
| 0.4.1 Release compile | Passed with zero warnings/errors. |
| 0.4.1 package structure | Passed; exact artifact is recorded in `DEVELOPMENT_AND_RELEASE.md`. |
| 0.4.1 live runtime | Pending the user's next run. |
| Multiplayer | Never runtime-tested. |
| Cold natural warming speed | Open; latest log classified it as `SubtractLocal`. |

The full unit suite is intentionally not required for this balance/runtime pass. Build the solution to compile both projects, validate package structure, and use the focused live sequence below.

## Focused 0.4.1 live sequence

1. Confirm `Elin's PEAK 0.4.1 loaded` and healthy hook lines for Afflictions, Petrification, Backpack Inventory, Backpack Wheel, and Backpack Visuals.
2. Confirm ESC layout: Main and blue Resiliency on the left, Vitality in Main, and short names for all eight Resiliency skills.
3. Confirm the previous Resilience level/XP appears under Vitality and the old save/config keys are gone after a save.
4. Test typed storage at one milestone and at +5: Backpack `4+n`, Fanny `2+n`, Jet `1+n` item slices plus exactly one fuel slice, Rocket unchanged.
5. Put an item in a newly exposed high index, reset levels, reopen, and confirm occupied data remains reachable rather than deleted.
6. Receive Poison/Cold/Heat/Drowsy/Spore/Hunger/Curse. Confirm actual incoming amount awards the matching Tolerance once. Recovery/cleanse should award no XP.
7. Gain Petrification from an amulet and a Citadel surface/collision. Confirm each positive gain awards Petrification once; clearing it awards nothing.
8. Compare natural Poison/Heat/Drowsy/Spore removal at level 1 and an elevated level. Record Cold warming separately.
9. Compare standing and full-run jumps. Sprint horizontal velocity should carry through the vertical jump impulse.

## Log interpretation

- `Exposure:<Status>` means incoming affliction and is eligible Resiliency work.
- `Exposure:Petrify` is normalized from integer petrification points.
- `NaturalRecovery:<Status>` may receive a speed multiplier but never XP in 0.4.1.
- `SubtractLocal`/`SubtractRPC` are diagnostic removal paths and never XP.
- Inventory logs include the concrete `BackpackType`, logical expansion, Strength level, and source.

## Compatibility boundaries

- Built against PEAK `2.1.a` and the exact assembly hash in `HANDOFF.md`; re-inspect hook signatures after a game update.
- BackpackCapacity overlaps the same data/wheel behavior and should remain disabled.
- MoreSlots is not required and is not guaranteed compatible.
- PeakStatsEx can display a cached/rounded Weight even when gameplay Weight has refreshed.
- Jet fuel, Rocket behavior, main inventory, and vanilla network ownership are intentional non-interference boundaries.
- Progression is local and persistent, but that architecture is not a substitute for a two-player runtime pass.
