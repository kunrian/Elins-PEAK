# Skills and balance

This is the implemented 0.4.7 default balance. Generated BepInEx configuration can override the rates.

All skills default to level 1, cap at 999, and use `round(100 × level^1.21)` XP for the next level. Airport XP is always disabled; custom-run XP is disabled by default.

## Main Skills

| Skill | Training default | Effect |
|---|---|---|
| Strength | raw Weight × movement meter × 2 XP | Anchored Weight reduction; +1/+2/+3/+4/+5 stored-item slots at 20/40/70/120/200. |
| Endurance | normalized raw stamina requested × 20 XP | +0.5% base stamina and +0.1% regeneration per level; the matching expanded threshold is honored before pass-out. |
| Wall Climbing | intentional meter × 8 XP | +0.3% speed per level; anchored stamina-cost reduction. |
| Rope Climbing | intentional meter × 8 XP | +0.3% speed per level; anchored stamina-cost reduction. |
| Vine Climbing | intentional meter × 8 XP | +0.3% speed, anchored cost reduction, and light momentum retention. |
| Athletics | grounded walk × 0.22 XP; sprint × 0.9 XP | +0.1% ground force and another +0.2% sprint force per level; anchored sprint-cost reduction. Sprinting still ends when vanilla regular stamina is exhausted. |
| Agility | successful local jump × 8 XP | +0.15% jump impulse, +0.025% air-control responsiveness, and anchored jump-cost reduction per level. |
| Vitality | normalized raw fall Injury × 100 XP | Anchored fall-Injury reduction. |
| Wet Grip | slippery weighted wall meter × 20 XP | Anchored slippery pull and related wind-drain reduction. |
| Climbing Tenacity | intentional wall meter below 20% regular stamina × 40 XP | Anchored low-stamina climbing-penalty reduction. |

## Resiliency

Every Resiliency skill except Hunger earns `actual incoming normalized affliction × 100 XP`. Hunger instead earns `displayed Hunger × movement meters × 0.02 XP` while at 10 or more displayed Hunger. Recovery and cleansing never award XP.

| Skill | Incoming effect | Natural recovery effect |
|---|---|---|
| Poison | 0.15% anchored reduction rate per level | +0.15% per level |
| Cold | 0.15% anchored reduction rate per level | +0.15% where PEAK exposes a natural-recovery path; ordinary warming remains under investigation |
| Heat | 0.15% anchored reduction rate per level | +0.15% per level |
| Drowsy | 0.15% anchored reduction rate per level | +0.15% per level |
| Spores | 0.15% anchored reduction rate per level | +0.15% per level |
| Hunger | 0.15% anchored reduction rate per level; incoming Hunger does not award XP | None |
| Curse | 0.15% anchored reduction rate per level | None |
| Petrification | 0.15% anchored reduction rate per level | None |

`Toxicology` remains the internal save/enum name for Poison so existing progression keeps its stable key. The other shortened names likewise retain their stable 0.4.0 keys.

## Strength item storage

| Back item | Base item slots | Strength milestones | Separate slot behavior |
|---|---:|---:|---|
| Backpack | 4 | +1 through +5 | None |
| Fanny Pack | 2 | +1 through +5 | None |
| Jet Pack | 1 | +1 through +5 | Its one fuel slot remains separate and unchanged |
| Rocket Pack | 0 | Excluded | Rocket behavior remains vanilla |

The data layer never deletes occupied high-index slots when a level/config is reduced. Logical capacity follows the equipped back-item type rather than the backing array's vanilla four-element allocation.

## Save migration

0.4.0 converts Poison/Cold/Heat/Drowsy/Spore Recovery into its matching Tolerance by summing the complete accumulated XP represented by both level/XP states plus lifetime work. Old Recovery keys are then removed. This is idempotent and does not rename the legacy `Toxicology` key.

Config schema 7 adopts the 0.4.6 XP defaults only when an entry still equals its prior default. Schema 8 restores Hunger movement training and migrates its former exact defaults from rate `0.1→0.02` and normalized threshold `0.30→0.10`. Custom values remain unchanged.
