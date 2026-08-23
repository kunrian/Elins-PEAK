# Architecture and hooks

This map describes source version 0.4.7 against the inspected PEAK assembly hash recorded in `HANDOFF.md`.

## Runtime flow

1. `Plugin` binds/migrates config, registers supplied translations into PEAK's localization table, loads the local save, merges retired Recovery states, creates services, and patches each Harmony class fail-soft.
2. `ActivitySampler` measures real character-center movement for Strength, climbing, Athletics, Wet Grip, Climbing Tenacity, and thresholded Hunger training.
3. Direct patches observe stamina use/pass-out decisions, jumps, falls, affliction gains/removal, movement/climbing calculations, and typed back-item UI/data.
4. `ProgressionService` validates eligibility, accumulates XP, levels, and emits level changes.
5. `EffectService` converts the effective level into linear bonuses or anchored reductions.
6. `SaveStore` writes atomically on its interval/shutdown. Pause UI values refresh once each time the menu is enabled.

Progression and effects are local-player only. There is no custom RPC or host-owned level/config handshake.

## Principal hooks

| PEAK target | Purpose |
|---|---|
| `Character.GetMaxStamina`, `UpdateVariablesFixed`, `UseStamina` | Endurance capacity/regen and raw stamina-use XP; activity-specific cost transforms. |
| `CharacterAfflictions.get_shouldPassOut`, `Character.HandlePassedOut` | Compare accumulated affliction against Endurance's expanded stamina threshold in both vanilla pass-out paths. |
| `StaminaBar.Update` | Render extended Endurance capacity. |
| `CharacterAfflictions.UpdateWeight` | Transform current local Weight and train Strength through sampled weighted movement. |
| `CharacterClimbing.GetRequestedPostition` | Wall speed/control/cost plus Wet Grip and Tenacity effects. |
| `CharacterRopeHandling.Update`, `CharacterVineClimbing.FixedUpdate` | Rope/vine movement and cost transforms. |
| `CharacterMovement.GetMovementForce` | Athletics ground/sprint force. |
| `CharacterMovement.JumpRpc` | Agility XP, impulse, and jump cost. |
| `CharacterMovement.CheckFallDamage`, `CharacterClimbing.CheckFallDamage` | Scope legitimate falls for Vitality XP/effect. |
| `CharacterAfflictions.AddStatus` | Incoming Poison/Cold/Hot/Drowsy/Spores/Hunger/Curse reduction. All but Hunger award exposure XP; Petrify is deliberately delegated. |
| `CharacterAfflictions.AddPetrify(int)` | One shared positive Petrification reduction/XP hook for AddStatus, amulet, and Citadel callers. The parameterless console-command overload is deliberately excluded; negative removal is ignored for XP/effect. |
| `CharacterAfflictions.SubtractStatus` | Apply matching Tolerance recovery speed only on natural recovery; never grant XP. |
| `BackpackData.DeserializeValue` | Preserve every serialized/current stored item without guessing back-item type. |
| `BackpackWheel.InitWheel` | Apply typed logical capacity and add only normal item slices. Jet fuel remains its separate vanilla slice. |
| `BackpackVisuals.RefreshVisuals` | Render typed item capacity for world/on-back visuals; skip Rocket Pack. |
| `LocalizedText.OnLangugageChanged` | Refresh dynamic skill rows/section titles after PEAK changes language; no Harmony patch or separate language setting. |

## Run eligibility invariant

`RunStateAdapter` never grants XP in an Airport scene and still respects the configurable `RunSettings.IsCustomRun` gate. Outside the Airport, gameplay is recognized by the presence of `Character.localCharacter`, not by requiring a vanilla `Level_*` scene name. This prevents terrain/scene-randomizer mods from disabling progression merely by changing scene naming.

## Endurance pass-out invariant

PEAK checks the vanilla status-sum threshold in more than one place. A postfix suppresses `CharacterAfflictions.shouldPassOut` while the local player's status sum remains below `EffectService.EnduranceCapacityMultiplier`, and a guarded transpiler replaces the matching `1.0` comparison in `Character.HandlePassedOut`. Both are local-player/effect gated, preserve the vanilla decision at level 1, and log patch-health failure rather than silently claiming support when the target changes.

## Sprint stamina boundary

PEAK's `Character.OutOfRegularStamina` checks only `currentStamina`, while `UseStamina` can spend `extraStamina` for actions that opt into it. Version 0.4.6 deliberately leaves `OutOfRegularStamina` unpatched: sprinting stops at vanilla regular-stamina exhaustion and does not roll into the bonus/food stamina bar. Endurance capacity, regeneration, and Athletics sprint-cost reduction remain active before that boundary.

## Localization invariant

`LocalizationService` loads the six packaged JSON catalogs once and registers every key through `PEAKLib.UI.MenuAPI.CreateLocalization`. PEAK's current `LocalizedText.Language` selects the displayed value and PEAK's own language-change event refreshes the dynamic panel. Spanish Spain and Spanish LatAm share `es.json`. Other game languages and missing/invalid entries remain English because the English value initializes every language slot before translated slots are overwritten. Locale files live beside the DLL under `Localization`; malformed or missing files fail soft and log their fallback.

## Typed back-item invariant

`BackpackData` initializes a four-element item array even for smaller back items, so array length cannot be treated as capacity. `InventorySkillService` derives the logical base from `BackpackSlot.BackpackType`: Backpack 4, Fanny 2, Jet 1, Rocket/None 0. Visible capacity is the greater of base-plus-Strength and the highest occupied index plus one. This prevents loss when levels fall while keeping hidden vanilla allocation from granting Fanny/Jet capacity.

Jet fuel uses `DataEntryKey.Fuel` and `BackpackWheel.jetpackSlice`; the mod changes neither. Rocket Pack is rejected before item-data/wheel/visual expansion.

## Affliction invariant

Exposure XP is calculated from the actual increase after PEAK validation, resistance, clamping, and rounding. Poison/Cold/Heat/Drowsy/Spore natural removal is multiplied by the same skill but cannot train it. Curse and Petrification have incoming behavior only.

Hunger is the exception to exposure training. `AddStatus` still applies Hunger's resistance effect, but Hunger XP comes only from accepted character-center movement at 10 or more displayed Hunger. The sampler rejects teleport/respawn deltas above five meters. Work is `displayed Hunger × movement meters`, with a default rate of 0.02 XP per work unit.

Petrification cannot share the normal `AddStatus` calculation: PEAK converts that call to integer points and forwards to `AddPetrify(int)`, while amulet/Citadel code can call that overload directly. PEAK also has a parameterless console command named `AddPetrify`; Harmony annotations must include `typeof(int)` or patch discovery is ambiguous. Owning the integer method avoids missed sources and double application.

## Save/config compatibility

Vitality retains enum value 7 and migrates the legacy `Resilience` save key. Values 8–13 remain stable; value 14 remains unused; Wet Grip/Tenacity retain 15/16. Recovery values 17–21 are retired, and Curse/Petrification use 22/23. Config schema 5 migrates Resilience settings to Vitality and removes obsolete generated entries. Schema 6 adopted the 0.4.2 XP defaults; schema 7 updates only Endurance `10→20` and sprinting Athletics `1.05→0.9`; schema 8 restores Hunger movement settings and migrates their former exact defaults from rate `0.1→0.02` and normalized threshold `0.30→0.10`. Exact-default migration preserves other custom values.
