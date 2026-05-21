# Release process

CI handles the binary build and the GitHub Release creation; NuGet push and
the final "publish the draft" step stay manual so a human is always in the
loop on what reaches users.

## Prerequisites

- Push permission on `svjatpro/nresx.tools`.
- A clean working tree on the release branch (typically `main`).
- `dotnet` SDK 9.x (for the NuGet push step).
- A NuGet.org API key with publish rights for the `nresx` and `nresx.Core`
  packages.

## 1. Bump version

Edit `<Version>` in both csprojs:

- `nresx.Core/nresx.Core.csproj`
- `nresx.CommandLine/nresx.CommandLine.csproj`

Keep them in lockstep. Commit and push:

```sh
git commit -am "Bump version to X.Y.Z"
git push
```

Wait for the `smoke` workflow on this commit to go green before tagging - a
red smoke check means the release zips will be broken.

## 2. Tag - this triggers the release workflow

```sh
git tag vX.Y.Z
git push origin vX.Y.Z
```

Pushing the tag triggers `.github/workflows/release.yml`, which:

- Builds the standalone binary for each RID (`win-x64`, `linux-x64`, `osx-arm64`)
  on the matching OS runner.
- Smoke-tests each binary (`--version` + `info` on resx and yaml fixtures).
- Uploads each zip as a workflow artifact.
- Creates a **draft** GitHub Release named `nresx vX.Y.Z` with auto-generated
  release notes (from PRs/commits since the previous tag) and the three zips
  attached.

Watch the workflow on the Actions tab. Total run time is typically 5-8 minutes.

## 3. Review and publish the draft release

The release is created as a **draft** so you can review before it goes live.

1. Open the draft on the Releases page.
2. Edit the auto-generated notes if needed (highlight breaking changes, big features).
3. Confirm the three zips are attached: `nresx-win-x64.zip`, `nresx-linux-x64.zip`,
   `nresx-osx-arm64.zip`.
4. Hit "Publish release".

The `README.md` download buttons start serving the new zips within seconds -
GitHub's `releases/latest/download/<name>` URLs update the moment the release
is published.

## 4. Push the NuGet packages

This step stays manual (one-off per release, no value in automating yet).

```sh
dotnet pack nresx.Core/nresx.Core.csproj             -c Release
dotnet pack nresx.CommandLine/nresx.CommandLine.csproj -c Release
dotnet nuget push <path-to-.nupkg> -s https://api.nuget.org/v3/index.json -k <api-key>
```

The CommandLine project is a `dotnet tool` package - publishing it makes
`dotnet tool install -g nresx` pick up the new version.

## 5. Verify

- Open `README.md` on GitHub, click a download button, confirm you get the
  new zip.
- `dotnet tool update -g nresx`, then `nresx --version` should show the new number.

## Hotfix flow

For a patch release on an older version: branch from the tag, make the fix,
bump to `X.Y.(Z+1)`, push the new tag - the release workflow handles the rest.

## Local builds and smoke tests

The CI workflow is the production path. Two local helpers exist for
debugging and pre-release confidence-building:

- `scripts/publish-binaries.ps1` - produces the same zips locally in
  `build/release/`. Useful for verifying a publish-config change without
  pushing a tag, or for the docker smoke test below.
- `scripts/smoke-linux-docker.ps1` - runs the linux-x64 zip inside a vanilla
  `ubuntu:latest` container. Catches missing-runtime-dep regressions that the
  GitHub smoke runner won't (the runner has `libicu` preinstalled; a clean
  container doesn't).
