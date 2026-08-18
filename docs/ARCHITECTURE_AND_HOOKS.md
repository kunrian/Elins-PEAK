# Architecture and hooks

This map describes source version 0.4.0 against the inspected PEAK assembly hash recorded in `HANDOFF.md`.

## Runtime flow

1. `Plugin` binds/migrates config, loads the local save, merges retired Recovery states, creates services, and patches each Harmony class fail-soft.
2. `ActivitySampler` measures real character-center movement for Strength, climbing, Athletics, Wet Grip, and Climbing Tenacity.
3. Direct patches observe stamina use, jumps, falls, affliction gains/removal, movement/climbing calculations, and typed back-item UI/data.
4. `ProgressionService` validates eligibility, accumulates XP, levels, and emits level changes.
5. `EffectService` converts the effective level into linear bonuses or anchored reductions.
6. `SaveStore` writes atomically on its interval/shutdown. Pause UI values refresh once each time the menu is enabled.

Progression and effects are local-player only. There is no custom RPC or host-owned level/config handshake.

## Principal hooks

| PEAK target | Purpose |
|---|---|
| `Character.GetMaxStamina`, `UpdateVariablesFixed`, `UseStamina` | Endurance capacity/regen and raw stamina-use XP; activity-specific cost transforms. |
| `StaminaBar.Update` | Render extended Endurance capacity. |
| `CharacterAfflictions.UpdateWeight` | Transform current local Weight and train Strength through sampled weighted movement. |
| `CharacterClimbing.GetRequestedPostition` | Wall speed/control/cost plus Wet Grip and Tenacity effects. |
| `CharacterRopeHandling.Update`, `CharacterVineClimbing.FixedUpdate` | Rope/vine movement and cost transforms. |
| `CharacterMovement.GetMovementForce` | Athletics ground/sprint force. |
| `CharacterMovement.JumpRpc` | Agility XP, impulse, and jump cost. |
| `CharacterMovement.CheckFallDamage`, `CharacterClimbing.CheckFallDamage` | Scope legitimate falls for Resilience XP/effect. |
| `CharacterAfflictions.AddStatus` | Incoming Poison/Cold/Hot/Drowsy/Spores/Hunger/Curse reduction and XP. Petrify is deliberately delegated. |
| `CharacterAfflictions.AddPetrify` | One shared positive Petrification reduction/XP hook for AddStatus, amulet, and Citadel callers. Negative removal is ignored for XP/effect. |
| `CharacterAfflictions.SubtractStatus` | Apply matching Tolerance recovery speed only on natural recovery; never grant XP. |
| `BackpackData.DeserializeValue` | Preserve every serialized/current stored item without guessing back-item type. |
| `BackpackWheel.InitWheel` | Apply typed logical capacity and add only normal item slices. Jet fuel remains its separate vanilla slice. |
| `BackpackVisuals.RefreshVisuals` | Render typed item capacity for world/on-back visuals; skip Rocket Pack. |

## Typed back-item invariant

`BackpackData` initializes a four-element item array even for smaller back items, so array length cannot be treated as capacity. `InventorySkillService` derives the logical base from `BackpackSlot.BackpackType`: Backpack 4, Fanny 2, Jet 1, Rocket/None 0. Visible capacity is the greater of base-plus-Strength and the highest occupied index plus one. This prevents loss when levels fall while keeping hidden vanilla allocation from granting Fanny/Jet capacity.

Jet fuel uses `DataEntryKey.Fuel` and `BackpackWheel.jetpackSlice`; the mod changes neither. Rocket Pack is rejected before item-data/wheel/visual expansion.

## Affliction invariant

XP is calculated from the actual increase after PEAK validation, resistance, clamping, and rounding. Poison/Cold/Heat/Drowsy/Spore natural removal is multiplied by the same skill but cannot train it. Hunger, Curse, and Petrification have incoming behavior only.

Petrification cannot share the normal `AddStatus` calculation: PEAK converts that call to integer points and forwards to `AddPetrify`, while amulet/Citadel code can call `AddPetrify` directly. Owning the shared method avoids missed sources and double application.

## Save/config compatibility

Enum values 8–13 remain stable; value 14 remains unused; Wet Grip/Tenacity retain 15/16. Recovery values 17–21 are retired, and new Curse/Petrification values use 22/23. Save migration addresses retired states by their string keys. Config schema 4 updates only exact former defaults and deletes obsolete generated entries.
