# Validate translations in CI

**Goal.** Fail the build when someone commits a resource file with broken
keys, drift between language siblings, duplicates, or untranslated entries.

`nresx validate` is built for this: it prints diagnostics to stdout, exits
non-zero on errors, and accepts whole trees in one invocation.

## What it checks

- **Empty keys / values** - entries that have no key or no value.
- **Duplicate keys** - same key appearing twice in one file.
- **Drift across language siblings** - `strings.en.resx` has `confirm.title`
  but `strings.de.resx` doesn't (`MissedElement`).
- **Untranslated entries** - the German file has the same value as the
  English file for some key (`NotTranslated`).

Drift and untranslated checks only run when the same group has multiple
language files. The base language is auto-detected (neutral file first, then
English, then alphabetical first) or you can pin it with `--basic-lan`.

## Step 1 - run it locally

```sh
nresx validate strings.*.resx -r
```

`-r` recurses into subdirectories. Output looks like:

```
strings.de.resx: error: MissedElement: confirm.title
strings.de.resx: warning: NotTranslated: app.name
Found 2 issues (1 error, 1 warning)
```

By default the exit code is non-zero on errors but zero on warnings. To make
the build also fail on warnings:

```sh
nresx validate strings.*.resx -r --warnings-as-errors
```

## Step 2 - wire it into GitHub Actions

```yaml
name: ci

on: [push, pull_request]

jobs:
  validate-translations:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      - name: Install nresx
        run: dotnet tool install -g nresx
      - name: Validate translations
        run: nresx validate src/*.resx -r --warnings-as-errors
```

The job fails the PR if anyone commits drift. The `--warnings-as-errors` flag
is opinionated - keep it off if you don't want to block on untranslated
entries (e.g. work-in-progress strings ship with the English value).

## Step 3 - pre-commit hook (optional)

For local prevention, a one-line git pre-commit hook:

```sh
# .git/hooks/pre-commit
#!/bin/sh
nresx validate *.resx -r || exit 1
```

Or via [pre-commit.com](https://pre-commit.com) framework:

```yaml
repos:
  - repo: local
    hooks:
      - id: nresx-validate
        name: Validate localization files
        entry: nresx validate
        language: system
        files: \.(resx|po|json)$
        pass_filenames: true
```

## Tips

- `--basic-lan en-US` pins the base file to a specific culture. Useful when
  your neutral resx isn't English, or when auto-detection picks the wrong
  one.
- The validator works the same way on `.po`, `.json`, `.yaml`, etc. - the
  underlying checks are format-agnostic.
- Want machine-parseable output for custom CI tooling? Run `nresx validate`
  inside a script and parse the per-line format `path: severity: type: key`.
