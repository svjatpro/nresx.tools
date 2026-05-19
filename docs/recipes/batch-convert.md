# Batch-convert a whole tree

**Goal.** Move a project between formats. The typical reason: switching i18n
frameworks (`.resx` → `.po` because you're moving to gettext, `.json` →
`.yaml` because your new framework prefers YAML), or generating a translator
deliverable in a different format from the source.

`nresx convert` works on globs + `-r` for recursion, so a whole tree is one
command.

## The basic shape

```sh
nresx convert *.resx -f po -r
```

This walks the current directory recursively, finds every `.resx`, and
writes a sibling `.po` next to each one. Original files are untouched.

```
src/
├── Strings.en.resx          # untouched
├── Strings.en.po            # new
├── Strings.de.resx          # untouched
├── Strings.de.po            # new
└── Settings/
    ├── Labels.resx          # untouched
    └── Labels.po            # new
```

## Going in the other direction

```sh
nresx convert *.po -f resx -r
```

Same shape, opposite direction. nresx maps the [shared model](../formats/README.md#cross-format-model)
across formats - keys, values, and the first comment per entry always
survive. Format-specific things (PO plurals, XLIFF source/target,
ARB placeholder metadata) drop on the way through; the [Format reference](../formats/README.md)
calls out exactly what each format loses.

## Pick the output location

By default, output files land next to the input. To collect them in a
sibling directory, pass a directory as the destination:

```sh
nresx convert src/*.resx translations/ -f po -r
```

Each source file is written to `translations/<original-name>.po`. The
directory is created if it doesn't exist.

## When source files should disappear

`nresx convert` never deletes the input. If you're permanently switching
formats, delete the originals once you've verified the output:

```sh
# Convert
nresx convert *.resx -f po -r

# Verify (check counts match, validate the new tree)
nresx validate *.po -r

# Then remove the originals
find . -name '*.resx' -delete    # bash / zsh
Get-ChildItem -Recurse -Filter *.resx | Remove-Item    # PowerShell
```

Keep the conversion + delete in separate commits - it makes rollback easier
if validation catches a problem after the fact.

## When formats are ambiguous

Two cases where `-f` matters:

- **Android `strings.xml`** - the `.xml` extension alone isn't enough.
  Use `-f xml` explicitly when converting *to* Android.
- **CSV vs TSV** - both are tabular. Use the right destination extension or
  `-f csv` / `-f tsv`.

For source format detection, nresx uses the extension. If your files have
weird extensions (`.translation` instead of `.po`), rename them first.

## Tips

- For a one-way migration, run `nresx validate` over the converted tree
  before deleting the originals - drift between the two trees catches any
  format-specific data loss you weren't expecting.
- The `-r` flag is required for recursion; without it, only files in the
  current directory are processed.
- See [format reference](../formats/README.md) for what each conversion
  preserves and drops.
