# INI

Extension: `.ini`. Old Windows configuration format, occasionally used for
localization in legacy applications, scripting tools, and game mods.

There is no formal INI spec - dialects vary. nresx targets the common shape.

## File shape

```ini
; semicolons are comments
# hashes are also comments

[menu]
open = Open
save = Save

[dialog]
confirm.title = "Are you sure?"
confirm.body = "This will discard your changes."

; outside any section
title = Application
```

## What's preserved

- Key / value pairs separated by `=`.
- `[section]` headers - the section name is prepended to keys as
  `section.key`. Keys outside any section keep their bare name.
- Comments prefixed with `;` or `#` on lines immediately preceding an entry.
  On save, all comments emit with `;`.
- Values may be quoted with `"..."` - quotes are stripped on load, re-added
  on save when the value needs them (leading/trailing whitespace, contains
  `;` or `#`).
- Escape sequences in values: `\\`, `\n`, `\r`, `\t`.

## What's dropped

- The original section structure on save. nresx writes flat
  `section.key=value` lines rather than re-grouping under `[section]`
  headers. The format round-trips through nresx's reader, but visual
  grouping is lost. The reason: re-grouping would lose context for any
  later non-sectioned keys, since `[section]` headers persist until the
  next one.
- Multi-line values (no INI dialect agrees on continuation syntax).
- The choice of comment prefix (`;` vs `#`) is normalised to `;` on save.

## CLI

```sh
nresx convert config.ini -f json
nresx convert strings.resx -f ini
```

## Notes

- Keys with `=` in them aren't supported (no escape for the separator).
- For applications that depend on `[section]`-grouped output, post-process
  the file after nresx writes it, or use a more structured format.
