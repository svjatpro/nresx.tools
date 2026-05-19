# Library API reference

Auto-generated from XML doc comments in `nresx.Core`. The [type index](index.md)
lists every public type; each type has its own page with properties, methods,
constructors, and inheritance.

If you're new to the library, read [Getting started](../getting-started.md)
first - it covers the most common load / mutate / save flow before you dive
into individual types.

## Starting points

The library is built around three types. Open these first:

- [ResourceFile](nresx.core.resourcefile.md) - the file model. Load, manipulate
  `Elements`, save.
- [ResourceElement](nresx.core.resourceelement.md) - one key/value/comment row.
- [ResourceFormatType](nresx.core.resourceformattype.md) - identifies a
  format. Passed to `Save` when writing in a different format.

Beyond that:

- [ResourceFileOption](nresx.core.resourcefileoption.md) and the per-format
  derivatives ([Json](nresx.core.resourcefileoptionjson.md),
  [Po](nresx.core.resourcefileoptionpo.md),
  [Resx](nresx.core.resourcefileoptionresx.md)) for tuning load/save.
- [LoadMode](nresx.core.loadmode.md) - strict / lenient / raw validation
  behaviour on load.

## How to regenerate

```pwsh
dotnet tool restore                       # first time only, pins xmldoc2md from .config/dotnet-tools.json
powershell -File scripts\gen-api-docs.ps1 # Windows PowerShell 5.1
# or: pwsh -File scripts/gen-api-docs.ps1 # PowerShell 7+
```

The script builds `nresx.Core` in Release, resolves transitive dependencies,
and runs `xmldoc2md` against the produced `nresx.Core.dll` + `nresx.Core.xml`.
The output goes to `docs/api/`. This file (`README.md`) is preserved across
regenerations; all other `*.md` files are replaced.

Generated content is committed so the API is browseable straight from GitHub
without a build step. Regenerate after every API-affecting change.
