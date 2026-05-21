# CI validate

**Scenario.** You want your CI to fail when someone commits localized
files that have broken keys, drift between language siblings, or
untranslated entries - so translation bugs don't reach production.

`nresx validate` is built for this: it exits non-zero on errors, accepts
glob patterns and whole trees in one invocation, and works on every format
nresx supports (resx, po, json, yaml, ...).

## What's in this directory

- [`.github/workflows/validate-i18n.yml`](.github/workflows/validate-i18n.yml) -
  drop-in GitHub Actions workflow. Copy it into your own repository's
  `.github/workflows/` directory and adjust the glob to match where your
  translation files live.

## How to use it

1. Copy `.github/workflows/validate-i18n.yml` into your repository.
2. Edit the last `nresx validate ...` line so the glob matches your
   translation tree. Examples:

   | Project shape                          | Glob                                          |
   | -------------------------------------- | --------------------------------------------- |
   | .NET resx                              | `"src/Resources/*.resx" -r`                   |
   | i18next (flat)                         | `"locales/*.json"`                            |
   | i18next (nested per-language)          | `"public/locales/**/*.json"`                  |
   | Android                                | `"app/src/main/res/values*/strings.xml" -r`   |
   | Mixed (resx + po side by side)         | `"src/**/*.resx" "translations/*.po" -r`      |

3. Commit and push. The job runs on every PR and push to `main` / `master`.

## What gets caught

- **Empty keys / values** - entries with no key or no value.
- **Duplicate keys** - same key appearing twice in one file.
- **Drift between language siblings** - `strings.en.resx` has
  `confirm.title` but `strings.de.resx` doesn't.
- **Untranslated entries** - the German file has the same value as the
  English file for some key (this is a *warning* by default; the job still
  passes unless you opt in to `--warnings-as-errors`).

## Variants

**Fail on untranslated entries too** (strict mode):

```yaml
- name: Validate translations
  run: nresx validate "src/Resources/*.resx" -r --warnings-as-errors
```

**Pin the base language** explicitly (when auto-detection isn't quite
right):

```yaml
- name: Validate translations
  run: nresx validate "src/Resources/*.resx" -r --basic-lan en-US
```

**Pre-commit hook** (catches issues before they reach CI):

```sh
# .git/hooks/pre-commit
#!/bin/sh
nresx validate "*.resx" -r || exit 1
```

## See also

- [CI validation recipe](../../docs/recipes/ci-validation.md) - fuller
  walkthrough including a pre-commit.com integration and how to interpret
  validate output line-by-line.
- [Command reference](../../nresx.CommandLine/README.md) - all `validate`
  flags and their semantics.
