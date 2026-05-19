# Format reference

One page per supported format. Each page covers: what file shape nresx reads
and writes, what it preserves through a roundtrip, what it drops, and any
format-specific quirks worth knowing.

Formats are listed by ecosystem reach, same order as the project [README](../../README.md).

| Format            | Extensions      | Page                                       |
| ----------------- | --------------- | ------------------------------------------ |
| JSON              | `.json`         | [json.md](json.md)                         |
| .NET resx / resw  | `.resx` `.resw` | [resx.md](resx.md)                         |
| YAML              | `.yaml` `.yml`  | [yaml.md](yaml.md)                         |
| Gettext PO        | `.po`           | [po.md](po.md)                             |
| Android strings   | `.xml`          | [android.md](android.md)                   |
| iOS strings       | `.strings`      | [ios.md](ios.md)                           |
| Java properties   | `.properties`   | [java-properties.md](java-properties.md)   |
| XLIFF 1.2         | `.xlf` `.xliff` | [xliff.md](xliff.md)                       |
| CSV / TSV         | `.csv` `.tsv`   | [csv-tsv.md](csv-tsv.md)                   |
| Excel             | `.xlsx`         | [xlsx.md](xlsx.md)                         |
| Flutter ARB       | `.arb`          | [arb.md](arb.md)                           |
| INI               | `.ini`          | [ini.md](ini.md)                           |
| Plain text        | `.txt`          | [plain-text.md](plain-text.md)             |

## Cross-format model

nresx maps every format onto one model:

- **Key** - string identifier.
- **Value** - string content.
- **Comment** - optional translator/developer note attached to the entry.
- **Headers** - optional file-level metadata (only a few formats carry these:
  PO, XLIFF).

Anything outside that shape (plurals, placeholders, type metadata, binary
blobs, font/color resources, nested mappings) is documented per format as
"not preserved".

## Roundtrip semantics

Two cases worth distinguishing on each page:

- **Same-format roundtrip** - load `foo.json` and save it back. Keys / values /
  comments survive; non-modeled metadata (e.g. ARB placeholders, XLIFF source
  vs target) is dropped.
- **Cross-format conversion** - load `foo.resx`, save `foo.po`. Limited to the
  shared model. Quirks of either format that don't fit are dropped.

When a format has a lossy convert path - typically formats that natively carry
metadata richer than the model (XLIFF, ARB, PO plurals) - the page calls it
out explicitly.

## Encoding

All text formats are read as UTF-8 (BOM auto-detected) and written as UTF-8
without BOM, except `.resx` / `.resw` which write the XML declaration's
default UTF-8 encoding.

Binary formats (`.xlsx`) handle encoding internally.
