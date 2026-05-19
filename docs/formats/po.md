# Gettext PO

Extension: `.po`. The GNU gettext interchange format. Used by Django, Symfony,
WordPress, KDE, GNOME, and most CMS-style projects with i18n needs. Crowdin,
Lokalise, Phrase, and Transifex all import/export PO.

## File shape

```po
# translator comment
#. extracted-comment (from source)
#: src/handler.cs:42
#, c-format
msgctxt "menu"
msgid "Open"
msgstr "Otvori"

msgid "1 file"
msgid_plural "{0} files"
msgstr[0] "1 datoteka"
msgstr[1] "{0} datoteke"
msgstr[2] "{0} datoteka"
```

## What's preserved

- `msgid` → key, `msgstr` → value.
- `msgctxt` → element context (separate field on `ResourceElement`).
- Each comment kind is preserved with its prefix:
  - `#` translator comment
  - `#.` extracted comment
  - `#:` reference (source location)
  - `#,` flags (e.g. `c-format`, `fuzzy`)
  - `#|` previous value
- Plural forms: `msgid_plural` and `msgstr[N]` round-trip via
  `element.KeyPlural` and `element.ValuePlurals`.
- The file header block (empty-`msgid` entry with `Project-Id-Version`,
  `Language`, `Plural-Forms`, etc.) is loaded into `file.Headers` and emitted
  on save.
- Multi-line values are emitted as quoted continuation lines with `\n`
  escapes - the standard PO encoding.

## What's dropped

- Obsolete entries (`#~ msgid ...`).
- Comments with prefixes outside the five listed above are ignored.

## Options

```csharp
var options = new ResourceFileOptionPo
{
    IgnoreEmptyHeaders = false, // keep header lines whose value is empty (default: drop)
};
```

## CLI

```sh
nresx convert strings.resx -f po
nresx convert strings.po -f json
```

## Cross-format conversion

When converting *to* a format that doesn't carry context, plurals, or
gettext-specific comment kinds, those fields are dropped. Going PO → resx
keeps only key / value / first comment.

Going *from* a non-PO format to PO produces a PO file with no `msgctxt`, no
plurals, and a single translator comment per entry.
