# Release process

CI handles the binary build, the GitHub Release creation, and the NuGet push;
triggering the NuGet push and the final "publish the draft" step stay manual
so a human is always in the loop on what reaches users.

## Prerequisites

- Push permission on `svjatpro/nresx.tools`.
- A clean working tree on the release branch (typically `main`).
- NuGet publishing uses Trusted Publishing (OIDC) - no API key. The policy on
  nuget.org (account `svjatpro` → Trusted Publishing) maps this repo +
  `nuget-publish.yml` to a short-lived push credential.

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

Manually triggered, automated execution: run the `nuget-publish` workflow from
the release branch (its csproj `<Version>` must be the version you intend to
publish):

```sh
gh workflow run nuget-publish.yml --ref main
```

The workflow packs both csprojs, exchanges a GitHub OIDC token for a
short-lived nuget.org API key (Trusted Publishing, `NuGet/login@v1`), and
pushes both packages with `--skip-duplicate`. No stored secret is involved;
if the login step fails with "no matching trust policy", check the policy on
nuget.org matches repo owner `svjatpro`, repo `nresx.tools`, workflow file
`nuget-publish.yml`, empty environment.

nuget.org validates/indexes for a few minutes after push. The CommandLine
project is a `dotnet tool` package - publishing it makes
`dotnet tool install -g nresx` pick up the new version.

## 5. Update install channel manifests (`packaging/`)

Both Windows channels pin a versioned download URL + SHA256 of
`nresx-win-x64.zip`, so each release needs a bump:

```sh
curl -sLO https://github.com/svjatpro/nresx.tools/releases/download/vX.Y.Z/nresx-win-x64.zip
sha256sum nresx-win-x64.zip
```

- **Scoop** - `packaging/scoop/nresx.json`: update `version`, `url`, `hash`.
- **Chocolatey** - `packaging/chocolatey/`: update `<version>` +
  `<releaseNotes>` in `nresx.nuspec`, `url64bit` + `checksum64` in
  `tools/chocolateyinstall.ps1`, then:

  ```sh
  cd packaging/chocolatey
  choco pack
  choco push nresx.X.Y.Z.nupkg --source https://push.chocolatey.org/ --api-key <chocolatey.org key>
  ```

  New versions of an approved package go through automated moderation only
  (fast); the first-ever submission also waits for a human moderator.

## 6. Verify

- Open `README.md` on GitHub, click a download button, confirm you get the
  new zip.
- `dotnet tool update -g nresx`, then `nresx --version` should show the new number.
- `scoop install <raw manifest URL>` and `choco install nresx` (after moderation)
  serve the new version.

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
