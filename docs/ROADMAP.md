# Roadmap and open work

This roadmap starts from source/package version 0.3.2. It separates required validation from optional expansion so a new conversation can make progress without reopening retired designs.

## Priority 0 — establish a trustworthy 0.3.2 runtime baseline

- Install 0.3.2 into `devtest`; the current installed copy is still 0.3.1.
- Run the focused solo checklist in [`TESTING_AND_COMPATIBILITY.md`](TESTING_AND_COMPATIBILITY.md).
- Confirm the final Strength backpack milestones, including save/load with occupied high slots.
- Confirm environmental warmth trains and accelerates Cold Recovery while direct item cleansing does not.
- Confirm no main-inventory expansion, Pack Rat row/data, or overflow penalties remain.
- Validate the reduced Hunger and Athletics XP rates from fresh logs.
- Save representative 0.3.2 logs before changing hooks or balance again.

## Priority 1 — known quality and balance work

- Refine the red/blue/green pause-menu panel bounds and tooltip placement at different resolutions/UI scales. Font size is currently acceptable; area/layout remains rough.
- Decide whether Endurance should award one XP per displayed stamina used, or keep the current normalized-unit calculation. The config description and earlier owner wording are not yet fully aligned.
- Decide whether Resistance training should use raw exposure or actual post-mitigation gain. Actual gain is implemented and naturally slows high-level training.
- Investigate PeakStatsEx's Weight-label caching only after gameplay systems are stable. Gameplay Weight is already affected; the one-decimal label is a compatibility/presentation issue.
- Make the debug level override exercise backpack milestones too, or add a safe non-release test mechanism. It currently affects effect multipliers but backpack capacity reads saved Strength.
- Re-evaluate very high-level positive bonuses in live play. Linear scaling to 999 is intentional, but jump/sprint/climb multipliers may become extreme long before the cap.

## Priority 2 — final compatibility and multiplayer phase

- Test Piggyback plus Strength Weight scaling.
- Test PeakStatsEx across changing Endurance/status combinations and document supported versions.
- Test EasyBackpack Fix and decide whether explicit Harmony ordering is necessary.
- Keep BackPackCapacity disabled unless deliberately implementing a compatibility adapter.
- Reproduce Craft PEAK's menu/error failure only if compatibility becomes a release priority; it is not a current blocker.
- Run local-player ownership tests with different host/client save levels. Add networking only if actual game authority creates incorrect results.
- Profile diagnostics, pause UI, inventory controller, and activity sampler in a longer session.

## Deferred systems

- Anti-farming beyond present validity checks: per-source rate limits, repeated-route diminishing returns, or other cheese prevention. The owner explicitly deferred this while the skill set was expanding.
- Manual import/export. The current requirement is ordinary local saves and BepInEx logging.
- Host-canonical levels/config. This conflicts with the accepted player-owned model.
- Main inventory/hotbar expansion. The first implementation bugged the live inventory and was removed.
- Pack Rat and per-overflow-item Weight/movement/stamina penalties. Retired in favor of Strength-based backpack capacity.
- Thunderstore publication of 0.3.2. GitHub and Thunderstore are separate operations; the last observed public Thunderstore build was 0.3.1.

## Candidate future skills/options

These are possibilities, not approved implementation requirements. Each needs a game-hook check, an XP event that cannot be trivially farmed, a meaningful effect, and an owner decision.

| Candidate | Possible training/effect | Main question |
|---|---|---|
| Additional status Resistance/Recovery | Curse, Web, Crab, Fly Trap, Petrify, or other assembly statuses | Does the status use a stable timer/gain/removal hook, and is natural recovery distinct from item removal? |
| General medical recovery | Natural Injury/HP recovery | Resilience already owns fall Injury; avoid overlap and item-heal farming. |
| Fall technique extension | Landing control or non-damage fall behavior | Fall damage reduction is already Resilience; only add this if a distinct mechanic exists. |
| Rope handling specialization | Reeling/control rather than raw travel speed | Confirm actual rope force/reel fields in the current assembly and whether the existing Rope Climbing effect already covers them. |
| Vine control specialization | Directional damping/launch control | Current Vine Climbing already reduces velocity damping; determine whether a separate skill adds value. |
| Hunger recovery | Faster Hunger timer reduction | Hunger normally increases and has no equivalent natural-recovery loop; likely not appropriate. |

Drowsy already represents sleep-like status and Spores already represent zombification-like exposure, so duplicate Sleep or Zombie skills should not be added.

## Definition of release-ready for the next version

- Clean build, all tests pass, and package validation succeeds.
- Fresh log proves the package version and contains no repeating patch exceptions.
- Every changed gameplay hook has a focused solo runtime test.
- Save upgrade/downgrade risk is documented and occupied items are protected.
- Player-facing README/changelog match behavior.
- Multiplayer is either tested or explicitly declared unsupported/experimental.
- GitHub release/source and Thunderstore package version, if published, use the same manifest and icon.
