# Elin's PEAK complete handoff

**Updated:** 2026-08-18

**Repository:** <https://github.com/kunrian/Elins-PEAK>

**Source/package version:** 0.3.2

**Baseline merged commit:** `32b6e00` (0.3.2 release merge)

This is the primary handoff for starting a new conversation. Read it with `../AGENTS.md`; do not reconstruct project history from the old logs or the sibling preimplementation specification unless a detail is missing here.

## One-paragraph state

Elin's PEAK is a standalone BepInEx/Harmony mod that gives the local player 21 persistent, use-based skills. Progress is stored outside Gale profiles, the host does not assign levels, Airport XP is disabled, and most effects are applied by transforming current PEAK calculations. The 0.3.2 source compiles, all 50 unit tests pass, and the Thunderstore ZIP was generated successfully. However, the installed devtest copy is still 0.3.1, so the 0.3.2 removal of main slots/Pack Rat, Strength-backed backpack capacity, Cold Recovery correction, and latest XP rates have not yet been tested in a fresh game session. Multiplayer has never been runtime-tested and remains the final testing phase.

## Current release and installation state

| Surface | State |
|---|---|
| GitHub `main` | 0.3.2 source merged |
| Local package | `dist/Elins_PEAK-0.3.2.zip` |
| 0.3.2 ZIP SHA256 | `9626F03481F2F4BD4C53428F0886ED992BC63331437A6BF58FA15FDD181C36A6` (documentation-branch validation build) |
| Thunderstore | 0.3.1 was the last observed published package; 0.3.2 was not uploaded during this work |
| Gale `devtest` | Stale `ChiseledCactusTeam-Elins_PEAK` 0.3.1 installation |
| Gale `Default` | No Elin's PEAK live installation was present in the last audit |
| Progression save | `%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json` |
| Backups | Five rotating JSON files beside the save |

Do not assume a successful package build installed the mod. Do not assume a GitHub merge published Thunderstore. These are separate operations.

## Current game and toolchain fingerprint

- Project root: `C:\Users\Chiseled\Documents\Projects\PEAK\PEAKUsageSkills`
- Reference plugin profile: `C:\Users\Chiseled\AppData\Roaming\com.kesomannen.gale\peak\profiles\Default\BepInEx\plugins`
- Runtime test profile: `C:\Users\Chiseled\AppData\Roaming\com.kesomannen.gale\peak\profiles\devtest`
- Local PEAK: `C:\Program Files (x86)\Steam\steamapps\common\PEAK`
- `Assembly-CSharp.dll` last modified 2026-08-15 and inspected at SHA256 `CAD8EF0702F512F0AD4595F9C169D4025EB8FA351083B64FD4E9FD6F78D5D14C`
- Project targets `netstandard2.1`; tests target .NET 10. The machine has .NET 10 installed.
- README compatibility target: PEAK 2.1.a.

Reinspect the assembly after any PEAK update before trusting private hooks or generated coroutine names.

## Product decisions that are already settled

- Maximum level is 999 by default.
- Next-level XP is `round(100 * level^1.21)`.
- Pause-menu levels use Elin-style `Lv. ##.##`, where the last two digits are floored percent progress to the next level.
- Positive bonuses scale linearly. Reduction effects use the anchored curve defined in `SkillMath.AnchoredReductionMultiplier`: the old level-999 reciprocal target is reached near level 500, then the multiplier approaches 0.001 at level 999.
- Wall, rope, and vine climbing are separate skills.
- Poison, Cold, Heat, Drowsy, and Spores have separate Resistance and natural Recovery skills.
- Sleep maps to PEAK's `Drowsy` status. Zombification exposure maps to `Spores`.
- Hunger uses one Tolerance skill: it gains XP only from movement at 30 or more displayed Hunger and intentionally keeps PEAK's 2.5-point display increments.
- Endurance increases real base stamina capacity and regeneration. It does not provide general stamina-cost reduction.
- Main inventory is vanilla-sized. Strength, not Pack Rat, owns backpack milestones.
- No extra overflow movement or stamina penalty exists.
- Progression/effects are local. The owner explicitly preferred the Atomic Leveling style over host-selected levels for this phase.
- XP never accrues in the Airport. Custom-run XP is off by default.
- Diagnostics use BepInEx logs. Manual data exports are not part of the current workflow.
- Multiplayer testing is last. Anti-farming systems beyond basic validity are deferred.

## Implemented systems

### Progression and persistence

- 21 `SkillId` values with per-skill level, current XP, and lifetime work.
- Atomic JSON save replacement and five rotating backups.
- Save values are clamped on access.
- Retired `PackRat` data is removed during save load.
- Dirty saves flush periodically, on scene load, application quit, and plugin destruction.

### Measurement

- A 0.2-second local sampler measures physical movement from `Character.Center`.
- Deltas above 5 meters are rejected as transitions/teleports.
- Strength uses raw pre-Strength Weight times physical distance.
- Athletics requires grounded intentional horizontal movement.
- Each climbing skill requires its matching state plus intentional input.
- Wet Grip requires slippery wall-climbing work.
- Climbing Tenacity currently requires wall climbing while `Character.GetTotalStamina()` is below 0.20.
- Endurance samples the raw `UseStamina` request before activity-specific efficiency.
- Agility awards only when the local non-pal jump RPC executes.
- Resilience scopes both normal and wall fall-damage paths before Injury is added.
- Resistance XP uses the actual accepted status increase. Hunger XP is kept out of the high-frequency status hook.
- Recovery XP uses actual natural reduction. Direct `Action_ModifyStatus` items are excluded.

### Effects

- Endurance base capacity and scoped regeneration.
- Stamina frame/backing/outline extension above 100 without merging Well Fed/extra stamina into base capacity.
- Vanilla bonus stamina remains spendable when regular stamina empties during sprinting.
- Strength reduces effective Weight and refreshes it on Strength level-up.
- Strength backpack slots at levels 20/40/70/120/200.
- Wall/rope/vine speed and activity-specific stamina efficiency.
- Vine momentum retention by moving slide damping lightly toward 1, capped at 75% retention.
- Athletics ground and additional sprint force plus sprint efficiency.
- Agility jump impulse, jump efficiency, and very light air control.
- Resilience fall Injury reduction.
- Five incoming-condition resistance multipliers and five recovery-speed multipliers.
- Wet Grip reduces slippy downward behavior and wind climbing stamina penalty without reducing Cold itself.
- Climbing Tenacity improves sub-20% climbing control, exhausted slide behavior, and wall stamina cost.

### UI and diagnostics

- Pause UI has red Main Skills, blue Resistance, and green Recovery sections.
- Values refresh once when the pause UI opens, not every 0.25 seconds.
- Hover tooltips explain the skill and current bonus.
- Development level/reset/status buttons were removed for release.
- A config-only compact debug overlay still exists.
- Automatic logs report patch health, snapshots, raw/effective stamina and Weight, status aggregates, fall amounts, save writes, and XP work sources.

## Work history: what was tried

### Initialization and logging

- An early DLL failed to initialize; the error was reviewed and the assembly was replaced only after PEAK was closed.
- Craft PEAK produced an infinite error log and prevented the menu from loading in one test instance. Craft PEAK was disabled and its conflict was deliberately not investigated further.
- A later broad solo run exercised lobby, first biome, climbing, eating, damage, poison, Weight changes, and other status effects. Those logs became the basis for hook discovery and balancing.

### Stamina and HUD

- Vanilla can exceed 100 through lollipop/temporary effects, but its frame does not expand as desired.
- Early frame attempts shifted the green bar outside the frame and allowed status segments to overlap or stop short.
- The current approach derives actual rendered width from Unity rectangles, holds the left edge fixed, expands the frame only from Endurance base capacity, and lets Weight/Hunger/statuses redistribute within that frame.
- The Well Fed/extra stamina pool originally was not used after the regular pool emptied during sprinting. `Character.OutOfRegularStamina` is now patched so vanilla `UseStamina` can roll into bonus stamina. A later runtime pass confirmed the food bar was consumed.
- Runtime tests at saved level and debug levels 50/100/300 showed the frame eventually scaling as intended. Level 999 math is unit-tested, not runtime-tested.

### Weight and status numbers

- Gameplay Weight reduction worked, but the displayed PeakStatsEx Weight number did not reliably refresh or show one decimal place.
- A post-PeakStatsEx text rewrite was attempted. PeakStatsEx owns/caches the label, so exact display remains unresolved and must not become the primary gameplay focus without a dedicated compatibility pass.
- Hunger display was intentionally left at vanilla/PeakStatsEx 2.5-point increments.
- Weight now refreshes on Strength level changes, but the final 0.3.2 behavior still needs a fresh runtime check.

### Pause UI

- Skills were initially tiny and centered over the menu.
- They were moved to side panels and enlarged.
- Debug/test/reset/effect buttons were added during measurement; some overlapped or disappeared due layout assumptions.
- Constant pause-menu refresh was removed at owner request; current values populate once on open.
- Final release layout uses three colored passive panels and hover bubbles. Font size was accepted, but overall panel/window fit was still described as rough and remains polish work.

### Inventory progression

- BackpackCapacity and MoreSlots were inspected as references; the intent was to implement capacity internally rather than depend on them.
- Pack Rat originally unlocked four main slots and five backpack slots, trained from overflow movement, and mitigated added Weight/movement/stamina penalties.
- Variable main arrays, hotbar clones, extra number-key switching, drop handling, backpack serialization, wheel slices, and backpack visuals were implemented.
- Main-inventory expansion later bugged the inventory/UI. The entire main-slot/hotbar path, Pack Rat skill, training, and overflow penalties were removed in 0.3.2.
- Backpack expansion remains internal and is now tied to saved Strength. It never shrinks occupied backpack data.

### Conditions and Cold Recovery

- Poison, Heat, Drowsy, and Spore recovery were observed through `SubtractStatus(..., decreasedNaturally: true)`.
- Cold did not use that normal flag. Assembly inspection showed PEAK naturally warms by adding `Hot`; `AddStatus(Hot)` internally subtracts existing `Cold` with the default non-natural flag.
- 0.3.2 scopes environmental Heat-versus-Cold cancellation as `NaturalRecovery:ColdByWarmth` and applies Cold Recovery there.
- `Action_ModifyStatus.RunAction` establishes an explicit item scope so status-changing items do not gain Recovery XP.
- This correction compiles but has not yet been confirmed in a new runtime log.

### Balance changes

- XP exponent changed to 1.21.
- Strength settled at `raw Weight * distance * 2 XP`.
- Wall/rope/vine settled at 2 XP per intentional meter in any direction.
- Agility settled at 4 XP per executed jump.
- Athletics was reduced 30% to 0.35 XP/m walking and 1.4 XP/m sprinting.
- Hunger Tolerance logged dramatically faster than the other skills and was reduced from 1.0 to 0.1 XP per displayed Hunger-point-meter above the threshold.
- Endurance currently remains 2 XP per normalized raw stamina request, which equals 0.02 XP per displayed stamina point. An earlier plain-language request discussed 1 XP per stamina used; the current normalized implementation should be explicitly accepted or retuned rather than assumed equivalent.

## What has not been tried or remains unverified

- No multiplayer host/client run, reconnect, host migration, mixed-version lobby, or multiple-group progression test.
- No 0.3.2 solo run after removing Pack Rat/main slots.
- No 0.3.2 Cold Recovery environmental-warming versus item-cleanse comparison.
- No validation of every Strength backpack milestone with save/load, wheel, visuals, death/drop, and reconnect.
- No current custom-run XP test.
- No level-999 runtime stress test.
- No systematic compatibility matrix with current PEAK Unlimited, EasyBackpack Fix, ItemStats, Sense of Direction, Piggyback, Atomic Leveling, or PeakStatsEx combinations.
- No anti-farm pass for jump spam, micro-movement, deliberate repeated fall/status farming, or rate caps.
- No performance profile of 2-second diagnostics during long/multiplayer sessions.
- No Thunderstore upload of 0.3.2 during the recorded work.

## Immediate recommended next sequence

1. Close PEAK and replace the stale devtest 0.3.1 package with local 0.3.2 only when explicitly authorized.
2. Back up the progression save and run one clean solo session with diagnostics enabled.
3. Verify startup patch health and confirm `PackRat`/overflow fields no longer appear in fresh logs.
4. Exercise Strength levels 1/20/40/70/120/200 and backpack serialize/wheel/visual behavior.
5. Compare passive environmental warming against a status-changing item and confirm only the first logs `NaturalRecovery:ColdByWarmth` and Cold Recovery XP.
6. Recheck Athletics/Hunger progression rates in a normal-length run.
7. Recheck pause panel fit at common resolutions and PeakStatsEx Weight text behavior.
8. Only after the solo pass, run final multiplayer validation.

## Copy/paste prompt for a new conversation

```text
Continue work on Elin's PEAK in C:\Users\Chiseled\Documents\Projects\PEAK\PEAKUsageSkills.
Read AGENTS.md and docs/HANDOFF.md completely, then check git status and docs/TESTING_AND_COMPATIBILITY.md.
Treat current source/runtime behavior as authoritative over the old PEAK_UsageSkills_Spec folder.
Do not deploy to Gale, reset saves, publish GitHub, or upload Thunderstore unless I explicitly ask.
Before changing a PEAK hook, inspect the current Assembly-CSharp.dll and update docs/ARCHITECTURE_AND_HOOKS.md.
```
