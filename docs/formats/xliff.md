# XLIFF 1.2

Extensions: `.xlf`, `.xliff`. OASIS-standard translation interchange format,
widely supported by translation management systems (Crowdin, Lokalise,
Phrase, memoQ, SDL Trados, smartling).

XLIFF 2.0 exists but TMS adoption lags - nresx targets 1.2 for the
broadest compatibility.

## File shape

```xml
<?xml version="1.0" encoding="UTF-8"?>
<xliff xmlns="urn:oasis:names:tc:xliff:document:1.2" version="1.2">
  <file source-language="en" target-language="fr" datatype="plaintext" original="messages">
    <body>
      <trans-unit id="greeting">
        <source>Hello</source>
        <target>Bonjour</target>
        <note>shown on the home page</note>
      </trans-unit>
    </body>
  </file>
</xliff>
```

## What's preserved

- `<trans-unit id="...">` → key.
- `<target>` if present and non-empty, otherwise `<source>` → value. (See
  "Source vs target" below.)
- `<note>` → comment.
- `<file target-language="...">` from the first `<file>` element →
  `file.Headers["language"]`. On save the value is written back to
  `target-language`.

## What's dropped

- Inline elements inside `<source>` / `<target>` (`<g>`, `<x/>`, `<ph>`) -
  XLIFF inline markup for placeholders is flattened to its text content on
  load.
- `<context>`, `<context-group>`, segment metadata, alt-trans, status
  attributes, approved flags.
- Multiple `<file>` elements with different `target-language` values -
  language is taken from the first `<file>` only.

## Source vs target

XLIFF natively separates the source language from the target. nresx's model
is "one file = one language", so:

- **Load** - prefer `<target>` (assumed to be the translated value); fall
  back to `<source>` when target is empty.
- **Save** - write the value to both `<source>` and `<target>`. Source
  language is hard-coded to `en`; target language comes from
  `file.Headers["language"]`.

This is lossy versus a full XLIFF workflow. If you need to preserve the
source/target split, edit the XML directly or use a dedicated TMS client.

## CLI

```sh
nresx convert strings.resx -f xliff
nresx convert messages.xlf strings.po
```

## Notes

- `.xlf` and `.xliff` both map to XLIFF 1.2; the extension only decides the
  default output extension on convert.
- For monolingual exchange (single-language `.xlf`), the source/target
  duplication is benign - TMS importers handle it without complaint.
