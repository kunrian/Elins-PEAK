# Skills and balance reference

This document describes the defaults implemented in source version 0.3.2. The generated BepInEx configuration can override most XP and effect rates, so runtime reports should include the active config when values differ.

## Shared progression

- Every skill begins at level 1 with zero XP.
- The default maximum is level 999.
- XP required for the next level is `round(100 * level^1.21)`.
- The pause menu displays `Lv. ##.##`; the final two digits are the floored percentage toward the next level, clamped to 00–99.
- XP is local, saved per installation, and disabled in the Airport/lobby. Custom-run XP is disabled by default.
- Movement work is sampled every 0.2 seconds from `Character.Center`. Samples over 5 meters are treated as teleports/scene changes, and movement skills require appropriate player input/state.

## Implemented skills

Unless a row says otherwise, the listed values are the 0.3.2 configuration defaults.

| Skill | XP calculation | Gameplay effect |
|---|---|---|
| Strength | `raw Weight × distance × 2 XP` while raw Weight is at least 0.025 | Reduces carried Weight on the anchored 0.003 curve. Unlocks backpack slots at Strength 20/40/70/120/200. |
| Endurance | Raw stamina requested × 2 XP in PEAK's normalized units; currently described as 0.02 XP per displayed stamina point | Adds 0.005 normalized capacity per level, equal to 0.5 displayed stamina, and +0.1% regeneration per level. It does not reduce general stamina costs. |
| Wall Climbing | Intentional wall-climbing distance in any direction × 2 XP/m | +0.3% wall-climbing speed per level and wall stamina-cost reduction on the anchored 0.003 curve. |
| Rope Climbing | Intentional rope-climbing distance in any direction × 2 XP/m | +0.3% rope movement/handling speed per level and rope stamina-cost reduction on the anchored 0.003 curve. |
| Vine Climbing | Intentional vine-climbing distance in any direction × 2 XP/m | +0.3% vine speed per level, vine stamina-cost reduction on the anchored 0.003 curve, and 0.05% less velocity damping per level up to 75% retention. |
| Athletics | Intentional grounded walking × 0.35 XP/m; sprinting × 1.4 XP/m | +0.1% ground movement force per level, another +0.2% while sprinting, and sprint stamina-cost reduction on the anchored 0.003 curve. |
| Agility | 4 XP per successfully executed local jump | +0.15% jump impulse and +0.025% airborne turning response per level; jump stamina-cost reduction on the anchored 0.003 curve. |
| Resilience | Raw normalized fall Injury × 100 XP | Reduces fall Injury on the anchored 0.003 curve. Ordinary non-fall Injury is not trained or reduced. |
| Toxicology | Actual normalized Poison gained × 100 XP | Reduces incoming Poison on the anchored 0.003 curve. |
| Cold Tolerance | Actual normalized Cold gained × 100 XP | Reduces incoming Cold on the anchored 0.003 curve. |
| Heat Tolerance | Actual normalized Hot gained × 100 XP | Reduces incoming Hot on the anchored 0.003 curve. |
| Drowsy Tolerance | Actual normalized Drowsy gained × 100 XP | Reduces incoming Drowsy on the anchored 0.003 curve. Drowsy is PEAK's sleep-like status. |
| Spore Tolerance | Actual normalized Spores gained × 100 XP | Reduces incoming Spores on the anchored 0.003 curve. Spores are PEAK's zombification-like exposure. |
| Hunger Tolerance | `displayed Hunger × movement distance × 0.1 XP`, only at displayed Hunger 30 or higher | Reduces incoming Hunger on the anchored 0.003 curve. PEAK's existing 2.5-point Hunger display increments are unchanged. |
| Wet Grip | Slippery intentional wall distance × current `slippy` factor × 2 XP/m | Reduces slippery downward pull and wind/rain climbing stamina drain on the anchored 0.003 curve. It does not alter the Cold portion of wind-chill behavior. |
| Climbing Tenacity | Intentional wall distance × 2 XP/m while `Character.GetTotalStamina()` is below 0.20 | Reduces low-stamina climbing control, slide, and stamina penalties on the anchored 0.003 curve. |
| Poison Recovery | Actual normalized Poison naturally removed × 100 XP | +0.3% natural Poison recovery per level. Direct item removal grants no XP. |
| Cold Recovery | Actual normalized Cold naturally removed × 100 XP | +0.3% natural Cold recovery per level. Environmental warming is intended to count; direct item removal does not. The 0.3.2 special hook needs runtime confirmation. |
| Heat Recovery | Actual normalized Hot naturally removed × 100 XP | +0.3% natural Heat recovery per level. Direct item removal grants no XP. |
| Drowsy Recovery | Actual normalized Drowsy naturally removed × 100 XP | +0.3% natural Drowsy recovery per level. Direct item removal grants no XP. |
| Spore Recovery | Actual normalized Spores naturally removed × 100 XP | +0.3% natural Spores recovery per level. Direct item removal grants no XP. |

There are 21 active skills. Numeric enum value 14 is intentionally vacant because it belonged to the retired Pack Rat skill; keeping the later IDs stable protects existing saves.

## Scaling formulas

Positive bonuses use uncapped linear scaling unless a specific cap is named:

```text
multiplier = 1 + level × configuredRate
```

Capacity is also linear. With the default Endurance rate, level 2 contributes one additional displayed stamina point, level 50 contributes 25, level 100 contributes 50, and level 999 contributes 499.5 before status segments are considered.

Reductions use `AnchoredReductionMultiplier`. Through level 500, it maps the old level-999 reciprocal result onto level 500. From 500 to 999, it transitions exponentially to a 0.001 multiplier, or 99.9% reduction. At the shared default rate of 0.003:

| Level | Remaining multiplier | Reduction |
|---:|---:|---:|
| 10 | 0.943450 | 5.66% |
| 25 | 0.869679 | 13.03% |
| 50 | 0.769408 | 23.06% |
| 75 | 0.689869 | 31.01% |
| 100 | 0.625234 | 37.48% |
| 200 | 0.454794 | 54.52% |
| 300 | 0.357373 | 64.26% |
| 500 | 0.250188 | 74.98% |
| 999 | 0.001000 | 99.90% |

This curve applies to Strength, climbing costs, sprint cost, jump cost, Resilience, the six tolerances, Wet Grip, and Climbing Tenacity. Recovery-speed bonuses remain linear.

## Strength backpack milestones

Main inventory/hotbar capacity remains vanilla. Strength only expands an equipped backpack:

| Strength | Extra backpack slots | Total slots, assuming vanilla four |
|---:|---:|---:|
| 1–19 | 0 | 4 |
| 20–39 | 1 | 5 |
| 40–69 | 2 | 6 |
| 70–119 | 3 | 7 |
| 120–199 | 4 | 8 |
| 200+ | 5 | 9 |

The debug all-skill level override currently affects gameplay multipliers but not these slot milestones, because backpack capacity reads the saved Strength level directly. That mismatch is a documented testing limitation, not an intended player rule.

## Important XP-unit details

- PEAK stores stamina and statuses as normalized floats while much of the HUD shows values multiplied by 100.
- Condition and fall defaults multiply normalized changes by 100 so one displayed status point is approximately one unit of work before the configured XP multiplier is interpreted.
- Resistance XP is based on the actual post-mitigation status gained. This means strong resistance eventually slows its own training. Changing it to raw exposure is a future balance option.
- Hunger is deliberately trained by movement while hungry rather than by every Hunger timer tick.
- Recovery XP is awarded only when the removal path is classified as natural. `Action_ModifyStatus` item actions are explicitly excluded.
- Endurance's current source/config description does not match the earlier plain-language goal of one XP per displayed stamina used. Confirm the desired normalized/displayed unit during balance testing before changing it again.

## Configuration migrations

Config schema 2 migrated earlier high defaults, including Endurance 20→2, Strength 10→2, climbing 8→2, Athletics walking 2→0.5, Agility 20→4, and the revised effect rates. Schema 3 then migrated unchanged defaults for Athletics walking 0.5→0.35, sprinting 2→1.4, and Hunger movement 1→0.1. A value is migrated only when it exactly matches the retired default, preserving explicit player changes.
