# Testing and compatibility status

The most important testing fact is that current source/package 0.3.2 is newer than the installed `devtest` build and its logs. Do not treat old 0.3.1 runtime output as proof that the final 0.3.2 changes work in game.

## Current evidence summary

| Surface | Status | Evidence/limitation |
|---|---|---|
| Compile and unit tests | Passed for 0.3.2 | All 50 tests passed on 2026-08-18 while validating this documentation branch. |
| Package construction | Passed for 0.3.2 | `dist/Elins_PEAK-0.3.2.zip` rebuilt with zero warnings/errors; validation-build SHA-256 is `9626F03481F2F4BD4C53428F0886ED992BC63331437A6BF58FA15FDD181C36A6`. |
| General progression/solo hooks | Broadly exercised on earlier builds | Lobby, first level, climbing, eating, damage, poison, Weight changes, and level overrides were logged. |
| Airport XP rejection | Runtime verified | Old log showed `xpEligible=False` in Airport. |
| Expanded stamina capacity/frame | Runtime verified on the preceding implementation | Different levels and food/status pools were tested; the frame eventually adjusted correctly and bonus stamina became spendable. |
| Pause-menu levels/tooltips | Runtime verified on the preceding implementation | Levels updated, reset worked during development, passive panels and hover bubbles rendered. Font became acceptable; panel sizing/placement still needs polish. |
| Main inventory expansion | Failed and removed | Expanded main slots/hotbar bugged in game. No code should reintroduce it without a new design. |
| Pack Rat/overflow penalties | Retired | Removed from 0.3.2 with save migration. |
| Strength backpack milestones | Not freshly runtime-tested | Final implementation exists only in current 0.3.2 source/package. |
| Cold Recovery warmth hook | Not freshly runtime-tested | Final special classification exists only in current 0.3.2 source/package. |
| 0.3.2 XP rebalance | Not freshly runtime-tested | Athletics and Hunger reductions compile and are covered only indirectly by unit logic. |
| Multiplayer | Not tested | Deliberately reserved for final testing. There is no custom synchronization layer. |
| Custom-run XP option | Not directly tested | Off by default. |

The `devtest` profile currently contains `ChiseledCactusTeam-Elins_PEAK` 0.3.1. Its latest logs include Pack Rat/overflow fields, proving they are from the retired implementation. Install the 0.3.2 package/DLL before collecting new evidence.

## Automated verification

Run from the repository root:

```powershell
dotnet test .\PEAKUsageSkills.slnx -c Release
.\scripts\Build-Package.ps1
```

The current test suite covers the XP exponent, progression percentages, linear and anchored scaling (including finite level-999 values), rendered-width correction, expanded stamina capacity, Strength backpack milestones, and the Hunger training threshold. It does not execute Unity, Harmony patch binding, game UI, serialization through live game types, or multiplayer.

## Compatibility observations

| Mod/system | Current relationship | What is known |
|---|---|---|
| PEAKLib Core/UI | Required dependency | Used for integration and pause-menu UI. |
| PeakStatsEx | Optional compatibility target | Stamina HUD patch is ordered after it. PeakStatsEx appears to cache/quantize the Weight label, so the attempted one-decimal Weight display does not reliably refresh even though gameplay Weight changes. Hunger display is intentionally left alone. |
| Piggyback | Optional compatibility target | Weight patch declares ordering after Piggyback and last priority. Not systematically tested across all carrying cases. |
| Atomic Leveling | Reference only | Its local/player-owned progression approach informed the current direction. No code dependency or data import. |
| Craft PEAK | Known bad co-install in testing | One enabled run produced an infinite error log and could not load the menu. It was disabled; fixing this is explicitly out of scope for now. |
| BackPackCapacity | Conflicting feature | Disable it when testing Elin's PEAK. Both modify backpack data/capacity. |
| MoreSlots | Not required | The old mod was considered as a reference, but main-slot expansion was removed. Compatibility is not guaranteed and it should not be needed. |
| EasyBackpack Fix | Co-installed in some profiles | No targeted compatibility result yet. |
| PEAK Unlimited, ItemStats, Sense of Direction | Co-installed/reference environment | No targeted validation; their presence in a profile is not proof of compatibility. |
| Vanilla PEAK updates | Fragile boundary | Exact game hooks, especially the generated jump iterator, must be rechecked after game updates. |

No third-party implementation should be copied without checking its license. Reference behavior and public APIs; keep Elin's PEAK's code independently maintained.

## Next solo runtime checklist

Use a clean `devtest` launch with Craft PEAK, BackPackCapacity, and MoreSlots disabled unless a compatibility test specifically calls for one of them.

1. Confirm the startup log reports Elin's PEAK 0.3.2 and all Harmony patch groups initialize without exceptions.
2. Confirm Airport activity awards no XP, then enter a standard level and confirm XP becomes eligible.
3. Reset or back up the progression save. Exercise walking, sprinting, jumping, weighted movement, wall/rope/vine climbing, and a fall. Compare menu values after reopening ESC; no constant refresh loop should run while it remains open.
4. Test Endurance around saved levels 1, 50, 100, 200, and 300 with no status, Weight/Hunger segments, Well Fed, sports drink, trail mix, and exhaustion. Confirm the left edge stays fixed, the frame reaches the intended width, status sections stay inside it, and all bonus stamina is spendable.
5. Raise saved Strength across 19→20, 39→40, 69→70, 119→120, and 199→200. Equip/reopen/serialize the backpack at each milestone; verify 4→9 total slots, wheel entries, visuals, and that occupied high slots survive reconnect/reload.
6. Verify main inventory remains vanilla and that no Pack Rat XP, UI row, log field, or overflow debuff appears.
7. Gain Cold, then recover through environmental warmth. Expect `NaturalRecovery:ColdByWarmth`, increased removal, and Cold Recovery XP. Use a direct status-removing item and confirm it awards no Recovery XP.
8. Compare Hunger and Athletics gains over measured distances to the new 0.1 / 0.35 / 1.4 defaults. Confirm Hunger awards nothing below displayed 30.
9. Exercise rain/slippery wall climbing and wall climbing while `Character.GetTotalStamina()` is below 0.20 for Wet Grip and Climbing Tenacity.
10. Inspect the log for rejected teleports, unexpected per-frame spam, patch exceptions, NaN/Infinity, and save failures.

## Multiplayer checklist (last phase)

Test at least host/client, two different existing save levels, joining/leaving, reconnecting, host migration if PEAK supports it, backpacks with expanded occupied slots, and status effects applied by another player. The intended current behavior is that each player uses their own progression and effects; the host must not select everyone's level. Record whether game-authoritative movement/status calls undermine that assumption before designing any RPC layer.

## Logs and saves

- BepInEx runtime log: the active Gale profile's `BepInEx\LogOutput.log`.
- Plugin config: the active profile's `BepInEx\config\com.chiseled.peak.usageskills.cfg` (confirm the generated name if the plugin GUID changes).
- Progression save: `%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json` plus rotating backups.
- Reference plugins: `C:\Users\Chiseled\AppData\Roaming\com.kesomannen.gale\peak\profiles\Default\BepInEx\plugins`.

Always identify the loaded plugin version near the top of the log before using it as evidence.
