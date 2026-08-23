# Development, packaging, and release

## Local layout

- Workspace: `C:\Users\Chiseled\Documents\Projects\PEAK`
- Repository: `C:\Users\Chiseled\Documents\Projects\PEAK\PEAKUsageSkills`
- Historical research: `C:\Users\Chiseled\Documents\Projects\PEAK\PEAK_UsageSkills_Spec` (non-authoritative)
- Game: `C:\Program Files (x86)\Steam\steamapps\common\PEAK`
- Gale reference profile plugins: `C:\Users\Chiseled\AppData\Roaming\com.kesomannen.gale\peak\profiles\Default\BepInEx\plugins`
- Gale development profile: `C:\Users\Chiseled\AppData\Roaming\com.kesomannen.gale\peak\profiles\devtest`

The project targets the locally configured PEAK/BepInEx/PEAKLib assemblies. .NET 10 is installed, while the project framework and game compatibility remain defined by the project files and game runtime.

## First-time local setup

Copy `Config.Build.user.props.template` to `Config.Build.user.props` and set local game/profile reference paths. The user-specific file should remain untracked. Confirm references resolve before interpreting compiler errors as source regressions.

## Build and test

From the repository root:

```powershell
dotnet restore .\PEAKUsageSkills.slnx
dotnet build .\PEAKUsageSkills.slnx -c Release -p:DeployToDevtest=false
dotnet test .\PEAKUsageSkills.slnx -c Release
```

`DeployToDevtest=false` is the safe default for validation. Do not replace the live DLL while PEAK is running; an earlier attempt failed because the game held/used the plugin. Close the game before deployment.

## Build the Thunderstore package

```powershell
.\scripts\Build-Package.ps1
```

The script validates `package/manifest.json`, checks the 256×256 icon, validates matching non-empty keys across all six JSON catalogs, builds without live deployment, stages the expected files, creates `dist/Elins_PEAK-<version>.zip`, checks archive entries, and reports a SHA-256 hash. The package root contains `manifest.json`, the English/localized READMEs, `CHANGELOG.md`, and `icon.png`; the DLL and `Localization/*.json` live together under `BepInEx/plugins/Elins_PEAK`.

The current artifact is `dist\Elins_PEAK-0.4.7.zip` with SHA-256 `EC2235040EC2329979D8A589C396CBE73C549DCD6EC0E4F8903BC7B2818AAEEE`, built on 2026-08-23 with zero warnings/errors. Its DLL is SHA-256 `8A64F9064A1F39A3F5CEE170FB2AED5ED46FE4FF16C421BAD5146A18410D89DF` and reports assembly/file version 0.4.7.0. The focused Hunger math test passed four cases; the full suite was intentionally skipped for this isolated balance restoration.

The Thunderstore-published 0.4.5 ZIP is SHA-256 `1B4F4C6E3988050721C30E28FD32D16F3DE3038B7701916E222121F158EC3E0D`. It contains the intended 0.4.4/0.4.5 gameplay code but a DLL that self-reports 0.4.2. The owner validated the separate 0.4.6 Gale build; 0.4.7 is packaged and source-published but not deployed to Gale or uploaded to Thunderstore.

## Runtime deployment and data

For focused testing, deploy to the Gale `devtest` profile only. Disable Craft PEAK and BackPackCapacity. MoreSlots is no longer required. After launch, verify the startup line reports the intended version before testing.

- Log: `<active Gale profile>\BepInEx\LogOutput.log`
- Config: `<active Gale profile>\BepInEx\config\...usageskills....cfg`
- Save: `%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json`
- Backups: adjacent rotating progression backups maintained by `SaveStore`

Back up the progression file before destructive/manual test changes. Resetting a user's skills is an explicit data change and should not be inferred from a normal build or install request.

Before the 0.4.6 deployment, the live package was archived as `2026-08-18\dist\Elins_PEAK-live-before-0.4.6-20260823.zip`, SHA-256 `272F34BBDC0FE5B878144791269478F54ABAF77ECEDC10B6D8D0FAFB984A0A4A`. The deployment copied only package/plugin files and left the active config and progression save untouched.

## Version update checklist

1. Update the assembly/plugin version in source/project metadata. `Build-Package.ps1` rejects disagreement among `PluginVersion`, the project `<Version>`, the manifest, and the compiled assembly.
2. Update `package/manifest.json` to the same semantic version.
3. Update root and package changelogs. Keep `package/README.md` player-facing and personal in tone.
4. Ensure root engineering docs distinguish implemented, runtime-verified, and merely planned work.
5. Confirm `package/icon.png` is the owner's final 256×256 image.
6. Run the Release solution build and package script. Run the full test suite when core math or persistence behavior warrants it; record explicitly when it is skipped for a focused runtime cycle.
7. Inspect the ZIP contents and manifest dependencies.
8. Install the exact packaged artifact into `devtest` and run focused tests.
9. Commit only intentional files; never commit local config, saves, logs, binaries outside the release artifact policy, or third-party mods.
10. Push through a reviewable branch/PR, then merge when approved.

## GitHub workflow

Remote: [kunrian/Elins-PEAK](https://github.com/kunrian/Elins-PEAK)

Use a branch from current `main`, stage explicit paths, commit a coherent change, push with upstream, and open a draft PR with validation results and remaining runtime risks. GitHub CLI authentication can be checked with:

```powershell
& 'C:\Program Files\GitHub CLI\gh.exe' auth status
```

GitHub source publication is not a Thunderstore upload. Thunderstore 0.4.5 was published on 2026-08-19. Version 0.4.7 is the GitHub/package release state; Gale remains on the owner-validated 0.4.6 build pending a separate deployment request.

## Updating after a PEAK release

1. Hash and record the new `Assembly-CSharp.dll`.
2. Reinspect every method in [`ARCHITECTURE_AND_HOOKS.md`](ARCHITECTURE_AND_HOOKS.md), especially generated iterator targets and UI layouts.
3. Build against the new assemblies.
4. Run automated tests, then the focused smoke checklist.
5. Update the assembly baseline and compatibility notes only after inspection/testing.
