# Android strings

Extension: `.xml`. Native Android resource format, typically `strings.xml`
under `res/values/`, `res/values-fr/`, etc.

## File shape

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
  <!-- shown on the home page -->
  <string name="greeting">Hello</string>
  <string name="farewell">Goodbye, see you later\nMore on the next line</string>
</resources>
```

## What's preserved

- `<string name="...">` entries: key, value, and immediately preceding
  `<!-- comment -->` (whitespace skipped).
- Android escape sequences in values: `\\`, `\'`, `\"`, `\n`, `\t`. These are
  unescaped on load and re-escaped on save.

## What's dropped

- `<string-array>` and `<plurals>` - they don't fit the single-value entry
  shape. Skipped on load, never emitted on save.
- `<resources>` attributes (`tools:ignore`, `xmlns:tools`).
- Standalone XML comments not directly preceding a `<string>`.

## Detection

The `.xml` extension is ambiguous (many formats use it). nresx uses
`AndroidStrings` only when the project enum / `ResourceFormatType` is set
explicitly, or when the file is passed through the format-by-extension
heuristic with no other XML format claiming it. If you have a Windows
`.resx` mis-named as `.xml`, force the format with the library API.

## CLI

```sh
nresx convert strings.xml strings.po       # by destination format
nresx convert strings.resx -f xml          # writes Android strings.xml
```

## Notes

- nresx writes a UTF-8 XML declaration and no `<resources>` attributes - if
  your build needs `xmlns:tools` or similar, post-process the file or load it
  with another tool.
- CR characters in values are stripped on save (Android tooling rejects `\r`
  inside string elements).
