# Elin's PEAK agent directives

These instructions are the repository-local source of truth for future coding agents. They supersede the planning assumptions in the older sibling folder `PEAK_UsageSkills_Spec` whenever that material conflicts with the current code or this file.

## Start here

1. Read `docs/HANDOFF.md`.
2. Read the task-specific document linked from `docs/README.md`.
3. Run `git status -sb` before changing anything.
4. Inspect the current installed `Assembly-CSharp.dll` before adding or changing a PEAK hook. Do not invent class, field, method, or IL names.

## Current product decisions

- The mod is standalone BepInEx/Harmony code with hard dependencies only on PEAKLib Core and PEAKLib UI.
- Progression and gameplay effects are local and player-owned. The host does not select another player's level. No custom RPC/config-handshake layer is implemented yet.
- The release has 18 skills: 10 Main Skills and 8 blue Resiliency skills. Retired Recovery progression is merged into matching Tolerances during load.
- Main-inventory capacity remains vanilla. Strength unlocks typed item slots for Backpack, Fanny Pack, and Jet Pack at levels 20, 40, 70, 120, and 200; Jet fuel and Rocket Pack remain untouched.
- Airport/lobby XP is always rejected. Custom-run XP is disabled by default.
- Multiplayer runtime testing is the final validation phase, not an assumption of correctness.
- Anti-farming beyond fundamental action validity and teleport rejection remains intentionally deferred.

## Hook and effect rules

- Keep direct PEAK internals in `GameAdapters` and Harmony patch classes.
- Prefer transforming an existing calculation or input. Never repeatedly add a persistent bonus to a mutable game field.
- Preserve vanilla, Ascent, and other-mod modifiers when practical; multiply or transform the current result instead of replacing it with a hard-coded absolute value.
- Temporary field changes must be restored in a Harmony finalizer so exceptions cannot leave a bonus stacked.
- Measure XP from raw/pre-effect work where the current hook permits it.
- Patch failures are fail-soft and must remain visible through patch-health logging.

## Persistence and logging

- Saves are local, versioned, atomic, and backed up. Do not delete or reset progression unless the owner explicitly asks.
- Keep diagnostics in the standard BepInEx log and rate-limit aggregate output. Do not add per-frame log spam.
- Update `docs/HANDOFF.md`, `docs/ARCHITECTURE_AND_HOOKS.md`, and the relevant roadmap/testing document whenever behavior or a hook changes.

## Build, deployment, and releases

- Build against the local PEAK assemblies through `Config.Build.user.props`.
- A zero-warning Release solution build and `scripts\Build-Package.ps1` are the normal gates. Run the full unit suite when core math/persistence changes warrant it; focused runtime cycles do not require every test.
- The build must not deploy to Gale unless `DeployToDevtest=true` is explicitly selected.
- Do not replace a live DLL while PEAK is running.
- A local package build, a Gale installation, a GitHub merge, and a Thunderstore upload are separate actions. Never infer authorization for one from another.
- Version 0.3.2 received a solo `devtest` runtime pass on 2026-08-18. Version 0.4.1 changes require a fresh runtime pass and must not be described as live-verified yet.

## Scope discipline

- Preserve user changes and unrelated dirty files.
- Treat the current code plus current game assembly/runtime behavior as authoritative over historical proposals.
- Record rejected approaches and unresolved options instead of silently removing their history.
