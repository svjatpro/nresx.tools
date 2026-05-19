# Excel

Extension: `.xlsx`. Microsoft Excel binary format - the lingua franca for
sending translation work to non-technical translators.

Backed by [MiniExcel](https://github.com/mini-software/MiniExcel) (MIT
licensed, no Office or COM dependency).

## File shape

A single sheet named `Resources` with three columns:

| Key      | Value           | Comment              |
| -------- | --------------- | -------------------- |
| greeting | Hello           | shown on the home page |
| farewell | Goodbye         |                      |

The header row is **required** on load - translator tools always emit
headers, and the binary format gives no clean way to detect their absence.
For headerless table exchange use [CSV / TSV](csv-tsv.md).

## What's preserved

- Three columns: `Key`, `Value`, `Comment`. Column names are matched
  case-insensitively.
- Multi-line values inside a single cell (Excel uses `\n` line breaks
  internally).
- Empty comment cells - the entry is loaded with no comment, not with an
  empty string.

## What's dropped

- Anything outside the first sheet.
- Cell formatting (bold, color, font, conditional formatting).
- Merged cells (only the top-left value of a merge is read).
- Formulas (the cached value is read, not the formula).
- Additional columns past Comment.
- Fully blank rows are skipped on load - translator tools sometimes pad
  sheets with empties.

## CLI

```sh
nresx convert strings.resx -f xlsx
nresx convert translations.xlsx strings.po
```

## Notes

- Output is a flat list - no styling, no autofilter, no frozen header row.
  Add those in Excel after export if needed.
- Excel's `xlsx` is unambiguous, so no extra format hint is needed when the
  filename uses the standard extension.
