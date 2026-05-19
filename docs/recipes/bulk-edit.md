# Bulk edits from code

**Goal.** Apply a programmatic transform to many resource files at once -
rename keys with a prefix, strip a deprecated naming convention, dedupe,
trim whitespace, anything you can express in C#. The CLI handles single
edits well (`nresx add`, `update`, `remove`); the library is where bulk
transforms live.

## The pattern

```csharp
using nresx.Core;

foreach (var path in Directory.EnumerateFiles("src", "*.resx", SearchOption.AllDirectories))
{
    var file = new ResourceFile(path);
    bool changed = MutateInPlace(file);
    if (changed) file.Save(path);    // overwrites the original
}

bool MutateInPlace(ResourceFile file)
{
    // your transform here - return true if anything actually changed
    return false;
}
```

The fixed scaffolding around your transform is small. The rest of this page
shows useful `MutateInPlace` bodies.

## Rename keys with a prefix

You renamed `confirm.title` to `dialog.confirm.title` everywhere in your
code; you need the resx files to match.

```csharp
bool MutateInPlace(ResourceFile file)
{
    var oldName = "confirm.title";
    var newName = "dialog.confirm.title";

    var element = file.Elements.FirstOrDefault(e => e.Key == oldName);
    if (element == null) return false;

    file.Elements.Add(newName, element.Value, element.Comment);
    file.Elements.Remove(oldName);
    return true;
}
```

For a wholesale prefix change (every key in this file gets `dialog.`
prepended), iterate over a snapshot - mutating the collection while
enumerating it is undefined:

```csharp
bool MutateInPlace(ResourceFile file)
{
    var snapshot = file.Elements.ToList();
    bool changed = false;

    foreach (var e in snapshot)
    {
        if (e.Key.StartsWith("dialog.")) continue;
        file.Elements.Remove(e.Key);
        file.Elements.Add("dialog." + e.Key, e.Value, e.Comment);
        changed = true;
    }
    return changed;
}
```

## Trim whitespace from values

Translators sometimes leave trailing spaces. Strip them:

```csharp
bool MutateInPlace(ResourceFile file)
{
    bool changed = false;
    foreach (var e in file.Elements.ToList())
    {
        var trimmed = e.Value?.Trim();
        if (trimmed == e.Value) continue;
        e.Value = trimmed;
        changed = true;
    }
    return changed;
}
```

## Remove empty entries

```csharp
bool MutateInPlace(ResourceFile file)
{
    var empty = file.Elements
        .Where(e => string.IsNullOrWhiteSpace(e.Value))
        .Select(e => e.Key)
        .ToList();

    foreach (var key in empty) file.Elements.Remove(key);
    return empty.Count > 0;
}
```

The CLI has `nresx remove --empty-value` for this exact case; the library
version is here as a template for variations (e.g. remove only entries whose
*comment* is empty).

## Apply the same transform across formats

The `ResourceFile` API is identical regardless of source format - the loop
above works for `.po`, `.json`, `.yaml`, etc. as long as you change the
extension filter. To convert during the transform, pass an explicit format
to `Save`:

```csharp
file.Save(Path.ChangeExtension(path, ".po"), ResourceFormatType.Po);
```

## Tips

- Always work on a snapshot (`file.Elements.ToList()`) when adding or
  removing during iteration.
- `file.Save(path)` writes back using the original format. To change format,
  pass an explicit `ResourceFormatType` as the second argument.
- For bulk transforms touching hundreds of files, async + parallelism is
  available (`ResourceFile.LoadAsync` / `SaveAsync` both accept
  `CancellationToken`).
- If your transform needs cross-file context (rename in *all* files
  consistently), load all files first into a list, mutate, then save -
  don't run two passes from disk.
