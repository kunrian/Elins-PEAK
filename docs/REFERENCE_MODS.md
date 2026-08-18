# Reference mods and dependency boundaries

The owner's reference plugin directory is:

`C:\Users\Chiseled\AppData\Roaming\com.kesomannen.gale\peak\profiles\Default\BepInEx\plugins`

Reference mods help identify PEAK APIs, UI patterns, and compatibility risks. Their presence does not make them dependencies, and observed behavior does not authorize copying code or assets. Check licenses before using implementation details.

## Required runtime dependencies

| Dependency | Use |
|---|---|
| BepInEx | Plugin loader, configuration, logging, Harmony environment. |
| PEAKLib Core | PEAK integration utilities required by the package manifest. |
| PEAKLib UI | Pause-menu panels, text, and tooltip construction. |

The package manifest is the authority for exact dependency package identifiers/versions.

## References and compatibility targets

| Mod | Role in this project | Current conclusion |
|---|---|---|
| Atomic Leveling | Reference for local use-based progression and lobby eligibility | Elin's PEAK uses its own save/math/hooks. The owner prefers player-owned progression across changing groups; no dependency. |
| PeakStatsEx | HUD compatibility target | Likely owns/caches the extra numeric labels. Stamina layout is explicitly patched after it. Exact one-decimal Weight display remains unresolved; gameplay Weight scaling works independently. |
| Piggyback | Weight/carry interaction target | Elin's PEAK's Weight patch is ordered after Piggyback and at last priority. Needs focused runtime testing. |
| Craft PEAK | Known incompatibility observed during development | With it enabled, one launch produced an infinite error log and failed to load the menu. It was disabled and is not being fixed now. |
| BackPackCapacity | Overlapping backpack implementation | Disable it with Elin's PEAK. Strength now owns backpack capacity milestones. |
| MoreSlots | Old main-inventory reference | Not needed. Main inventory expansion was tried, bugged out, and was removed. Compatibility is not guaranteed. |
| EasyBackpack Fix | Potential backpack compatibility target | Present in some environments but not specifically tested with the final Strength milestones. |
| PEAK Unlimited | General reference/co-installed mod | No deliberate integration or targeted compatibility result. |
| ItemStats | UI/stat reference | No deliberate integration or targeted compatibility result. |
| Sense of Direction | UI/co-installed reference | No deliberate integration or targeted compatibility result. |

## Dependency policy

- Keep hard dependencies minimal: BepInEx and the PEAKLib modules actually used.
- Prefer runtime detection/Harmony ordering for optional compatibility.
- Do not make Atomic Leveling, PeakStatsEx, Piggyback, MoreSlots, or BackPackCapacity mandatory.
- Avoid patching a third-party label as a core gameplay dependency. If PeakStatsEx is absent, Elin's PEAK effects and saves must still work.
- When a compatibility adapter is added, document the exact tested mod version and whether failure disables only the adapter or the whole plugin.
- Do not treat an old decompile or method name as stable after a PEAK update; inspect the current assembly.

## Profile hygiene for reliable tests

Start with a minimal `devtest` profile containing the required loader/libraries and Elin's PEAK. Add optional mods one at a time for compatibility runs. Always capture the loaded plugin list and version header with the log. The 0.3.2 solo log is a valid baseline; it does not approve the 0.4.0 Resiliency merge or typed Fanny/Jet capacity.
