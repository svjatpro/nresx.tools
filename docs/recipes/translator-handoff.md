# Translator handoff

**Goal.** You have a project with `.resx` files. You want to send them to a
translator who works in Excel, then merge their translated values back into
the project.

The shape: export → translator edits → import. Round-tripping the same key
set is what makes this work - we use `.xlsx` because every translator on
earth can edit it.

## Step 1 - export the source file

Pick the language you want translated. Convert it to `.xlsx`.

```sh
nresx convert strings.en.resx -f xlsx
# writes strings.en.xlsx next to strings.en.resx
```

The xlsx has three columns: `Key`, `Value`, `Comment`. The `Comment` column
is where developer notes / context for the translator goes - either populated
already in your resx, or you can fill them in the spreadsheet before sending.

If you want translators to work on multiple resx files at once, export each
one separately - one resx per xlsx keeps key collisions impossible.

## Step 2 - send the xlsx to the translator

The translator overwrites the `Value` column with translated strings. They
must not touch the `Key` column (keys are the join axis). The `Comment`
column is informational.

Save the returned file as e.g. `strings.de.xlsx`.

## Step 3 - import their translation back

Convert the translated xlsx into your target-language resx.

```sh
nresx convert strings.de.xlsx strings.de.resx
```

`strings.de.resx` is now a complete sibling of `strings.en.resx`, ready to
ship.

## Step 4 - validate the result

Before committing, sanity-check the new file lines up with the source.

```sh
nresx validate strings.en.resx strings.de.resx
```

This flags any keys that exist in `en` but not `de` (the translator missed
them), keys in `de` that aren't in `en` (typos), and entries where the German
value equals the English (probably forgot to translate). Exit code is non-zero
if anything is wrong, so this fits a pre-commit hook.

## Tips

- For ongoing translation work, keep the xlsx files in a separate
  `translations/` directory rather than next to the resx - they're work
  product, not source of truth.
- If a translator returns a CSV instead of xlsx, the flow is identical -
  `nresx convert strings.de.csv strings.de.resx` works the same way.
- For translation management systems (Crowdin, Lokalise, Phrase), use
  [XLIFF](../formats/xliff.md) instead: `nresx convert strings.en.resx -f xliff`.
