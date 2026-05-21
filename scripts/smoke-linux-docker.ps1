# Manual smoke test for the linux-x64 standalone binary using Docker.
#
# Spins up a vanilla ubuntu container, installs the runtime dependencies a
# minimal Linux machine would need (libicu), unzips the produced binary,
# and runs the same smoke commands the CI smoke job runs. This complements
# .github/workflows/smoke.yml: the GitHub runner has libicu preinstalled
# so CI won't catch a missing-dep regression - this script will.
#
# Prerequisites:
#   - Docker Desktop running
#   - scripts/publish-binaries.ps1 already produced build/release/nresx-linux-x64.zip
#
# Usage:
#   powershell -File scripts\smoke-linux-docker.ps1

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$zipPath  = Join-Path $repoRoot 'build/release/nresx-linux-x64.zip'

if (-not (Test-Path $zipPath))
{
    throw "missing $zipPath - run scripts\publish-binaries.ps1 first"
}

$releaseDir = Join-Path $repoRoot 'build/release'
$fixturesDir = Join-Path $repoRoot '.test_files'

Write-Host "==> Running linux-x64 smoke inside ubuntu:latest..."

$bashScript = @'
set -e
apt-get update -qq
apt-get install -y unzip libicu-dev -qq >/dev/null
unzip -q /work/nresx-linux-x64.zip -d /opt/nresx
chmod +x /opt/nresx/nresx

echo "=== version ==="     && /opt/nresx/nresx --version
echo "=== help ==="        && /opt/nresx/nresx --help | head -5
echo "=== info resx ==="   && /opt/nresx/nresx info /fixtures/Resources.resx
echo "=== info yaml ==="   && /opt/nresx/nresx info /fixtures/Resources.yaml
echo "=== info json ==="   && /opt/nresx/nresx info /fixtures/Resources.json
echo "=== info xlsx ==="   && /opt/nresx/nresx info /fixtures/Resources.xlsx
echo "=== convert resx->po ==="
cp /fixtures/Resources.resx /tmp/src.resx
/opt/nresx/nresx convert /tmp/src.resx -f po
head -10 /tmp/src.po

echo ""
echo "SMOKE OK"
'@

& docker run --rm `
    -v "${releaseDir}:/work:ro" `
    -v "${fixturesDir}:/fixtures:ro" `
    ubuntu:latest bash -c $bashScript

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "SMOKE FAILED (exit $LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Linux smoke passed." -ForegroundColor Green
