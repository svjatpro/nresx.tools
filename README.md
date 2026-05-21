# nresx

Toolkit for localization resource files. Read, write, convert, and validate
files across 13+ formats. Ships as a **cross-platform CLI** (run it from any
shell, no .NET visible to the user once installed) and a **.NET library**
(for use from .NET apps).

## What it is

- **`nresx`** - cross-platform CLI for Windows / Linux / macOS. Convert
  between formats, inspect files, add/update/remove keys, batch-process
  whole trees, validate for drift and duplicates.
- **`nresx.Core`** - .NET library targeting `netstandard2.0`. Works on .NET
  Framework 4.6.1+, .NET Core 2.0+, Mono, Xamarin, and Unity.

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

## Install

Available channels:

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

### Standalone binary (no .NET required)

Single-file self-contained executable. Download from the [latest release](https://github.com/svjatpro/nresx.tools/releases/latest):

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

### Roadmap

Additional channels planned for a future release:

- **Chocolatey** - `choco install nresx`
- **Homebrew** - `brew install nresx`
- **Scoop** - `scoop install nresx`

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

Full [command reference](nresx.CommandLine/README.md).

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

More:

- [Getting started](docs/getting-started.md) - longer walkthrough.
- [API reference](docs/api/README.md) - full `nresx.Core` API.
- [Recipes](docs/recipes/README.md) - common workflows (translator handoff, CI validation, batch ops).
- [Worked examples](examples/README.md) - WinForms, web, and CI project shapes.

## License

[MIT](LICENSE)
