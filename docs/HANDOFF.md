# Elin's PEAK handoff

**Date:** 2026-08-23

**Current source/package version:** 0.4.7

**Current Gale `devtest` version:** 0.4.6 (owner-validated)

**Current game assembly inspected:** PEAK `Assembly-CSharp.dll` SHA-256 `B5C1B8B73F82CCA847942B0BE758DD554FE347D1A7C89C69E292FAFB115BFFFD`

**0.4.7 ZIP:** `dist/Elins_PEAK-0.4.7.zip`, SHA-256 `EC2235040EC2329979D8A589C396CBE73C549DCD6EC0E4F8903BC7B2818AAEEE`

**0.4.7 DLL:** SHA-256 `8A64F9064A1F39A3F5CEE170FB2AED5ED46FE4FF16C421BAD5146A18410D89DF`

## Where the project is

Elin's PEAK is a local-player, use-based BepInEx/Harmony progression mod with 18 persistent skills and a default maximum level of 999. Version 0.4.7 is the current source, packaged artifact, and GitHub release target. It has not been uploaded to Thunderstore and has not replaced the owner-validated 0.4.6 Gale install.

Thunderstore remains on 0.4.5. That published payload contains the intended 0.4.4/0.4.5 gameplay code but its DLL identity was accidentally left at 0.4.2. Version 0.4.6 corrected the identity, restored vanilla sprint stopping at regular-stamina exhaustion, and adopted the owner's active Endurance/sprinting-Athletics XP values. The owner confirmed that live behavior on 2026-08-23.

## 0.4.7 changes

- Restored movement-based Hunger training and removed incoming-Hunger XP. Incoming Hunger still receives Hunger's tolerance reduction.
- Hunger movement training now begins at 10 displayed Hunger (`HungerTrainingThreshold = 0.10`) instead of 30.
- Reduced the per-Hunger-point, per-movement-meter rate from `0.1` to `0.02`, exactly one-fifth of the former rate.
- Restored teleport/respawn rejection for Hunger work so large nonphysical movement deltas cannot train it.
- Config schema 8 migrates only the former exact Hunger defaults (`0.1→0.02`, `0.30→0.10`) and preserves other custom values.
- Retained the validated 0.4.6 sprint boundary and XP defaults unchanged.

## Current XP defaults

| Skill/source | XP rate |
|---|---:|
| Endurance | 20 per normalized stamina requested |
| Strength | 2 per raw Weight × meter |
| Wall/Rope/Vine Climbing | 8 per intentional meter |
| Athletics walk / sprint | 0.22 / 0.9 per meter |
| Agility | 8 per successful jump |
| Vitality | 100 per normalized fall Injury |
| Resiliency except Hunger | 100 per normalized actual incoming affliction |
| Hunger | 0.02 per displayed Hunger point × meter at Hunger 10+ |
| Wet Grip | 20 per slippery weighted wall meter |
| Climbing Tenacity | 40 per low-stamina wall meter |

## Validation evidence

- The 0.4.6 live log reports the correct plugin version, all six 36-key localization catalogs, healthy expected hooks, no `BonusSprint` hook, and successful config schema 7 initialization. The owner confirmed sprinting stops without consuming bonus/food stamina.
- `scripts/Build-Package.ps1` completed for 0.4.7 with zero warnings/errors and validated version parity, localization catalogs, the 256×256 icon, compiled assembly version `0.4.7.0`, and archive layout.
- The focused `HungerMovementStartsAtTenDisplayedPoints` test passed all four cases: below threshold, exact threshold, Hunger 30, and Hunger 75.
- Root/package changelogs and all seven README variants are byte-identical to their packaged copies.
- The exact 0.4.7 artifact has not received a gameplay runtime pass yet.

## Focused 0.4.7 runtime pass

1. Install the exact 0.4.7 package only after PEAK is closed, then confirm the startup line reports 0.4.7 and expected hooks remain healthy.
2. Confirm config schema 8 contains Hunger rate `0.02` and threshold `0.10` without replacing other custom values.
3. Move below Hunger 10 and confirm Hunger XP does not change.
4. Move the same approximate distance at Hunger 10, 30, and a higher value. Confirm `HungryMovement` XP scales at 0.02 per displayed Hunger point-meter.
5. Let Hunger increase while stationary. Confirm its resistance effect remains active but the incoming gain itself awards no Hunger XP.
6. Confirm teleport, respawn, and scene-transition deltas do not award Hunger work.

## Known open work

- Cold recovery-speed handling still needs the actual environmental warming caller identified. Do not restore Recovery XP and do not multiply item cleansing.
- Real gameplay Petrification from both an amulet and a Citadel hazard is not evidenced in the current log, even though the exact hook is healthy.
- Live language switching/fallback and CJK wrapping still need an explicit UI pass.
- Multiplayer remains untested.

## Publication and deployment state

- GitHub `main`: 0.4.7 source/docs are the current release state.
- Thunderstore: remains `ChiseledCactusTeam-Elins_PEAK-0.4.5`, published 2026-08-19; no 0.4.7 upload was requested.
- Gale `devtest`: remains on owner-validated 0.4.6; no 0.4.7 live deployment was requested.
- Previous live backup: `2026-08-18/dist/Elins_PEAK-live-before-0.4.6-20260823.zip`, SHA-256 `272F34BBDC0FE5B878144791269478F54ABAF77ECEDC10B6D8D0FAFB984A0A4A`.
