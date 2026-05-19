# iOS / macOS strings

Extension: `.strings`. Apple's native `.strings` format used in iOS, macOS,
watchOS, and tvOS apps, typically under `Localizable.strings` in `*.lproj`
directories.

## File shape

```
/* Comment for next entry */
"greeting" = "Hello";

// also-valid line comment
"farewell" = "Goodbye, %@";
"with newline" = "Line one\nLine two";
```

## What's preserved

- Quoted key / value pairs.
- Both comment styles - `/* ... */` block comments and `// ...` line comments
  preceding an entry attach to that entry. On save nresx always emits
  block-style comments.
- Escape sequences inside quoted strings: `\\`, `\"`, `\'`, `\n`, `\r`, `\t`,
  `\0`. Unescaped on load, re-escaped on save.

## What's dropped

- Trailing comments after `;` on the same line.
- Comments not immediately preceding an entry.
- UTF-16 input - the format is canonically UTF-16 LE on Apple platforms, but
  modern Xcode reads UTF-8 fine and nresx writes UTF-8 without BOM. Re-encode
  if your build pipeline expects UTF-16.

## CLI

```sh
nresx convert Localizable.strings -f resx
nresx convert strings.po Localizable.strings
```

## Notes

- The file extension `.strings` is unambiguous, so no extra format hint is
  needed.
- `*/` inside a comment is escaped to `* /` on save to keep the block-comment
  delimiter intact.
