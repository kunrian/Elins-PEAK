# Elin's PEAK handoff

**Date:** 2026-08-23

**Current source/package version:** 0.4.5

**Game assembly inspected:** PEAK `Assembly-CSharp.dll` SHA-256 `CAD8EF0702F512F0AD4595F9C169D4025EB8FA351083B64FD4E9FD6F78D5D14C`

**Published Thunderstore 0.4.5 ZIP:** SHA-256 `1B4F4C6E3988050721C30E28FD32D16F3DE3038B7701916E222121F158EC3E0D`

**Corrected local 0.4.5 ZIP:** `dist/Elins_PEAK-0.4.5.zip`, SHA-256 `82740E85B5BF80CB79F0FCCD121172CB232FDCA912731DBB140E95F33D49F7FE`

## Where the project is

Elin's PEAK is a local-player, use-based BepInEx/Harmony progression mod with 18 persistent skills and a default maximum level of 999. Version 0.4.5 is the current Thunderstore release and installed Gale package.

The published 0.4.5 payload contains the intended 0.4.4/0.4.5 gameplay code, but its DLL metadata and BepInEx plugin constant were accidentally left at 0.4.2. This is why the 2026-08-19 log says `Elin's PEAK 0.4.2` even though the pass-out and modded-scene changes are present. Source, project, and manifest metadata are now aligned at 0.4.5, and the package script rejects future version mismatches. The corrected local package is not byte-identical to the already-published Thunderstore artifact and has not been installed or uploaded as part of the source realignment.

## Changes after 0.4.2

- 0.4.3 replaced relative translated-README links with GitHub links so they work from Thunderstore.
- 0.4.4 removed the brittle `Level_*` scene-name requirement. XP eligibility now requires an active local character outside the Airport, while `RunSettings.IsCustomRun` and `EnableXpInCustomRuns` retain the custom-run policy.
- 0.4.5 aligns Endurance's expanded stamina capacity with both vanilla pass-out decisions: `CharacterAfflictions.get_shouldPassOut` and the threshold in `Character.HandlePassedOut`.
- The public README translations, changelog, description, and manifest now match the published 0.4.5 package.
- `PluginVersion`, the project `<Version>`, `package/manifest.json`, and the compiled assembly now all report 0.4.5.

## Current skill behavior

- Main Skills: Strength, Endurance, Wall Climbing, Rope Climbing, Vine Climbing, Athletics, Agility, Vitality, Wet Grip, and Climbing Tenacity.
- Blue Resiliency skills: Poison, Cold, Heat, Drowsy, Spores, Hunger, Curse, and Petrification.
- Existing `Resilience` progression/settings migrate to Vitality. Retired Recovery progression merges once into the matching Resiliency skill.
- Resiliency earns XP only from actual incoming affliction. Recovery and cleansing award no XP.
- Poison/Cold/Heat/Drowsy/Spores also accelerate exposed natural-recovery paths. Hunger, Curse, and Petrification have no recovery-speed effect.
- Main inventory remains vanilla. Strength expands only Backpack, Fanny Pack, and Jet Pack item capacity at levels 20/40/70/120/200. Jet fuel and Rocket Pack remain untouched.
- The localized pause panel follows PEAK's language selector live. Six supplied catalogs cover French, German, Spanish, Simplified Chinese, Japanese, and Korean; all other choices fall back to English.

Exact XP defaults and effects remain in [`SKILLS_AND_BALANCE.md`](SKILLS_AND_BALANCE.md).

## Verified assembly and runtime findings

The 2026-08-19 `devtest` log is from a pre-publication build of the same gameplay changes, with the stale 0.4.2 identity:

- All six localization files registered 36 keys.
- Petrification patch health resolved the intended `CharacterAfflictions.AddPetrify(Int32)` overload.
- Both new pass-out hooks reported healthy, and `HandlePassedOut` replaced one vanilla threshold.
- At an Endurance threshold of 1.015, vanilla pass-out was observably suppressed at status sum 1.000.
- TerrainRandomiser 1.1.10 and several other mods were loaded without an Elin's PEAK patch exception.
- The Airport remained XP-ineligible, while the spawned character in `Level_8` accrued normal gameplay XP.

This validates the new hook wiring and pass-out behavior, but not every 0.4.5 feature. The current installed package was refreshed on 2026-08-23 from the published Thunderstore payload; the latest available log predates that reinstall.

## Known open work

- Cold recovery-speed handling still needs the actual environmental warming caller identified. Do not restore Recovery XP and do not multiply item cleansing.
- Real gameplay Petrification from both an amulet and a Citadel hazard is not evidenced in the current log, even though the exact hook is healthy.
- Live language switching/fallback and CJK wrapping still need an explicit UI pass.
- Multiplayer remains untested.
- The corrected 0.4.5 metadata build needs only a short startup check before any later deployment: confirm BepInEx and the plugin startup line both say 0.4.5 and that all patch-health entries remain healthy.

## Publication state

- Thunderstore latest: `ChiseledCactusTeam-Elins_PEAK-0.4.5`, published 2026-08-19.
- Gale `devtest`: published 0.4.5 package contents, whose DLL self-identifies as 0.4.2.
- Corrected local artifact: `dist/Elins_PEAK-0.4.5.zip`; compile/package validation passed with zero warnings/errors on 2026-08-23.
- GitHub `main` is the canonical source target. Old `agent/*` branches may be removed only after their tips are confirmed ancestors of `origin/main`.
