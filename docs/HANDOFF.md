# Elin's PEAK handoff

**Date:** 2026-08-18

**Source/package version:** 0.4.1

**Game assembly inspected:** PEAK `Assembly-CSharp.dll` SHA-256 `CAD8EF0702F512F0AD4595F9C169D4025EB8FA351083B64FD4E9FD6F78D5D14C`

**0.4.1 package:** `dist/Elins_PEAK-0.4.1.zip`, SHA-256 `5A6E3C98F3A855EE1C20995601A4CF3C2983802EC000E0126455FD843B415393`

## Where the project is

Elin's PEAK is a local-player, use-based BepInEx/Harmony progression mod with 18 persistent skills and a default maximum level of 999. Version 0.3.2 received a successful solo runtime pass in the Gale `devtest` profile: the plugin loaded, the pause-menu test controls worked, Strength expanded the ordinary Backpack at the expected milestones, reset worked, and no plugin exception appeared. That supersedes older documentation which called the live profile stale.

Version 0.4.1 is the current implementation. It compiles and packages with zero warnings/errors, but has not yet been approved by a fresh in-game log. Multiplayer remains untested.

## 0.4.1 behavior

- Main Skills: Strength, Endurance, Wall Climbing, Rope Climbing, Vine Climbing, Athletics, Agility, Vitality, Wet Grip, and Climbing Tenacity.
- Blue Resiliency skills use the short names Poison, Cold, Heat, Drowsy, Spores, Hunger, Curse, and Petrification.
- Existing `Resilience` save progression and custom XP/effect settings migrate to Vitality once, then the legacy keys are removed.
- The five former Recovery skills are retired. On first 0.4.0 load, their complete accumulated XP and lifetime work are merged into the matching Tolerance, then the old save keys are removed.
- A Tolerance gets XP only when the matching affliction increases. Natural recovery, item cleansing, and hungry movement do not grant Resiliency XP.
- Poison/Cold/Heat/Drowsy/Spore Tolerance each applies incoming reduction and natural-recovery acceleration. Curse, Hunger, and Petrification have no recovery-speed effect.
- Both condition bonuses were halved from 0.3% to 0.15% per level because one skill now owns both benefits.
- Curse is handled through `CharacterAfflictions.AddStatus`. Petrification is handled once at `CharacterAfflictions.AddPetrify`, the shared positive-gain path used by status additions, amulet actions, and Citadel climb/collision modifiers.
- Main inventory stays vanilla. Strength adds +1/+2/+3/+4/+5 item slots at levels 20/40/70/120/200 to Backpack (base 4), Fanny Pack (base 2), and Jet Pack (base 1). Jet fuel is a separate data entry/UI slice and is untouched. Rocket Pack is explicitly excluded.
- The blue Resiliency panel is below Main Skills on the left.

## 0.4.1 default XP rates

| Skill/source | XP rate |
|---|---:|
| Endurance | 6 per normalized stamina requested |
| Strength | 2 per raw Weight × meter |
| Wall/Rope/Vine Climbing | 4 per intentional meter |
| Athletics walk / sprint | 0.28 / 1.12 per meter |
| Agility | 3.2 per successful jump |
| Vitality | 100 per normalized fall Injury |
| Resiliency | 100 per normalized actual incoming affliction |
| Wet Grip | 6 per slippery weighted wall meter |
| Climbing Tenacity | 6 per low-stamina wall meter |

Config migration changes only values still equal to the old defaults; user-tuned values are preserved. Obsolete recovery-XP, hungry-movement, and Pack Rat entries are removed from the generated config.

## Verified assembly findings

### Sprint momentum and jumping

`CharacterMovement.GetMovementForce` adds grounded movement/sprint force. `JumpRpc` adds a vertical impulse but does not clear or replace horizontal velocity. Existing horizontal sprint momentum therefore carries into the jump. Athletics is an acceleration/force modifier, not a direct jump-distance multiplier, and its extra ground force stops once airborne; short run-ups and drag can make the distance difference subtle. The report that sprint velocity is discarded at jump time is not supported by the inspected code.

### Glider, parachute, and balloons

`Glider.FixedUpdate` pays opening/per-frame stamina and calls `CharacterMovement.ApplyGlider`, which applies fall drag and forward force. Balloons instead modify gravity and jump multipliers. The parachute uses the parasol-drag path. They are related by airborne traversal but are not one shared mechanic. A future **Aeronautics** skill is viable, but its scope must be chosen explicitly: glider-only is the cleanest hook; glider/parachute/balloons would require separate adapters and balance rules.

### Throwing

`CharacterItems.DropItemRpc` converts a 0..1 throw charge into force, then multiplies it by the item's own `throwForceMultiplier`. A future **Throwing** skill can train from valid charged local throws and scale the computed impulse while preserving item-specific behavior. It should not rewrite `throwCharge`, because PEAK also uses that value for thrown-data/events.

## Known open issue

Cold exposure XP and incoming Cold reduction work through `AddStatus`. The recovery-speed half remains unverified because the latest log showed ordinary warming as `Cold:SubtractLocal`, not PEAK's `decreasedNaturally` path, and the prior Heat scope did not activate. Do not reintroduce Recovery XP to solve this. The next fix should first identify the actual warming caller and apply only the Cold Tolerance recovery multiplier there.

## Minimal next runtime pass

1. Confirm startup reports Elin's PEAK 0.4.1 and all hook-health entries, including Petrification and Backpack Wheel, are healthy.
2. Open ESC: Main Skills and blue Resiliency should be stacked on the left, with Vitality and all eight short Resiliency names visible.
3. Confirm existing Resilience progression appears under Vitality and the generated config contains Vitality rather than Resilience settings.
4. At Strength milestone levels, open a Backpack, Fanny Pack, and Jet Pack. Confirm item capacities are base + milestone, Jet has exactly one separate fuel slot, and Rocket Pack is unchanged.
5. Receive Curse and Petrification through real gameplay. Confirm XP is awarded once and the received amount is reduced at elevated levels.
6. Spot-check one natural Poison/Heat/Drowsy/Spore recovery for speed but no XP. Capture Cold warming separately for the outstanding hook investigation.
7. Compare a standing jump with a full sprint jump on flat ground using the same Agility level; horizontal momentum should remain.

Do not run the full unit suite for this balance/runtime cycle unless a core math change warrants it. A Release solution build and package validation are sufficient before the live pass.

## Publication state

The exact 0.4.1 payload is installed in `devtest`; PEAK was closed and the installed/source DLL hashes match. The previous 0.4.0 live folder is recoverable from `dist/Elins_PEAK-live-before-0.4.1-20260818.zip`. The local Git repository may be updated and committed, but no remote push or Thunderstore upload is authorized by this handoff.
