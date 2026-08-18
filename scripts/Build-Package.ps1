[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$packageSource = Join-Path $repositoryRoot "package"
$manifestPath = Join-Path $packageSource "manifest.json"
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json

if ($manifest.name -notmatch '^[A-Za-z0-9_]{1,128}$') {
    throw "manifest.json contains an invalid package name: $($manifest.name)"
}

if ($manifest.version_number -notmatch '^\d+\.\d+\.\d+$') {
    throw "manifest.json contains an invalid semantic version: $($manifest.version_number)"
}

if ([string]::IsNullOrWhiteSpace($manifest.description) -or $manifest.description.Length -gt 250) {
    throw "manifest.json description must contain between 1 and 250 characters."
}

$requiredPackageFiles = @("icon.png", "README.md", "manifest.json")
foreach ($fileName in $requiredPackageFiles) {
    $filePath = Join-Path $packageSource $fileName
    if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
        throw "Required Thunderstore file is missing: $filePath"
    }
}

$localizationSource = Join-Path $packageSource "Localization"
$requiredLocalizationFiles = @("de.json", "es.json", "fr.json", "ja.json", "ko.json", "zh-CN.json")
$baselineLocalizationKeys = $null
foreach ($fileName in $requiredLocalizationFiles) {
    $filePath = Join-Path $localizationSource $fileName
    if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
        throw "Required localization file is missing: $filePath"
    }

    $localization = Get-Content -LiteralPath $filePath -Raw | ConvertFrom-Json
    $keys = @($localization.PSObject.Properties.Name | Sort-Object)
    if ($keys.Count -eq 0) {
        throw "Localization file contains no translations: $filePath"
    }

    $emptyKeys = @($localization.PSObject.Properties | Where-Object { [string]::IsNullOrWhiteSpace([string]$_.Value) })
    if ($emptyKeys.Count -gt 0) {
        throw "Localization file contains empty translations: $filePath"
    }

    if ($null -eq $baselineLocalizationKeys) {
        $baselineLocalizationKeys = $keys
    }
    elseif (Compare-Object -ReferenceObject $baselineLocalizationKeys -DifferenceObject $keys) {
        throw "Localization key set does not match the other locale files: $filePath"
    }
}

Add-Type -AssemblyName System.Drawing
$iconPath = Join-Path $packageSource "icon.png"
$icon = [System.Drawing.Image]::FromFile($iconPath)
try {
    if ($icon.Width -ne 256 -or $icon.Height -ne 256) {
        throw "Thunderstore icon must be exactly 256x256; found $($icon.Width)x$($icon.Height)."
    }
}
finally {
    $icon.Dispose()
}

& dotnet build (Join-Path $repositoryRoot "PEAKUsageSkills.slnx") -c $Configuration --nologo -p:DeployToDevtest=false
if ($LASTEXITCODE -ne 0) {
    throw "Release build failed with exit code $LASTEXITCODE."
}

$pluginPath = Join-Path $repositoryRoot "src\PEAKUsageSkills\bin\$Configuration\netstandard2.1\PEAKUsageSkills.dll"
if (-not (Test-Path -LiteralPath $pluginPath -PathType Leaf)) {
    throw "Compiled plugin was not found: $pluginPath"
}

$distRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "dist"))
$stagingRoot = [System.IO.Path]::GetFullPath((Join-Path $distRoot "$($manifest.name)-$($manifest.version_number)"))
if (-not $stagingRoot.StartsWith($distRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to stage outside the repository dist directory: $stagingRoot"
}

if (Test-Path -LiteralPath $stagingRoot) {
    Remove-Item -LiteralPath $stagingRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $stagingRoot -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $packageSource "icon.png") -Destination $stagingRoot
Copy-Item -LiteralPath (Join-Path $packageSource "README.md") -Destination $stagingRoot
Copy-Item -LiteralPath (Join-Path $packageSource "manifest.json") -Destination $stagingRoot
Get-ChildItem -LiteralPath $packageSource -Filter "README.*.md" -File |
    ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $stagingRoot }

$changelogPath = Join-Path $packageSource "CHANGELOG.md"
if (Test-Path -LiteralPath $changelogPath -PathType Leaf) {
    Copy-Item -LiteralPath $changelogPath -Destination $stagingRoot
}

$pluginDestination = Join-Path $stagingRoot "BepInEx\plugins\Elins_PEAK"
New-Item -ItemType Directory -Path $pluginDestination -Force | Out-Null
Copy-Item -LiteralPath $pluginPath -Destination $pluginDestination
$localizationDestination = Join-Path $pluginDestination "Localization"
New-Item -ItemType Directory -Path $localizationDestination -Force | Out-Null
foreach ($fileName in $requiredLocalizationFiles) {
    Copy-Item -LiteralPath (Join-Path $localizationSource $fileName) -Destination $localizationDestination
}

$zipPath = Join-Path $distRoot "$($manifest.name)-$($manifest.version_number).zip"
if (Test-Path -LiteralPath $zipPath -PathType Leaf) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $stagingRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $entries = @($archive.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
    foreach ($rootFile in $requiredPackageFiles) {
        if ($entries -notcontains $rootFile) {
            throw "Generated archive does not contain $rootFile at its root."
        }
    }

    if ($entries -notcontains "BepInEx/plugins/Elins_PEAK/PEAKUsageSkills.dll") {
        throw "Generated archive does not contain the plugin DLL in the BepInEx plugins directory."
    }

    foreach ($fileName in $requiredLocalizationFiles) {
        $entryName = "BepInEx/plugins/Elins_PEAK/Localization/$fileName"
        if ($entries -notcontains $entryName) {
            throw "Generated archive does not contain localization file $entryName."
        }
    }
}
finally {
    $archive.Dispose()
}

$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash
Write-Output "Created $zipPath"
Write-Output "SHA256 $hash"
