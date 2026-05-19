# Plain text

Extension: `.txt`. Last-resort dump format - one value per blank-line-separated
block.

This is not a real localization format. It exists mostly for diagnostic
output and as a convenient drop-target when you don't care about keys at all
(rare). For real interchange use [JSON](json.md), [CSV / TSV](csv-tsv.md), or
[XLSX](xlsx.md).

## File shape

```text
Hello, world

Goodbye,
see you later

Another value
```

Blocks are separated by blank lines. A block can span multiple lines.

## What's preserved

- Values. That's it.

## What's dropped

- Keys - on load, the value text doubles as the key (effectively keyless).
- Comments - the format has no comment syntax.

## When to use it

- Pulling raw text out of a resource file for inspection or external diffing.
- Hand-editing a flat list of strings without worrying about keys.

## When not to use it

- Anything that requires round-tripping through a translator. The lack of
  keys makes correlation impossible after the file is touched.

## CLI

```sh
nresx convert strings.resx -f txt
```

## Notes

- `ElementHasKey` and `ElementHasComment` are both `false` for this format,
  so commands that operate on keys (e.g. `add`, `update`, `remove -k <key>`)
  are not useful here.
