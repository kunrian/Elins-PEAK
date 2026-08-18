# Elin's PEAK handoff

**Date:** 2026-08-18

**Source/package version:** 0.4.2

**Game assembly inspected:** PEAK `Assembly-CSharp.dll` SHA-256 `CAD8EF0702F512F0AD4595F9C169D4025EB8FA351083B64FD4E9FD6F78D5D14C`

**0.4.2 package:** `dist/Elins_PEAK-0.4.2.zip`, SHA-256 `590A350E9A7E78D292B9010354C85D109EE065B05F2FF744FDF67416C55BD1A9`

## Where the project is

Elin's PEAK is a local-player, use-based BepInEx/Harmony progression mod with 18 persistent skills and a default maximum level of 999. Version 0.3.2 received a successful solo runtime pass in the Gale `devtest` profile: the plugin loaded, the pause-menu test controls worked, Strength expanded the ordinary Backpack at the expected milestones, reset worked, and no plugin exception appeared. That supersedes older documentation which called the live profile stale.

Version 0.4.2 is the current implementation. It compiles and packages with zero warnings/errors, but has not yet been approved by a fresh in-game log. Multiplayer remains untested.

## 0.4.2 behavior

- Main Skills: Strength, Endurance, Wall Climbing, Rope Climbing, Vine Climbing, Athletics, Agility, Vitality, Wet Grip, and Climbing Tenacity.
- Blue Resiliency skills use the short names Poison, Cold, Heat, Drowsy, Spores, Hunger, Curse, and Petrification.
- Existing `Resilience` save progression and custom XP/effect settings migrate to Vitality once, then the legacy keys are removed.
- The five former Recovery skills are retired. On first 0.4.0 load, their complete accumulated XP and lifetime work are merged into the matching Tolerance, then the old save keys are removed.
- A Tolerance gets XP only when the matching affliction increases. Natural recovery, item cleansing, and hungry movement do not grant Resiliency XP.
- Poison/Cold/Heat/Drowsy/Spore Tolerance each applies incoming reduction and natural-recovery acceleration. Curse, Hunger, and Petrification have no recovery-speed effect.
- Both condition bonuses were halved from 0.3% to 0.15% per level because one skill now owns both benefits.
- Curse is handled through `CharacterAfflictions.AddStatus`. Petrification is handled once at `CharacterAfflictions.AddPetrify(int)`, the shared positive-gain path used by status additions, amulet actions, and Citadel climb/collision modifiers.
- Main inventory stays vanilla. Strength adds +1/+2/+3/+4/+5 item slots at levels 20/40/70/120/200 to Backpack (base 4), Fanny Pack (base 2), and Jet Pack (base 1). Jet fuel is a separate data entry/UI slice and is untouched. Rocket Pack is explicitly excluded.
- The blue Resiliency panel is below Main Skills on the left.
- The skill panel registers its English strings and six supplied JSON catalogs through PEAKLib UI's wrapper over PEAK's `LocalizedText` table. French, German, Spanish Spain/LatAm, Simplified Chinese, Japanese, and Korean render translated names/tooltips; all other PEAK languages and missing keys use English.
- `LocalizedText.OnLangugageChanged` refreshes section titles, skill rows, and tooltip state immediately after the game language changes. There is no separate mod-language setting and no restart requirement.
- PEAK has both `AddPetrify(int petrify)` and a parameterless console-command overload. The 0.4.1 annotation was ambiguous and disabled the adapter; 0.4.2 targets the integer overload explicitly and records that exact signature in patch health.

## 0.4.2 default XP rates

| Skill/source | XP rate |
|---|---:|
| Endurance | 10 per normalized stamina requested |
| Strength | 2 per raw Weight × meter |
| Wall/Rope/Vine Climbing | 8 per intentional meter |
| Athletics walk / sprint | 0.22 / 1.05 per meter |
| Agility | 8 per successful jump |
| Vitality | 100 per normalized fall Injury |
| Resiliency | 100 per normalized actual incoming affliction |
| Wet Grip | 20 per slippery weighted wall meter |
| Climbing Tenacity | 40 per low-stamina wall meter |

Config schema 6 changes only values still equal to the prior 0.4.1 defaults; user-tuned values are preserved. Obsolete recovery-XP, hungry-movement, and Pack Rat entries are removed from the generated config.

## Verified assembly findings

### Sprint momentum and jumping

`CharacterMovement.GetMovementForce` adds grounded movement/sprint force. `JumpRpc` adds a vertical impulse but does not clear or replace horizontal velocity. Existing horizontal sprint momentum therefore carries into the jump. Athletics is an acceleration/force modifier, not a direct jump-distance multiplier, and its extra ground force stops once airborne; short run-ups and drag can make the distance difference subtle. The report that sprint velocity is discarded at jump time is not supported by the inspected code.

### Glider, parachute, and balloons

`Glider.FixedUpdate` pays opening/per-frame stamina and calls `CharacterMovement.ApplyGlider`, which applies fall drag and forward force. Balloons instead modify gravity and jump multipliers. The parachute uses the parasol-drag path. They are related by airborne traversal but are not one shared mechanic. A future **Aeronautics** skill is viable, but its scope must be chosen explicitly: glider-only is the cleanest hook; glider/parachute/balloons would require separate adapters and balance rules.

### Throwing

`CharacterItems.DropItemRpc` converts a 0..1 throw charge into force, then multiplies it by the item's own `throwForceMultiplier`. A future **Throwing** skill can train from valid charged local throws and scale the computed impulse while preserving item-specific behavior. It should not rewrite `throwCharge`, because PEAK also uses that value for thrown-data/events.

### Petrification

`CharacterAfflictions.AddStatus(Petrify, amount)` converts normalized status to integer points and calls `AddPetrify(int)`. Citadel climb/collision hazards, scout amulets, and other amulet behaviors also call the integer overload directly. The parameterless `AddPetrify()` method is only a console command. Patching the integer method once covers real positive gain sources without double-applying the normal status path; negative removal is ignored for XP/effect.

## Known open issue

Cold exposure XP and incoming Cold reduction work through `AddStatus`. The recovery-speed half remains unverified because the latest log showed ordinary warming as `Cold:SubtractLocal`, not PEAK's `decreasedNaturally` path, and the prior Heat scope did not activate. Do not reintroduce Recovery XP to solve this. The next fix should first identify the actual warming caller and apply only the Cold Tolerance recovery multiplier there.

## Minimal next runtime pass

1. Confirm startup reports Elin's PEAK 0.4.2, six localization files register 36 keys, and Petrification is healthy at `CharacterAfflictions.AddPetrify(Int32)`.
2. Open ESC, then change PEAK's language among French, German, both Spanish choices, Simplified Chinese, Japanese, and Korean. Section titles, skill rows, and new tooltips should update without a restart; CJK text should wrap and render with the game's language fonts.
3. Select Italian, Portuguese (Brazil), Russian, Ukrainian, Traditional Chinese, Polish, or Turkish and confirm the mod UI falls back to English rather than showing missing localization keys.
4. Confirm existing Resilience progression appears under Vitality and the generated config contains Vitality rather than Resilience settings.
5. At Strength milestone levels, open a Backpack, Fanny Pack, and Jet Pack. Confirm item capacities are base + milestone, Jet has exactly one separate fuel slot, and Rocket Pack is unchanged.
6. Receive Curse and Petrification through real gameplay. Test both an amulet and Citadel hazard; each positive Petrification gain should award XP once and be reduced at elevated levels.
7. Spot-check one natural Poison/Heat/Drowsy/Spore recovery for speed but no XP. Capture Cold warming separately for the outstanding hook investigation.
8. Compare a standing jump with a full sprint jump on flat ground using the same Agility level; horizontal momentum should remain.

Do not run the full unit suite for this balance/runtime cycle unless a core math change warrants it. A Release solution build and package validation are sufficient before the live pass.

## Publication state

An earlier 0.4.1 payload is installed in `devtest`. The 0.4.2 package has not been deployed or runtime-tested, so the installed and current source DLLs do not match. The previous 0.4.0 live folder is recoverable from `dist/Elins_PEAK-live-before-0.4.1-20260818.zip`. Git publication is authorized by the owner; Thunderstore upload remains a separate action.
