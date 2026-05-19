# nresx

Multi-format resource file library and CLI for .NET. Read, write, convert, and
validate localization files across 13 formats with a single dependency.

## What it is

Two things in one repository:

- **`nresx.Core`** - a netstandard2.0 library for loading, manipulating, and
  saving resource files. Use it from any .NET app (Framework 4.6.1+, Core 2.0+,
  Mono, Xamarin, Unity).
- **`nresx`** - a CLI wrapping the library for convert / inspect / validate /
  batch operations on the file system.

## Supported formats

| Format            | Extensions          |
| ----------------- | ------------------- |
| .NET resx / resw  | `.resx` `.resw`     |
| YAML              | `.yaml` `.yml`      |
| JSON              | `.json`             |
| Gettext PO        | `.po`               |
| XLIFF 1.2         | `.xlf` `.xliff`     |
| Android strings   | `.xml`              |
| iOS strings       | `.strings`          |
| Java properties   | `.properties`       |
| INI               | `.ini`              |
| CSV / TSV         | `.csv` `.tsv`       |
| Flutter ARB       | `.arb`              |
| Excel             | `.xlsx`             |
| Plain text        | `.txt`              |

## Install

### CLI

```sh
dotnet tool install -g nresx
nresx --help
```

Requires the .NET SDK. Standalone binaries for win/linux/mac are planned for
a future release.

### Library

```sh
dotnet add package nresx.Core
```

## Quick start

### CLI

```sh
# Convert a .resx to .po
nresx convert strings.resx -f po

# Convert every .resx in a tree to .yaml
nresx convert *.resx -f yaml -r

# Inspect a file
nresx info strings.resx

# Print every key/value
nresx list strings.resx

# Validate (empty entries, duplicates, drift between languages)
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

Async overloads are available: `ResourceFile.LoadAsync` and
`file.SaveAsync` both accept a `CancellationToken`.

Longer walkthrough: [`docs/getting-started.md`](docs/getting-started.md).

## License

[MIT](LICENSE)
