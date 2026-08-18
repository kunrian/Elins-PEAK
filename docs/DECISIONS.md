# Decision record

This is a compact record of owner decisions and superseded proposals. It prevents old planning documents from silently becoming requirements again.

| Area | Current decision | Status/history |
|---|---|---|
| Product | Standalone Elin-inspired use-based progression mod | Accepted |
| Dependencies | BepInEx + PEAKLib Core/UI only | Accepted; Atomic Leveling and Craft PEAK are references, not dependencies |
| Progress ownership | Local/player-owned across groups | Accepted owner override; host does not assign levels |
| Networking | No custom RPC/config handshake yet | Deferred until final multiplayer testing; old host-canonical spec is not implemented |
| Maximum level | 999 default | Accepted |
| XP curve | `round(100 * level^1.21)` | Accepted after balance revision |
| Reduction scaling | Anchored reciprocal curve through 999 | Accepted; reaches old 999 target near 500 and 0.001 multiplier at 999 |
| Positive scaling | Linear per level | Accepted, with explicit caps only where implemented (vine retention 75%) |
| Endurance | Capacity + regeneration, no global cost reduction | Accepted; activity skills own efficiency |
| Climbing | Separate Wall, Rope, and Vine skills | Accepted |
| Directional climbing XP | Intentional distance in all directions | Accepted; down/side movement can be legitimate work |
| Agility | Executed jump XP, impulse/cost/light air control | Accepted; no landing-displacement scorer yet |
| Conditions | Separate Resistance and natural Recovery for Poison/Cold/Heat/Drowsy/Spores | Accepted |
| Sleep/zombie naming | Sleep = Drowsy, zombification exposure = Spores | Verified in game assembly |
| Hunger | One Tolerance skill; XP only from movement at displayed Hunger >=30 | Accepted; Hunger Recovery deliberately absent |
| Cold Recovery | Passive Heat-to-Cold cancellation counts; item actions do not | Implemented in 0.3.2, runtime confirmation pending |
| Inventory | Main inventory stays vanilla | Accepted after expanded main slots caused bugs |
| Backpack | Strength unlocks +1/+2/+3/+4/+5 at 20/40/70/120/200 | Accepted, runtime confirmation pending |
| Pack Rat | Removed, including overflow penalties and XP | Retired in 0.3.2; save migration deletes its entry |
| MoreSlots | Not required; compatibility not guaranteed | No dependency |
| BackpackCapacity | Disable alongside Elin's PEAK | Both modify backpack data |
| XP in Airport | Always off | Accepted and observed in logs |
| Custom-run XP | Off by default, configurable | Implemented, dedicated runtime test pending |
| Anti-farming | Fundamental validity now; rate limits/diminishing returns later | Explicitly deferred by owner |
| Diagnostics | Rate-limited BepInEx logs; no manual import/export | Accepted |
| Pause menu | Three passive colored panels, refresh once on open | Accepted; layout polish remains |
| Debug controls | Remove level/reset/status buttons from release UI | Accepted; config debug override/overlay remain |
| Weight display | Gameplay value matters; PeakStatsEx decimal label is secondary compatibility work | Current limitation |
| Multiplayer order | Solo validation first, multiplayer last | Accepted |

## Rejected or retired approaches

- Repeated additive writes to movement/stamina fields.
- A single generic Climbing skill.
- Host-selected level values.
- Treating bonus/Well Fed stamina as Endurance base capacity.
- Resizing the stamina frame from current status totals instead of Endurance capacity.
- Constant 0.25-second pause-menu refresh.
- Release-visible test/reset/effect buttons.
- Main-inventory array/hotbar expansion.
- Pack Rat overflow penalties and mitigation.
- Giving Recovery XP for antidotes or other direct cleansing.
- Forcing Hunger into one-decimal display when PEAK uses 2.5-point increments.
