# File index

This map helps a new conversation find the current source of truth quickly.

## Repository-level files

| Path | Purpose |
|---|---|
| `AGENTS.md` | Repository directives, owner decisions, safety boundaries, and definition of done. |
| `README.md` | Public project overview plus engineering-status links. |
| `CHANGELOG.md` | Source/repository change history. |
| `Directory.Build.props` | Shared version/build properties. |
| `Config.Build.user.props.template` | Template for local game/profile assembly paths. |
| `PEAKUsageSkills.slnx` | Solution entry point. |
| `scripts/Build-Package.ps1` | Validates and builds the Thunderstore archive. |
| `package/manifest.json` | Package identity, version, and dependency source of truth. |
| `package/README.md` | Player-facing Thunderstore README. |
| `package/CHANGELOG.md` | Player-facing packaged changelog. |
| `package/icon.png` | Final package icon; must be 256×256. |

## Core and configuration

| Path | Purpose |
|---|---|
| `src/PEAKUsageSkills/Plugin.cs` | Plugin lifecycle, service creation, Harmony application, save/update flow. |
| `src/PEAKUsageSkills/Core/SkillId.cs` | Stable active skill identifiers; value 14 remains retired. |
| `src/PEAKUsageSkills/Core/SkillMath.cs` | XP curve, linear/anchored scaling, HUD width, stamina capacity, backpack milestones. |
| `src/PEAKUsageSkills/Core/ProgressionService.cs` | XP eligibility, awarding, leveling, Recovery-to-Tolerance save migration, dirty save state. |
| `src/PEAKUsageSkills/Config/UsageSkillsConfig.cs` | All public config entries and exact-default schema migrations. |
| `src/PEAKUsageSkills/Effects/EffectService.cs` | Converts effective levels/config into gameplay multipliers. |

## Game integration

| Path | Purpose |
|---|---|
| `src/PEAKUsageSkills/GameAdapters/RunStateAdapter.cs` | Gameplay scene, Airport, and custom-run eligibility. |
| `src/PEAKUsageSkills/GameAdapters/ConditionSkillAdapter.cs` | PEAK status-to-Tolerance and recovery-effect mapping. |
| `src/PEAKUsageSkills/GameAdapters/InventorySkillService.cs` | Typed Backpack/Fanny/Jet Strength capacity and safe data expansion; Rocket exclusion. |
| `src/PEAKUsageSkills/GameAdapters/Patches/StaminaPatches.cs` | Endurance XP/effects, activity stamina costs, bonus-pool usability, stamina HUD. |
| `src/PEAKUsageSkills/GameAdapters/Patches/WeightPatches.cs` | Strength Weight multiplier and optional numeric-label attempt. |
| `src/PEAKUsageSkills/GameAdapters/Patches/ClimbingPatches.cs` | Wall/rope/vine performance, Wet Grip, Climbing Tenacity. |
| `src/PEAKUsageSkills/GameAdapters/Patches/MovementPatches.cs` | Athletics, jumps, and air control. |
| `src/PEAKUsageSkills/GameAdapters/Patches/FallAndAfflictionPatches.cs` | Vitality, Resiliency afflictions, shared Petrification hook, natural recovery effect, and cold-warmth scope. |
| `src/PEAKUsageSkills/GameAdapters/Patches/InventoryPatches.cs` | Type-neutral deserialization plus typed wheel and visuals. |
| `src/PEAKUsageSkills/Tracking/ActivitySampler.cs` | Distance/work sampling for Strength, climbing, Wet Grip, Tenacity, Athletics. |

## UI, diagnostics, and persistence

| Path | Purpose |
|---|---|
| `src/PEAKUsageSkills/UI/PauseMenuIntegration.cs` | Main/Resiliency panels, `Lv. ##.##`, one refresh per open, and hover tooltips. |
| `src/PEAKUsageSkills/UI/DebugOverlay.cs` | Optional config-driven compact diagnostics overlay. |
| `src/PEAKUsageSkills/Diagnostics/DiagnosticHub.cs` | Rate-limited aggregate BepInEx logging. |
| `src/PEAKUsageSkills/Diagnostics/RuntimeMetrics.cs` | Current runtime measurements used by logs/overlay. |
| `src/PEAKUsageSkills/Persistence/SaveModels.cs` | Save schema models. |
| `src/PEAKUsageSkills/Persistence/SaveStore.cs` | Atomic writes, rotating backups, and retired-key cleanup. |

## Tests

| Path | Purpose |
|---|---|
| `tests/PEAKUsageSkills.Tests/SkillMathTests.cs` | Pure tests for progression/scaling/HUD/backpack math. |
| `tests/PEAKUsageSkills.Tests/PEAKUsageSkills.Tests.csproj` | Test project. |

## Documentation authority

- Start with [`HANDOFF.md`](HANDOFF.md) for current status and the next test sequence.
- Use [`SKILLS_AND_BALANCE.md`](SKILLS_AND_BALANCE.md) for exact defaults.
- Use [`ARCHITECTURE_AND_HOOKS.md`](ARCHITECTURE_AND_HOOKS.md) before changing a patch.
- Use [`DECISIONS.md`](DECISIONS.md) before reviving an old design.
- Use [`TESTING_AND_COMPATIBILITY.md`](TESTING_AND_COMPATIBILITY.md) to distinguish verified behavior from compile-only work.
- Use [`ROADMAP.md`](ROADMAP.md) for pending work and candidate options.

The sibling `PEAK_UsageSkills_Spec` directory predates the current implementation. It can explain history, but it is not authority for networking, skill count, Pack Rat, main inventory, or balance.
