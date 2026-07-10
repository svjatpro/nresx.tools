[`< Back`](./)

---

# ResourceManager

Namespace: nresx.Core

Static facade over the top-level, multi-file resource operations (RSX-235): converting a file
 between formats, loading and validating translation groups, and diffing two files. Single-file
 primitives live on [ResourceFile](./nresx.core.resourcefile.md); wildcard/glob expansion stays in the CLI - these
 methods take explicit paths.

```csharp
public class ResourceManager
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceManager](./nresx.core.resourcemanager.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Constructors

### **ResourceManager()**

```csharp
public ResourceManager()
```

## Methods

### **GetVersion()**

Returns the library version string (e.g. `v1.0.0`).

```csharp
public static string GetVersion()
```

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **LoadGroups(IEnumerable&lt;String&gt;, String)**

Groups an explicit list of resource file paths into translation groups (locale sets), detecting
 the base/source-language file of each. Wildcard expansion is a CLI concern - pass concrete paths.
 Convenience passthrough to [ResourceGroup.Detect(IEnumerable&lt;String&gt;, String)](./nresx.core.resourcegroup.md#detectienumerablestring-string).

```csharp
public static IReadOnlyList<ResourceGroup> LoadGroups(IEnumerable<string> paths, string baseLanguage)
```

#### Parameters

`paths` [IEnumerable&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>
Explicit file paths (each must exist).

`baseLanguage` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Optional language code (e.g. `en`) forcing the base-file pick; auto-detected when null.

#### Returns

[IReadOnlyList&lt;ResourceGroup&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

#### Exceptions

[FileNotFoundException](https://docs.microsoft.com/en-us/dotnet/api/system.io.filenotfoundexception)<br>
Thrown when any path does not exist.

### **Validate(IEnumerable&lt;String&gt;, String)**

Validates a set of resource files as translation groups and returns every finding as data.
 Files are grouped by [ResourceGroup.Detect(IEnumerable&lt;String&gt;, String)](./nresx.core.resourcegroup.md#detectienumerablestring-string), then each group is validated
 (per-file element checks plus group-level missed / not-translated checks - see
 [ResourceGroup.Validate()](./nresx.core.resourcegroup.md#validate)).

```csharp
public static IReadOnlyList<ResourceValidationIssue> Validate(IEnumerable<string> paths, string baseLanguage)
```

#### Parameters

`paths` [IEnumerable&lt;String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>
Explicit file paths (each must exist).

`baseLanguage` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Optional language code (e.g. `en`) forcing the base-file pick; auto-detected when null.

#### Returns

[IReadOnlyList&lt;ResourceValidationIssue&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

#### Exceptions

[FileNotFoundException](https://docs.microsoft.com/en-us/dotnet/api/system.io.filenotfoundexception)<br>
Thrown when any path does not exist.

### **Convert(String, String, Nullable&lt;ResourceFormatType&gt;)**

Loads `sourcePath` and writes it to `destinationPath` in a
 (possibly different) format. When `format` is null the target format is derived
 from the destination path's extension.

```csharp
public static void Convert(string sourcePath, string destinationPath, Nullable<ResourceFormatType> format)
```

#### Parameters

`sourcePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path of the file to read. Format is inferred from its extension.

`destinationPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to write. When `format` is null, its extension selects the target format.

`format` [Nullable&lt;ResourceFormatType&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1)<br>
Explicit target format; when null, derived from `destinationPath`'s extension.

#### Exceptions

[UnknownResourceFormatException](./nresx.core.exceptions.unknownresourceformatexception.md)<br>
Thrown when the target format is neither supplied nor derivable from the destination extension (or the source extension is unrecognized).

[ResourceFormatMismatchException](./nresx.core.exceptions.resourceformatmismatchexception.md)<br>
Thrown when an explicit `format` conflicts with the destination path's extension.

### **Diff(String, String)**

Computes the key-based difference between two resource files (which may be in different formats).
 Elements are read raw (duplicates preserved; first occurrence per key wins). See [ResourceDiff](./nresx.core.resourcediff.md).

```csharp
public static ResourceDiff Diff(string firstPath, string secondPath)
```

#### Parameters

`firstPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The "before" / left file.

`secondPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
The "after" / right file.

#### Returns

[ResourceDiff](./nresx.core.resourcediff.md)<br>

#### Exceptions

[UnknownResourceFormatException](./nresx.core.exceptions.unknownresourceformatexception.md)<br>
Thrown when either path's extension is not a recognized resource format.

---

[`< Back`](./)
