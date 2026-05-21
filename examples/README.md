# Examples

Small, real-shape walkthroughs of `nresx` in concrete project contexts.
Each subdirectory is self-contained: sample files, the commands you run on
them, and what the result looks like.

If you want broader-pattern recipes (translator handoff, CI integration,
batch ops), see [`docs/recipes/`](../docs/recipes/README.md) instead.
Examples are about *projects*; recipes are about *workflows*.

## Available examples

1. [WinForms - resx ↔ po roundtrip](winforms-resx-po/README.md) - export your
   WinForms `.resx` to `.po` for translators using gettext tools (POEdit,
   Crowdin gettext), import back when they're done.
2. [Web - JSON ↔ po](web-json-po/README.md) - i18next-style locale JSON
   files round-tripped through `.po` for translation services that prefer
   gettext over JSON.
3. [CI validate](ci-validate/README.md) - drop-in GitHub Actions workflow
   that runs `nresx validate` on PRs and fails the build on translation
   drift.
