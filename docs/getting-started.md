# Getting started

This page walks through the first hour with nresx: installing the CLI, running
your first conversions, and using the library from C# code.

## Install

### CLI

```sh
dotnet tool install -g nresx
```

The `nresx` command is now available globally. Verify:

```sh
nresx --version
nresx --help
```

Requires the .NET 9 SDK (or newer) on PATH.

### Library

In any .NET project:

```sh
dotnet add package nresx.Core
```

Targets `netstandard2.0` - the widest-compatibility base: one build runs
everywhere from .NET Framework 4.6.1 to the latest .NET, Unity included.

## Concepts

- **`ResourceFile`** - the in-memory model of a resource file. Loaded from a
  path or stream, manipulated, then saved.
- **`ResourceElement`** - one key/value/comment row inside a file.
- **`ResourceFormatType`** - enum of the supported formats. Used when you want
  to save in a format different from the source.
- **Headers and file-level comments** - PO-style metadata that some formats
  carry; round-tripped where the format supports it.

## CLI: five common workflows

### 1. Convert a single file

```sh
nresx convert strings.resx -f po
# writes strings.po next to strings.resx
```

Or set an explicit destination:

```sh
nresx convert strings.resx strings.po
```

### 2. Batch-convert a whole tree

```sh
nresx convert *.resx -f yaml -r
```

`-r` recurses; every `.resx` under the current directory becomes a sibling
`.yaml` with the same key/value content.

### 3. List or inspect

```sh
nresx info strings.resx          # summary: format, culture, element count
nresx list strings.resx          # every key=value
nresx list strings.resx -t "\k\t\v\t\c"   # tab-separated with comments
```

### 4. Add / update / remove entries

```sh
nresx add strings.resx -k confirm.title -v "Are you sure?"
nresx update strings.resx -k confirm.title -v "Are you sure?" -c "modal title"
nresx remove strings.resx -k confirm.title
nresx remove strings.resx --empty-value     # housekeeping
```

### 5. Validate

```sh
# Single file: empty keys, duplicates
nresx validate strings.resx

# Multi-file: also flags drift between language siblings
nresx validate strings.*.resx -r
```

Findings have two severities: *errors* (broken file: duplicated or empty keys)
and *warnings* (quality issues: empty values, missed or not translated
elements). The exit code is non-zero on errors; add `--warnings-as-errors` to
fail on warnings too, which makes `nresx validate` drop-in usable as a strict
CI gate. See the [exit code table](../src/nresx.CommandLine/README.md#exit-codes)
for the full set of codes (`3` not-found, `4` format error, `5` destination conflict, etc.) so scripts
can branch on specific failure modes.

## Library: load / mutate / save

```csharp
using nresx.Core;

var file = new ResourceFile("strings.resx");

// Iterate
foreach (var e in file.Elements)
    Console.WriteLine($"{e.Key} = {e.Value}  // {e.Comment}");

// Lookup by key
var greeting = file.Elements["greeting"];

// Mutate
file.Elements.Add("farewell", "Goodbye");
file.Elements.Remove("obsolete_key");   // throws if not found; use TryRemove for no-throw

// Save in another format
file.Save("strings.po", ResourceFormatType.Po);
```

### Streams

```csharp
using var stream = File.OpenRead("strings.resx");
var file = new ResourceFile(stream, ResourceFormatType.Resx);
```

Pass the format explicitly when the stream is a `MemoryStream` or doesn't
carry a filename.

### Async

```csharp
var file = await ResourceFile.LoadAsync("strings.resx", cancellationToken: ct);
await file.SaveAsync("strings.po", cancellationToken: ct);
```

### Validation

```csharp
var errors = file.Validate();
foreach (var error in errors)
    Console.WriteLine($"{error.ErrorType}: {error.ElementKey}");
// severity: error.ErrorType.GetSeverity(), extension in nresx.Core.Extensions
```

You can also choose how aggressively the constructor validates:

```csharp
var options = new ResourceFileOption { LoadMode = LoadMode.Lenient };
var file = new ResourceFile("strings.resx", options);
// errors are collected in file.ValidationErrors; no throw
```

`LoadMode.Strict` (default) throws on Error-severity findings; `Lenient`
collects them; `Raw` skips validation entirely.

## Where to next

- [Command reference](../src/nresx.CommandLine/README.md) - every CLI command with
  full flag documentation.
- [Format reference](formats/README.md) - what's preserved, what's lossy, per
  format.
- [Library API reference](api/README.md) - every public type, auto-generated
  from XML doc comments.
- [Recipes](recipes/README.md) - translator handoff, CI validation, drift
  detection, batch convert, programmatic bulk edits.
