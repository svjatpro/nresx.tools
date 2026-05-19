# Recipes

Practical, copy-pasteable walkthroughs for real localization workflows. Each
recipe is self-contained: the goal up front, the commands, and what to watch
out for.

If you're new to the tool, read [Getting started](../getting-started.md) first
for install and the basic CLI/library tour.

## Recipes

1. [Translator handoff](translator-handoff.md) - extract `.resx` to Excel for a
   translator, import their work back when they're done.
2. [Validate translations in CI](ci-validation.md) - run `nresx validate` as a
   build step that fails the pipeline on drift, duplicates, or untranslated
   entries.
3. [Find missing translations](find-missing-translations.md) - spot keys that
   exist in one language file but not the others, across a whole `resx`/`po`
   tree.
4. [Batch-convert a whole tree](batch-convert.md) - move a project between
   formats (e.g. `resx` → `po` or `json` → `yaml`) in one pass.
5. [Bulk edits from code](bulk-edit.md) - rename keys, strip prefixes, dedupe,
   or apply any programmatic transform across many files via the library.

## Conventions

- All CLI examples assume `nresx` is on PATH (see [install](../getting-started.md#install)).
- File trees use forward slashes; on Windows, both `/` and `\` work.
- Glob patterns (`*.resx`, `strings.*.resx`) are expanded by `nresx` itself, not
  the shell, so the same patterns work on Windows PowerShell, cmd, bash, and zsh.
