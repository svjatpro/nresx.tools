# nresx

Toolkit for localization resource files. Read, write, convert, and validate
files across various formats. Ships as a **.NET library** (for use from .NET
apps) and a **cross-platform CLI** (run it from any shell - no .NET runtime
visible to the user once installed).

## What it is

- **`nresx.Core`** - .NET library targeting `netstandard2.0`. Works on .NET
  Framework 4.6.1+, .NET Core 2.0+, Mono, Xamarin, and Unity.
- **`nresx`** - cross-platform CLI for Windows / Linux / macOS. Convert
  between formats, inspect files, add/update/remove keys, batch-process
  whole trees, validate for drift and duplicates.

## Supported formats

Ordered roughly by ecosystem reach. More formats are on the roadmap.

| Format            | Extensions          |
| ----------------- | ------------------- |
| JSON              | `.json`             |
| .NET resx / resw  | `.resx` `.resw`     |
| YAML              | `.yaml` `.yml`      |
| Gettext PO        | `.po`               |
| Android strings   | `.xml`              |
| iOS strings       | `.strings`          |
| Java properties   | `.properties`       |
| XLIFF 1.2         | `.xlf` `.xliff`     |
| CSV / TSV         | `.csv` `.tsv`       |
| Excel             | `.xlsx`             |
| Flutter ARB       | `.arb`              |
| INI               | `.ini`              |
| Plain text        | `.txt`              |

## Install

Multiple install channels are landing for 1.0. Available today:

### CLI via dotnet tool

```sh
dotnet tool install -g nresx
nresx --help
```

Requires the .NET SDK to install; the resulting `nresx` command runs anywhere
the .NET 9 runtime is available.

### Library

```sh
dotnet add package nresx.Core
```

### Coming in 1.0

- **Standalone binaries** (no .NET required) - direct download from the
  GitHub Releases page, for Windows / Linux / macOS
- **Chocolatey** - `choco install nresx`
- *Additional channels under discussion; see the phase plan.*

## Quick start

### CLI

```sh
# Convert a single .resx into a .po file (placed next to the source)
nresx convert strings.resx -f po

# Convert every .resx in the current directory tree into a .yaml sibling
nresx convert *.resx -f yaml -r

# Show format, culture, and element count for a file
nresx info strings.resx

# Print every key=value pair (use -t to customize the row template)
nresx list strings.resx

# Surface drift, duplicates, and empty entries across language siblings (non-zero exit on issues)
nresx validate strings.*.resx -r
```

Full command reference: [`nresx.CommandLine/README.md`](nresx.CommandLine/README.md).

### Library

```csharp
using nresx.Core;

// Load
var file = new ResourceFile("strings.resx");

foreach (var element in file.Elements)
    Console.WriteLine($"{element.Key} = {element.Value}");

// Mutate
file.Elements.Add("hello", "Hello, world!");

// Save as a different format
file.Save("strings.po", ResourceFormatType.Po);
```

Async overloads are available: `ResourceFile.LoadAsync` and `file.SaveAsync`
both accept a `CancellationToken`.

Longer walkthrough: [`docs/getting-started.md`](docs/getting-started.md).

## License

[MIT](LICENSE)
