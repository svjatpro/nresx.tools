# Builds self-contained single-file standalone binaries for GitHub Releases.
#
# Produces one zip per RID at build/release/, ready to upload as a Release
# asset. Self-contained = no .NET runtime required on the target machine.
#
# Usage (Windows PowerShell 5.1 or PowerShell 7+):
#   powershell -File scripts\publish-binaries.ps1
#   pwsh -File scripts/publish-binaries.ps1
#
# Optional: -Rid <rid> builds just one RID (faster iteration), e.g.
#   pwsh -File scripts/publish-binaries.ps1 -Rid win-x64

param(
    [string]$Rid = ''
)

$ErrorActionPreference = 'Stop'

$rids = @('win-x64', 'linux-x64', 'osx-arm64')
if ($Rid) { $rids = @($Rid) }

# Walk up to the inner repo root (the folder that contains nresx.CommandLine/)
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try
{
    $cliProj   = Join-Path $repoRoot 'nresx.CommandLine/nresx.CommandLine.csproj'
    $outRoot   = Join-Path $repoRoot 'build/release'
    $stageRoot = Join-Path $repoRoot 'build/release/_stage'

    # Read version from the csproj for the build banner. The zip filename
    # itself is version-less so the GitHub releases/latest/download/<name>.zip
    # URL pattern stays stable across releases.
    [xml]$cliXml = Get-Content $cliProj
    $version = ($cliXml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1).Trim()
    if (-not $version) { throw "could not read <Version> from $cliProj" }

    Write-Host "==> Publishing nresx v$version for: $($rids -join ', ')"

    if (Test-Path $outRoot) { Remove-Item $outRoot -Recurse -Force }
    New-Item -ItemType Directory -Path $outRoot   | Out-Null
    New-Item -ItemType Directory -Path $stageRoot | Out-Null

    foreach ($r in $rids)
    {
        Write-Host ''
        Write-Host "==> $r"

        $stageDir = Join-Path $stageRoot $r
        New-Item -ItemType Directory -Path $stageDir | Out-Null

        & dotnet publish $cliProj `
            -c Release `
            -r $r `
            --self-contained true `
            -p:PublishSingleFile=true `
            -p:IncludeNativeLibrariesForSelfExtract=true `
            -p:EnableCompressionInSingleFile=true `
            -p:DebugType=embedded `
            -p:DebugSymbols=false `
            -o $stageDir `
            --verbosity quiet
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed for $r" }

        # Keep just the user-facing files: the binary itself + license + readme.
        Get-ChildItem $stageDir -File |
            Where-Object { $_.Name -notin @('nresx', 'nresx.exe') } |
            Remove-Item -Force
        Get-ChildItem $stageDir -Directory |
            Remove-Item -Recurse -Force

        Copy-Item (Join-Path $repoRoot 'LICENSE')   $stageDir
        Copy-Item (Join-Path $repoRoot 'README.md') $stageDir

        $zipName = "nresx-$r.zip"
        $zipPath = Join-Path $outRoot $zipName
        Compress-Archive -Path (Join-Path $stageDir '*') -DestinationPath $zipPath -Force

        $sizeMb = [math]::Round((Get-Item $zipPath).Length / 1MB, 1)
        Write-Host "    -> $zipName ($sizeMb MB)"
    }

    # Stage dir is only useful for debugging; remove so build/release/ holds only zips.
    Remove-Item $stageRoot -Recurse -Force

    Write-Host ''
    Write-Host "Done. Assets in build/release/:" -ForegroundColor Green
    Get-ChildItem $outRoot -File | ForEach-Object { Write-Host "  $($_.Name)" }
}
finally
{
    Pop-Location
}
