# Find missing translations

**Goal.** You have `strings.en.resx`, `strings.de.resx`, `strings.fr.resx`,
and the German team added five new keys last week. You want a list of
everything missing from `de` (and probably `fr` too) so you can hand it off.

## The CLI shortcut

```sh
nresx validate strings.*.resx
```

This reports every key that's present in some file in the group but missing
in another, with `MissedElement` as the error type:

```
strings.de.resx: error: MissedElement: confirm.title
strings.de.resx: error: MissedElement: dialog.body
strings.fr.resx: error: MissedElement: confirm.title
strings.fr.resx: error: MissedElement: dialog.body
strings.fr.resx: error: MissedElement: settings.header
Found 5 issues (5 errors, 0 warnings)
```

That's already actionable - copy the keys into a translation request. For
ad-hoc one-off use this is enough.

## A scripted version

When you want one file with everything that needs translating, grab the
output programmatically. The library makes this clean.

```csharp
using nresx.Core;

var english = new ResourceFile("strings.en.resx");
var languages = new[] { "strings.de.resx", "strings.fr.resx", "strings.it.resx" };

foreach (var path in languages)
{
    var target = new ResourceFile(path);
    var targetKeys = target.Elements.Select(e => e.Key).ToHashSet();

    var missing = english.Elements
        .Where(e => !targetKeys.Contains(e.Key))
        .ToList();

    if (missing.Count == 0)
    {
        Console.WriteLine($"{path}: complete");
        continue;
    }

    Console.WriteLine($"{path}: {missing.Count} missing");

    // Optional: write a partial resx with just the missing keys for handoff
    var handoff = new ResourceFile();
    foreach (var e in missing)
        handoff.Elements.Add(e.Key, e.Value, e.Comment);
    handoff.Save(Path.ChangeExtension(path, ".missing.resx"));
}
```

This produces `strings.de.missing.resx`, `strings.fr.missing.resx`, etc. -
each containing only the entries that need translating. Send those to the
translator instead of the full file.

## Find untranslated entries

Different problem: the key is there, but the value is still the English
source.

```csharp
var english = new ResourceFile("strings.en.resx");
var englishByKey = english.Elements.ToDictionary(e => e.Key, e => e.Value);

foreach (var path in new[] { "strings.de.resx", "strings.fr.resx" })
{
    var target = new ResourceFile(path);
    var untranslated = target.Elements
        .Where(e => englishByKey.TryGetValue(e.Key, out var en) && en == e.Value)
        .ToList();

    Console.WriteLine($"{path}: {untranslated.Count} untranslated entries");
    foreach (var e in untranslated)
        Console.WriteLine($"  {e.Key}");
}
```

The CLI surfaces the same check as `NotTranslated` warnings - use
`--warnings-as-errors` if you want CI to block on them.

## Tips

- Both checks ignore file ordering, encoding, and whitespace - they compare
  keys and values directly.
- For the CLI path, base-language detection picks the neutral file (the one
  without a culture in the name) first, then English. Override with
  `--basic-lan de` if your project's source language isn't English.
- This pattern works equally for `.po`, `.json`, `.yaml`, etc.
