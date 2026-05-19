# Java properties

Extension: `.properties`. Used by Java applications, Spring Boot, IntelliJ
plugins, and various JVM frameworks.

## File shape

```properties
# greeting shown on the home page
greeting = Hello

! exclamation marks are also comments
farewell : Goodbye
multi.line = first line\
             continues here
unicode.value = Café
```

## What's preserved

- Key / value pairs separated by `=`, `:`, or whitespace.
- Comments prefixed with `#` or `!` on lines immediately preceding an entry.
  On save, multi-line comments emit one `#` line per line.
- Escape sequences: `\n`, `\r`, `\t`, `\f`, `\\`, `\=`, `\:`, `\` (space), and
  `\uXXXX` Unicode escapes.
- Multi-line continuations - lines ending in an odd-count trailing `\` are
  joined on load.

## What's dropped

- The original separator choice (`=`, `:`, whitespace). Save always emits `=`.
- Line continuations on save - multi-line values are encoded as `\n` escapes
  on a single line.

## Encoding

The classic Java spec mandates ISO-8859-1 with `\uXXXX` escapes for non-Latin
characters. Modern tooling (Spring, Java 9+) defaults to UTF-8. nresx reads
and writes UTF-8 (BOM auto-detected on read). If you target a Java runtime
that expects ISO-8859-1, the `\uXXXX` escapes nresx emits will still work,
but high-bit UTF-8 characters may not - convert externally if needed.

## CLI

```sh
nresx convert messages.properties -f json
nresx convert messages.resx -f properties
```

## Notes

- CR characters in values are stripped on save (Java's `Properties.load` is
  tolerant but keeping them invites trouble).
- Keys with `=`, `:`, or leading whitespace are escaped on save.
