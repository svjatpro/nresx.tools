# Regenerates docs/api/ from XML doc comments in nresx.Core.
#
# Requires the local dotnet tool manifest at .config/dotnet-tools.json -
# run `dotnet tool restore` once after cloning.
#
# Usage (Windows PowerShell 5.1 or PowerShell 7+):
#   powershell -File scripts\gen-api-docs.ps1
#   pwsh -File scripts/gen-api-docs.ps1

$ErrorActionPreference = 'Stop'

# Walk up to the repo root (the folder that contains nresx.Core/)
$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try
{
    $coreProj = Join-Path $repoRoot 'nresx.Core/nresx.Core.csproj'
    $depsDir  = Join-Path $repoRoot 'build/api-docs-deps'
    $apiDir   = Join-Path $repoRoot 'docs/api'

    Write-Host '==> Publishing nresx.Core to resolve transitive deps...'
    & dotnet publish $coreProj -c Release -o $depsDir --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

    Write-Host '==> Wiping previous generated files (preserves hand-written README.md)...'
    if (Test-Path $apiDir)
    {
        Get-ChildItem $apiDir -File -Filter '*.md' |
            Where-Object { $_.Name -ne 'README.md' } |
            Remove-Item -Force
    }

    Write-Host '==> Running xmldoc2md...'
    & dotnet xmldoc2md (Join-Path $depsDir 'nresx.Core.dll') `
        --output $apiDir `
        --structure flat `
        --member-accessibility-level public `
        --back-button
    if ($LASTEXITCODE -ne 0) { throw 'xmldoc2md failed' }

    Write-Host ''
    Write-Host "Done. Review docs/api/ and commit if the diff looks right." -ForegroundColor Green
}
finally
{
    Pop-Location
}
