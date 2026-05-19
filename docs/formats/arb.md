# Flutter ARB

Extension: `.arb`. Application Resource Bundle, used by Flutter's
`intl`/`flutter_localizations` packages. JSON-based with sibling metadata
keys.

## File shape

```json
{
  "@@locale": "en",
  "greeting": "Hello",
  "@greeting": {
    "description": "shown on the home page",
    "placeholders": {
      "name": { "type": "String" }
    }
  },
  "farewell": "Goodbye"
}
```

- `@@locale`, `@@last_modified`, etc. - global metadata (top-level `@@*`).
- `key` / `value` - the translation entry.
- `@key` - optional per-entry metadata block (description, placeholders,
  type, context).

## What's preserved

- Top-level string entries (key → value).
- The `description` field inside `@key` → element comment.

## What's dropped

- `@@locale` and other `@@global` metadata. (If the file is part of a
  multi-locale Flutter project, the locale is encoded in the filename
  (`app_en.arb`), not in nresx's model.)
- Placeholder metadata inside `@key` blocks (`placeholders`, `type`,
  `context`, `example`). On save, only `description` is re-emitted.
- Nested objects under non-`@` keys (treated as not a string entry, skipped).

## Roundtrip note

Same lossy pattern as XLIFF: load → save preserves keys, values, and the
`description` of `@key` blocks, but drops any placeholder typing. If your
Flutter project relies on typed placeholders, edit the ARB directly or use
`intl_translation` for that step.

## CLI

```sh
nresx convert strings.resx -f arb
nresx convert app_en.arb strings.json
```

## Notes

- The format is JSON underneath - any JSON-aware editor or linter will work
  on the output.
- Duplicate keys at the top level are preserved on load (uses a token-level
  scan rather than `JObject.Parse`, which would collapse them); typical JSON
  consumers and Flutter itself would treat duplicates as an error.
