[`< Back`](./)

---

# ResourceGroup

Namespace: nresx.Core

A set of related locale files that belong together as one translation unit
 (e.g. `strings.resx`, `strings_de.resx`, `strings_fr.resx`), together with the
 base (source-language) file when one can be detected. Use [ResourceGroup.Detect(IEnumerable&lt;String&gt;, String)](./nresx.core.resourcegroup.md#detectienumerablestring-string) to group an
 explicit list of file paths the same way the CLI does.

```csharp
public class ResourceGroup
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceGroup](./nresx.core.resourcegroup.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Properties

### **Files**

The files that make up this group, in detection order.

```csharp
public IReadOnlyList<ResourceFile> Files { get; }
```

#### Property Value

[IReadOnlyList&lt;ResourceFile&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **BaseFile**

The base (source-language) file of the group, or `null` when no base can be
 determined (a single-file group, or a group in which no file carries a detectable culture).
 This is always one of the entries in [ResourceGroup.Files](./nresx.core.resourcegroup.md#files) (by reference) when non-null.

```csharp
public ResourceFile BaseFile { get; }
```

#### Property Value

[ResourceFile](./nresx.core.resourcefile.md)<br>

## Methods

### **Detect(IEnumerable&lt;String&gt;, String)**

Groups an explicit list of resource file paths into translation groups. Mirrors the CLI's
 grouping heuristic: (1) files sharing a folder, format, and each carrying a culture extracted
 from their file name are grouped together; (2) remaining files are grouped by culture-specific
 sibling folders (grouped by grandparent directory); (3) anything left over becomes a
 single-file group. Every file is loaded (in [LoadMode.Lenient](./nresx.core.loadmode.md#lenient) mode, so content
 validation findings never throw).

```csharp
public static IReadOnlyList<ResourceGroup> Detect(IEnumerable<string> paths, string baseLanguage)
```

#### Parameters

`paths` [IEnumerable&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>
Explicit file paths. No wildcard expansion is performed - each path must exist.

`baseLanguage` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Optional language code (e.g. `en` or `en-US`) that forces the base-file pick. When null,
 the base file is auto-detected: neutral (no culture) &gt; English &gt; first alphabetical by culture.

#### Returns

[IReadOnlyList&lt;ResourceGroup&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

#### Exceptions

[ArgumentNullException](https://docs.microsoft.com/en-us/dotnet/api/system.argumentnullexception)<br>
Thrown when `paths` is null.

[FileNotFoundException](https://docs.microsoft.com/en-us/dotnet/api/system.io.filenotfoundexception)<br>
Thrown when any path does not exist.

[UnknownResourceFormatException](./nresx.core.exceptions.unknownresourceformatexception.md)<br>
Thrown when a path's extension is not a recognized resource format.

### **Validate()**

Validates the group and returns every finding as data (instead of printing). Combines per-file
 element checks (duplicate/empty key, empty value - see [ResourceFileExtensions.ValidateElements(IEnumerable&lt;ResourceElement&gt;, List`1&)](./nresx.core.extensions.resourcefileextensions.md#validateelementsienumerableresourceelement-list`1&))
 with the group-level checks: [ResourceElementErrorType.MissedElement](./nresx.core.extensions.resourceelementerrortype.md#missedelement) (a key present in
 another file of the group but absent here) and [ResourceElementErrorType.NotTranslated](./nresx.core.extensions.resourceelementerrortype.md#nottranslated)
 (a non-base file whose value equals the [ResourceGroup.BaseFile](./nresx.core.resourcegroup.md#basefile)'s value for the same key). Elements are
 read raw (duplicates preserved) so duplicate-key findings survive.

```csharp
public IReadOnlyList<ResourceValidationIssue> Validate()
```

#### Returns

[IReadOnlyList&lt;ResourceValidationIssue&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

---

[`< Back`](./)
