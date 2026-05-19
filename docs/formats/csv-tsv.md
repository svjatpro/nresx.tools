# CSV / TSV

Extensions: `.csv` (comma-separated), `.tsv` (tab-separated). Translator-friendly
exchange format - any spreadsheet program can edit either.

## File shape

```csv
key,value,comment
greeting,"Hello, world","shown on the home page"
multi,"first line
second line",
empty,,
```

```tsv
key	value	comment
greeting	Hello, world	shown on the home page
```

Three columns in this order: `key`, `value`, `comment`. The header row is
optional - if the first row's columns are `key` and `value` (case-insensitive),
it's treated as a header; otherwise it's data.

## What's preserved

- Three-column rows.
- RFC 4180 quoting: fields containing the delimiter, double quotes, CR, or LF
  are wrapped in `"..."`; embedded `"` is doubled (`""`).
- Multi-line values via quoted fields (LF inside the quotes).
- Blank lines are skipped on load.

## What's dropped

- Columns past the third are ignored.
- Rows with empty keys are skipped.

## Save format

nresx always emits a header row (`key,value,comment` or
`key\tvalue\tcomment`) so files round-trip cleanly through translator tools
that expect headers.

## CLI

```sh
nresx convert strings.resx -f csv
nresx convert strings.json -f tsv
nresx convert translations.csv strings.po
```

## Notes

- CSV and TSV share the same parser; only the delimiter differs.
- For Excel-friendly exchange that includes header styling or multiple
  sheets, use the [Excel format](xlsx.md) instead.
- CSV produced by Excel may use `\r\n` line endings; nresx handles both LF
  and CRLF on input.
