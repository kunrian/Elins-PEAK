# Roadmap from 0.4.2

## Priority 0 — focused live validation

- Validate 0.4.2 startup, live PEAK-language switching/fallback, CJK rendering, and Vitality save/config migration.
- Validate Strength capacity on Backpack/Fanny/Jet and confirm Jet fuel/Rocket behavior remain vanilla.
- Confirm the explicit `AddPetrify(int)` hook is healthy and capture Curse/Petrification XP/effect evidence from an amulet and Citadel hazard.
- Confirm recovery gives effect but no XP for Poison/Heat/Drowsy/Spores.

## Priority 1 — Cold Tolerance recovery path

The latest 0.3.2 log showed warming as `Cold:SubtractLocal`, and the existing Heat scope did not classify it. Trace the actual caller in a fresh 0.4.2 log/stack-aware diagnostic, then apply Cold's recovery multiplier only to that environmental path. Do not restore Recovery XP and do not multiply item cleansing.

## Priority 2 — multiplayer

- Host and join with different local levels.
- Verify each player keeps local progression/effects.
- Exercise item storage, status, stamina, and climbing across ownership boundaries.
- Confirm expanded Backpack/Fanny/Jet data serializes for reconnect/late join without touching Jet fuel.

## Candidate skill: Aeronautics

Assembly finding: `Glider.FixedUpdate` spends stamina and calls `CharacterMovement.ApplyGlider` for vertical/horizontal drag plus forward force. Balloons modify gravity/jump multipliers instead; parachute uses parasol drag.

Recommended first design: glider-only training from horizontal glide distance while the glider is open and paying stamina; effect could improve stamina efficiency or modest forward control. A combined balloons/parachute skill is possible but needs separate hooks, sources, and caps and should not be implied by the Glider hook alone.

## Candidate skill: Throwing

Assembly finding: `CharacterItems.DropItemRpc` derives force from 0..1 charge and the item's `throwForceMultiplier`, then stores the original charge in thrown data/events.

Recommended first design: award XP for valid charged local throws, weighted by charge and possibly item carry weight. Scale the computed impulse after charge interpolation while preserving the item multiplier. Do not modify `throwCharge` itself. Use a guarded transpiler or a prefix/finalizer strategy that cannot leak temporary force values after exceptions.

## Deferred Thunderstore publication

- Upload the exact tested ZIP to Thunderstore through its separate authenticated workflow.
