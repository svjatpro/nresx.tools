# Release process

Manual release flow for cutting a new version of `nresx`. CI automation
lands later (RSX-109); this is the canonical procedure until then.

## Prerequisites

- Push permission on `svjatpro/nresx.tools`.
- PowerShell available (the publish script is `.ps1`).
- A clean working tree on the release branch (typically `main`).
- `dotnet` SDK 9.x.

## 1. Bump version

Edit `<Version>` in both csprojs:

- `nresx.Core/nresx.Core.csproj`
- `nresx.CommandLine/nresx.CommandLine.csproj`

Keep them in lockstep. Commit:

```sh
git commit -am "Bump version to X.Y.Z"
```

## 2. Build standalone binaries

```powershell
powershell -File scripts\publish-binaries.ps1
```

Produces three zips in `build/release/`:

- `nresx-win-x64.zip`
- `nresx-linux-x64.zip`
- `nresx-osx-arm64.zip`

The asset names are intentionally version-less so the
`releases/latest/download/nresx-<rid>.zip` URLs in `README.md` keep working
across releases.

## 3. Manual smoke test

Cross-platform CI runs on every push (`.github/workflows/smoke.yml`), so green
checks on the release commit cover the automated side. Before tagging,
verify the actual zip you're about to ship:

- **Windows** - extract `nresx-win-x64.zip`, run `nresx --version`,
  `nresx info .test_files\Resources.resx`, `nresx convert ... -f po` against
  a real file.
- **Linux** - same drill under WSL or a Linux VM.
- **macOS** - no local option without a Mac. Trust the CI green check, or
  ask a Mac user to spot-check.

## 4. Push the NuGet packages

```sh
dotnet pack nresx.Core/nresx.Core.csproj         -c Release
dotnet pack nresx.CommandLine/nresx.CommandLine.csproj -c Release
dotnet nuget push <path-to-.nupkg> -s https://api.nuget.org/v3/index.json -k <api-key>
```

The CommandLine project is a `dotnet tool` package - publishing it makes
`dotnet tool install -g nresx` pick up the new version.

## 5. Tag and create the GitHub Release

```sh
git tag vX.Y.Z
git push origin vX.Y.Z
```

Then on GitHub:

1. Releases -> "Draft a new release".
2. Tag: `vX.Y.Z` (the one you just pushed).
3. Title: `nresx vX.Y.Z`.
4. Description: highlights from the changelog. Link to the milestone if there is one.
5. Upload the three zips from `build/release/` as release assets.
6. Publish.

That's it. The `README.md` download buttons will start serving the new zips
within a few seconds (GitHub's `releases/latest/download/<name>` redirect
updates as soon as the release is published).

## 6. Verify

- Open `README.md` on GitHub, click a download button, confirm you get the
  new zip.
- `dotnet tool update -g nresx`, then `nresx --version` should show the new
  number.

## Hotfix flow

For a patch release on an older version: branch from the tag, make the fix,
bump to `X.Y.(Z+1)`, repeat steps 2-6.
