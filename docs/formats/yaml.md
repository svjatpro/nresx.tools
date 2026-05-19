# YAML

Extensions: `.yaml`, `.yml`. Common in Ruby/Rails i18n, Hugo, Jekyll, and as a
human-friendly exchange format.

## File shape

```yaml
# greeting shown on the home page
greeting: Hello

# leading comment block
# attaches to the next entry
farewell: |
  Goodbye,
  see you later
```

nresx reads top-level `key: value` mappings, including block scalars (`|` and
`>` for multi-line values). Nested mappings are not extracted.

## What's preserved

- Keys and values (including multi-line values via block scalars).
- Leading `#` comment blocks that immediately precede an entry (blank-line
  bounded). On save, multi-line comments emit one `#` line per line.

## What's dropped

- Nested mappings and sequences (only top-level scalar pairs are treated as
  entries).
- Inline / trailing comments (`key: value  # trailing`).
- YAML anchors and aliases (`&anchor`, `*alias`).
- Document separators (`---`, `...`).
- Explicit type tags (`!!str`, `!!int`).

## CLI

```sh
nresx convert strings.yaml -f json
nresx convert strings.resx -f yaml
```

## Notes

- Output uses standard YAML quoting rules. Multi-line values are emitted as
  block scalars (`|`) when they contain newlines.
- Both `.yaml` and `.yml` map to the same format; the extension is only used
  to decide the default output extension on convert.
