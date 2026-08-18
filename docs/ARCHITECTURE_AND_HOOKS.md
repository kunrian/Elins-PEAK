# Architecture and game hooks

This is the implementation map for source version 0.3.2. Hook names must be rechecked against the installed game assembly after every PEAK update.

## Assembly baseline

The last inspected game assembly was:

- Path: `C:\Program Files (x86)\Steam\steamapps\common\PEAK\PEAK_Data\Managed\Assembly-CSharp.dll`
- Size: 1,562,112 bytes
- Modified: 2026-08-15 20:59:24 local time
- SHA-256: `CAD8EF0702F512F0AD4595F9C169D4025EB8FA351083B64FD4E9FD6F78D5D14C`
- Unity executable file/product version: 6000.3.15 / 6000.3.15f1

These details describe the binary that was inspected, not a declaration of compatibility with all later builds.

## Runtime flow

1. `Plugin` binds and migrates configuration, loads the local save, creates services/controllers, and applies Harmony patches.
2. `RunStateAdapter` classifies the scene. XP requires a scene containing `Level`, rejects the Airport, and rejects custom runs unless enabled. Effects require a gameplay scene and the local character.
3. Patches observe raw game events and apply idempotent multipliers or targeted argument changes.
4. `ActivitySampler` awards distance/work XP on a 0.2-second sampler for movement-based skills.
5. `ProgressionService` applies XP, levels skills through the shared curve, records diagnostics, and marks the save dirty.
6. `SaveStore` writes atomically on its interval and on shutdown. `PauseMenuIntegration` renders current saved levels once whenever the pause menu is enabled.

Progression and effects are local-player only. There is no custom RPC, host-owned level selection, or config handshake in 0.3.2.

## Harmony integration table

| Game method | Patch/source | Purpose and notes |
|---|---|---|
| `Character.GetMaxStamina` | `StaminaPatches.cs` | Adds Endurance base capacity while preserving food/status stamina segments. |
| `Character.UseStamina` | `StaminaPatches.cs` | Classifies raw usage for Endurance XP and applies activity-specific cost multipliers. |
| `Character.OutOfRegularStamina` | `StaminaPatches.cs` | Lets extended/bonus stamina remain usable after vanilla regular stamina reaches zero. This fixed the sports-drink pool being stranded. |
| `Character.UpdateVariablesFixed` / `Character.AddStamina` | `StaminaPatches.cs` | Applies Endurance regeneration and low-stamina state handling. |
| `StaminaBar.Update` | `StaminaPatches.cs` | Extends the actual rendered bar/frame width for Endurance and repositions status segments. Ordered after PeakStatsEx when present. |
| `CharacterAfflictions.UpdateWeight` | `WeightPatches.cs` | Applies Strength to raw carried Weight. Ordered after Piggyback and at last Harmony priority to reduce overwrite conflicts. Also attempts a one-decimal label update after PeakStatsEx. |
| `CharacterClimbing.GetRequestedPostition` | `ClimbingPatches.cs` | Wall speed/control, wall stamina efficiency, slippery pull, and low-stamina tenacity. The misspelling is the actual game method name. |
| `CharacterRopeHandling.Update` | `ClimbingPatches.cs` | Rope movement/handling performance and rope stamina efficiency. |
| `CharacterVineClimbing.FixedUpdate` | `ClimbingPatches.cs` | Vine movement performance, stamina efficiency, and reduced velocity damping for light slingshot potential. |
| `WindChillZone.ApplyStatus` | `ClimbingPatches.cs` | Scopes the slippery/wind climbing penalties used by Wet Grip without assigning the Cold component to Wet Grip. |
| `CharacterMovement.GetMovementForce` | `MovementPatches.cs` | Athletics ground and sprint force bonuses. |
| `Character.CalculateWorldMovementDir` | `MovementPatches.cs` | Very light Agility air-control adjustment. |
| `CharacterMovement.JumpRpc` | `MovementPatches.cs` | Counts an executed local jump and applies jump-cost/performance state. |
| Generated jump iterator `MoveNext` | `MovementPatches.cs` | Transpiles the jump impulse/cost path. This compiler-generated target is fragile across game updates and requires special smoke testing. |
| `CharacterMovement.CheckFallDamage` | `FallAndAfflictionPatches.cs` | Marks normal movement fall-damage scope. |
| `CharacterClimbing.CheckFallDamage` | `FallAndAfflictionPatches.cs` | Marks wall-climbing fall-damage scope. |
| `CharacterAfflictions.AddStatus` | `FallAndAfflictionPatches.cs` | Applies Resilience to scoped fall Injury; applies matching Resistance and records actual exposure XP. Also identifies environmental Hot canceling existing Cold. |
| `CharacterAfflictions.SubtractStatus` | `FallAndAfflictionPatches.cs` | Applies matching natural Recovery and awards actual recovered XP. |
| `Action_ModifyStatus.RunAction` | `FallAndAfflictionPatches.cs` | Creates an explicit-item scope so antidotes and other direct item effects cannot award Recovery XP. |
| `BackpackData.Init` / `BackpackData.DeserializeValue` | `InventoryPatches.cs` | Ensures Strength-unlocked backpack capacity without discarding occupied high indexes. |
| `BackpackWheel.InitWheel` | `InventoryPatches.cs` | Builds wheel entries for the expanded backpack. |
| `BackpackVisuals.RefreshVisuals` | `InventoryPatches.cs` | Refreshes visual backpack slots for the expanded data. |

## Notable implementation choices

### Stamina and HUD

Endurance is added to vanilla base capacity rather than multiplying the current total. Food/Well Fed/status pools remain separate. The frame target is computed from the current rendered width and held to the original left edge, so different levels and status combinations resize inside one coherent frame. Early prototypes instead resized from the wrong total and produced bars that began outside the frame, never visually reached maximum, or overlapped Weight/Hunger segments.

### Condition classification

Resistance mappings are Poison→Toxicology, Cold→Cold Tolerance, Hot→Heat Tolerance, Drowsy→Drowsy Tolerance, Spores→Spore Tolerance, and Hunger→Hunger Tolerance. Recovery exists for the first five only.

Most natural recovery arrives through `SubtractStatus(..., decreasedNaturally: true)`. Cold is special: assembly inspection showed environmental warming adding Hot, which internally subtracts existing Cold without that flag. The 0.3.2 patch uses a thread-local environmental-warming scope and labels the recovery `NaturalRecovery:ColdByWarmth`; `Action_ModifyStatus` remains excluded. This path has compiled and passed unit tests but still needs fresh runtime evidence.

### Inventory safety

Only backpack `itemSlots` are expanded. Main inventory/hotbar modifications and all Pack Rat overflow logic were removed after runtime bugs. When reading a backpack, capacity is at least the desired Strength capacity and at least one beyond the highest occupied index, preventing a downgrade from deleting an existing high-index item.

### Fail-soft behavior and coexistence

- Patches should affect only `Character.localCharacter`.
- Multipliers are applied at use sites rather than permanently accumulating values each frame.
- Optional-mod ordering is expressed with Harmony metadata where known.
- Diagnostic snapshots are rate-limited; status changes are aggregated.
- Patch failures should be visible in the BepInEx log and should avoid corrupting saves.

## Persistence

Progression is stored at `%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json` using schema 1. Writes use a temporary file and replace/move strategy with five rotating backups. A migration removes the retired Pack Rat key. Configuration is the ordinary BepInEx config and currently uses internal config schema 3.

## Source organization

- `Core/` — skill IDs, progression service, shared math.
- `Config/` — public settings and exact-default migrations.
- `Effects/` — effective-level lookup and multiplier calculations.
- `GameAdapters/` — scene/status/inventory adapters and Harmony patches.
- `Tracking/` — sampled movement/work XP.
- `Persistence/` — save model and atomic save store.
- `Diagnostics/` — rate-limited log aggregation and current metrics.
- `UI/` — pause-menu panels/tooltips and optional debug overlay.

See [`FILE_INDEX.md`](FILE_INDEX.md) for direct feature-to-file navigation.
