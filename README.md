# nresx

[![CI](https://github.com/svjatpro/nresx.tools/actions/workflows/smoke.yml/badge.svg)](https://github.com/svjatpro/nresx.tools/actions/workflows/smoke.yml)
[![NuGet: nresx CLI](https://img.shields.io/nuget/v/nresx.svg?label=nresx%20CLI)](https://www.nuget.org/packages/nresx)
[![NuGet: nresx.Core](https://img.shields.io/nuget/v/nresx.Core.svg?label=nresx.Core)](https://www.nuget.org/packages/nresx.Core)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**One command-line tool for every localization file format.** Read, write,
convert, and validate translation resources across 13 formats, on Windows /
Linux / macOS, from any shell.

```sh
# a translator sent back a .po - fold it into your app's locale files
nresx convert strings.uk.po -f json

# gate CI on localization health: drift, duplicates, untranslated entries
nresx validate "locales/strings.*.json" -r --warnings-as-errors
```

## Why nresx

- **Format-agnostic.** po, json, yaml, Android, iOS, resx, xliff and more -
  one model, one command set, any-to-any conversion.
- **CI-friendly by design.** Deterministic output, documented
  [exit codes](nresx.CommandLine/README.md#exit-codes), errors on stderr,
  `validate` as a drop-in pipeline gate.
- **Batch-first.** Every command accepts wildcards and `-r` to sweep whole
  directory trees in one call.
- **Scriptable and embeddable.** The same operations from any shell (Windows /
  Linux / macOS, no .NET visible once installed) or from C# via `nresx.Core` -
  a single `netstandard2.0` build that runs everywhere from .NET Framework 4.6.1
  to the latest .NET, Unity included.

## Install

### Standalone binary (no .NET required)

The `nresx` command-line tool as a single self-contained executable -
download, unzip, run. Grab it from the [latest release](https://github.com/svjatpro/nresx.tools/releases/latest):

| Platform              | Download                                                                                                |
| --------------------- | ------------------------------------------------------------------------------------------------------- |
| Windows (x64)         | [`nresx-win-x64.zip`](https://github.com/svjatpro/nresx.tools/releases/latest/download/nresx-win-x64.zip)       |
| Linux (x64)           | [`nresx-linux-x64.zip`](https://github.com/svjatpro/nresx.tools/releases/latest/download/nresx-linux-x64.zip)   |
| macOS (Apple Silicon) | [`nresx-osx-arm64.zip`](https://github.com/svjatpro/nresx.tools/releases/latest/download/nresx-osx-arm64.zip)   |

After extracting, add the directory to your PATH so `nresx` is callable from any shell:

<details><summary>Windows</summary>

```powershell
# Extract to C:\Tools\nresx, then:
[Environment]::SetEnvironmentVariable("Path", $env:Path + ";C:\Tools\nresx", "User")
# Restart your shell. Confirm:
nresx --version
```

</details>

<details><summary>Linux / macOS</summary>

```sh
# Extract to ~/.local/bin (already on PATH on most distros) or /usr/local/bin
unzip nresx-linux-x64.zip -d ~/.local/bin/
chmod +x ~/.local/bin/nresx
nresx --version
```

**Linux runtime dependency**: requires `libicu` (standard .NET globalization library). Most desktop distros (Ubuntu, Debian, Fedora) include it by default. On minimal containers / slim server images, install it:

```sh
# Debian / Ubuntu
sudo apt-get install -y libicu-dev

# Fedora / RHEL / Rocky
sudo dnf install -y libicu

# Alpine is not supported - the binary needs glibc, not musl
```

On macOS, Gatekeeper may quarantine the binary on first run. Remove the quarantine flag:

```sh
xattr -d com.apple.quarantine ~/.local/bin/nresx
```

</details>

### CLI via dotnet tool

If you already have the .NET SDK:

```sh
dotnet tool install -g nresx
nresx --help
```

### .NET library

The same engine as a package, for use from C# code:

```sh
dotnet add package nresx.Core
```

### Planned channels

Coming with the 1.0 release: **Chocolatey** (`choco install nresx`) and
**Scoop** (`scoop install nresx`). Homebrew is planned for a later release.

## [Supported formats](docs/formats/README.md)

Ordered roughly by ecosystem reach. Click the heading above for per-format
details: what's preserved, what's lossy, and format-specific quirks. More
formats are on the roadmap.

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

## Quick start

### CLI

```sh
# Convert a single file into another format (placed next to the source)
nresx convert strings.po -f json

# Convert every .resx in the current directory tree into a .yaml sibling
nresx convert *.resx -f yaml -r

# Show format, culture, and element count for a file
nresx info strings.json

# Print every key=value pair (use -t to customize the row template)
nresx list strings.json

# Surface drift, duplicates, and empty entries across language siblings
# (non-zero exit on errors; --warnings-as-errors makes it strict)
nresx validate strings.*.json -r
```

Full [command reference](nresx.CommandLine/README.md).

### .NET library

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

## Docs

- [Getting started](docs/getting-started.md) - install, first conversions, library walkthrough.
- [Command reference](nresx.CommandLine/README.md) - every CLI command, flags, exit codes.
- [Format reference](docs/formats/README.md) - per-format roundtrip and quirks.
- [API reference](docs/api/README.md) - full `nresx.Core` API.
- [Recipes](docs/recipes/README.md) - translator handoff, CI validation, batch ops.
- [Worked examples](examples/README.md) - WinForms, web, and CI project shapes.

## License

[MIT](LICENSE)
