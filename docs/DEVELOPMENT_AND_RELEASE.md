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

The script validates `package/manifest.json`, checks the 256×256 icon, builds without live deployment, stages the expected files, creates `dist/Elins_PEAK-<version>.zip`, checks archive entries, and reports a SHA-256 hash. The package root must contain `manifest.json`, `README.md`, `CHANGELOG.md`, `icon.png`, and the plugin DLL in the structure expected by Thunderstore.

The current local artifact is `dist\Elins_PEAK-0.4.1.zip` with SHA-256 `5A6E3C98F3A855EE1C20995601A4CF3C2983802EC000E0126455FD843B415393`, built on 2026-08-18 with zero warnings/errors. The solution and test project were compiled, but the full test suite was intentionally not executed for this focused UI/migration cycle. Rebuilding may change the ZIP hash even when payloads are equivalent, so record the exact live-tested/publication artifact again.

## Runtime deployment and data

For focused testing, deploy to the Gale `devtest` profile only. Disable Craft PEAK and BackPackCapacity. MoreSlots is no longer required. After launch, verify the startup line reports the intended version before testing.

- Log: `<active Gale profile>\BepInEx\LogOutput.log`
- Config: `<active Gale profile>\BepInEx\config\...usageskills....cfg`
- Save: `%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json`
- Backups: adjacent rotating progression backups maintained by `SaveStore`

Back up the progression file before destructive/manual test changes. Resetting a user's skills is an explicit data change and should not be inferred from a normal build or install request.

## Version update checklist

1. Update the assembly/plugin version in source/project metadata.
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

GitHub source publication is not a Thunderstore upload. Thunderstore publication requires its own authenticated workflow and should happen only after the exact package passes runtime validation. No remote push or Thunderstore upload is authorized for the current 0.4.1 live-test build.

## Updating after a PEAK release

1. Hash and record the new `Assembly-CSharp.dll`.
2. Reinspect every method in [`ARCHITECTURE_AND_HOOKS.md`](ARCHITECTURE_AND_HOOKS.md), especially generated iterator targets and UI layouts.
3. Build against the new assemblies.
4. Run automated tests, then the focused smoke checklist.
5. Update the assembly baseline and compatibility notes only after inspection/testing.
