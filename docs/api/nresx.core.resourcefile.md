[`< Back`](./)

---

# ResourceFile

Namespace: nresx.Core

In-memory representation of a localized resource file (resx, po, json, yaml, xliff, …).
 Load via constructors, manipulate [ResourceFile.Elements](./nresx.core.resourcefile.md#elements), then call [ResourceFile.Save(String, Boolean, ResourceFileOption)](./nresx.core.resourcefile.md#savestring-boolean-resourcefileoption)
 (or [ResourceFile.SaveAsync(String, Boolean, ResourceFileOption, CancellationToken)](./nresx.core.resourcefile.md#saveasyncstring-boolean-resourcefileoption-cancellationtoken)) to persist.

```csharp
public class ResourceFile
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceFile](./nresx.core.resourcefile.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Properties

### **FileFormat**

Format of the underlying resource file (or [ResourceFormatType.NA](./nresx.core.resourceformattype.md#na) for unsaved blank instances).

```csharp
public ResourceFormatType FileFormat { get; }
```

#### Property Value

[ResourceFormatType](./nresx.core.resourceformattype.md)<br>

### **Culture**

Culture this file represents. Derived from the file name (e.g. `strings.de.resx` → `de`)
 or from the `Language` header when the file has one; defaults to [CultureInfo.InvariantCulture](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.invariantculture).

```csharp
public CultureInfo Culture { get; set; }
```

#### Property Value

[CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)<br>

### **Headers**

Free-form headers (key→value). Format-specific; e.g. PO files use this for `Content-Type`, `Plural-Forms`, etc.
 Read-only view; use [ResourceFile.SetHeader(String, String)](./nresx.core.resourcefile.md#setheaderstring-string) / [ResourceFile.RemoveHeader(String)](./nresx.core.resourcefile.md#removeheaderstring) / [ResourceFile.ClearHeaders()](./nresx.core.resourcefile.md#clearheaders) to mutate.
 Setting the `Language` header also syncs [ResourceFile.Culture](./nresx.core.resourcefile.md#culture).

```csharp
public IReadOnlyDictionary<string, string> Headers { get; }
```

#### Property Value

[IReadOnlyDictionary&lt;String, String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2)<br>

### **IsNewFile**

True when this instance was constructed in-memory (not loaded from disk).

```csharp
public bool IsNewFile { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **FileName**

Bare file name of the source file (when loaded from disk); empty otherwise.

```csharp
public string FileName { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **AbsolutePath**

Absolute path of the source file (when loaded from disk); empty otherwise.

```csharp
public string AbsolutePath { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Elements**

The resource entries themselves. Use indexers and `Add`/`Remove` methods to mutate.

```csharp
public ResourceElements Elements { get; private set; }
```

#### Property Value

[ResourceElements](./nresx.core.resourceelements.md)<br>

### **Comments**

File-level comments (those not attached to any specific element).
 Read-only view; use [ResourceFile.AddComment(Comment)](./nresx.core.resourcefile.md#addcommentcomment) / [ResourceFile.RemoveComment(Comment)](./nresx.core.resourcefile.md#removecommentcomment) / [ResourceFile.ClearComments()](./nresx.core.resourcefile.md#clearcomments) to mutate.

```csharp
public IReadOnlyList<Comment> Comments { get; }
```

#### Property Value

[IReadOnlyList&lt;Comment&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **ValidationErrors**

Validation findings produced during load (in [LoadMode.Strict](./nresx.core.loadmode.md#strict) or [LoadMode.Lenient](./nresx.core.loadmode.md#lenient) mode)
 or by the most recent call to [ResourceFile.Validate()](./nresx.core.resourcefile.md#validate). Always empty in [LoadMode.Raw](./nresx.core.loadmode.md#raw).

```csharp
public IReadOnlyList<ResourceElementError> ValidationErrors { get; }
```

#### Property Value

[IReadOnlyList&lt;ResourceElementError&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **ElementHasKey**

```csharp
public bool ElementHasKey { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ElementHasComment**

```csharp
public bool ElementHasComment { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Constructors

### **ResourceFile(String, ResourceFileOption)**

Loads a resource file from disk. The format is inferred from the file extension.
 If the file does not exist, the instance is created in [ResourceFile.IsNewFile](./nresx.core.resourcefile.md#isnewfile) mode
 (no error) so callers can populate elements and call [ResourceFile.Save(String, Boolean, ResourceFileOption)](./nresx.core.resourcefile.md#savestring-boolean-resourcefileoption).

```csharp
public ResourceFile(string path, ResourceFileOption options)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

#### Exceptions

[UnknownResourceFormatException](./nresx.core.exceptions.unknownresourceformatexception.md)<br>
Thrown when the file extension is not recognized.

### **ResourceFile(Stream, ResourceFormatType, ResourceFileOption)**

Loads a resource file from a stream. Specify `resourceFormat` when
 the stream's source format can't be inferred (e.g. `MemoryStream`).

```csharp
public ResourceFile(Stream stream, ResourceFormatType resourceFormat, ResourceFileOption options)
```

#### Parameters

`stream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

`resourceFormat` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

#### Exceptions

[UnknownResourceFormatException](./nresx.core.exceptions.unknownresourceformatexception.md)<br>
Thrown when the format cannot be determined.

### **ResourceFile(ResourceFileOption)**

Creates a new, empty resource file with no specific format set.

```csharp
public ResourceFile(ResourceFileOption options)
```

#### Parameters

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

### **ResourceFile(ResourceFormatType, ResourceFileOption)**

Creates a new, empty resource file of the given format.

```csharp
public ResourceFile(ResourceFormatType fileFormat, ResourceFileOption options)
```

#### Parameters

`fileFormat` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

## Methods

### **SetHeader(String, String)**

Sets or replaces a header value. When `name` is `Language`, also updates [ResourceFile.Culture](./nresx.core.resourcefile.md#culture).

```csharp
public void SetHeader(string name, string value)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RemoveHeader(String)**

Removes a header. Returns true if it was present.

```csharp
public bool RemoveHeader(string name)
```

#### Parameters

`name` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ClearHeaders()**

Removes all headers.

```csharp
public void ClearHeaders()
```

### **AddComment(Comment)**

Appends a file-level comment.

```csharp
public void AddComment(Comment comment)
```

#### Parameters

`comment` [Comment](./nresx.core.comment.md)<br>

### **AddComment(CommentType, String)**

Convenience overload: creates and appends a [Comment](./nresx.core.comment.md) of the given type and text.

```csharp
public void AddComment(CommentType type, string value)
```

#### Parameters

`type` [CommentType](./nresx.core.commenttype.md)<br>

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RemoveComment(Comment)**

Removes the given file-level comment instance. Returns true if it was present.

```csharp
public bool RemoveComment(Comment comment)
```

#### Parameters

`comment` [Comment](./nresx.core.comment.md)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ClearComments()**

Removes all file-level comments.

```csharp
public void ClearComments()
```

### **Validate()**

Re-runs validation against the current state and refreshes [ResourceFile.ValidationErrors](./nresx.core.resourcefile.md#validationerrors). Does not throw.

```csharp
public IReadOnlyList<ResourceElementError> Validate()
```

#### Returns

[IReadOnlyList&lt;ResourceElementError&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **LoadRawElements(String)**

Loads raw element rows from a file without deduping by key - duplicates are preserved
 in document order. Use this when you need to inspect the file as authored, including
 duplicate or malformed entries.

```csharp
public static IEnumerable<ResourceElement> LoadRawElements(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[IEnumerable&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

#### Exceptions

[UnknownResourceFormatException](./nresx.core.exceptions.unknownresourceformatexception.md)<br>
Thrown when the file extension is not recognized.

### **LoadRawElements(Stream, ResourceFormatType)**

Stream variant of [ResourceFile.LoadRawElements(String)](./nresx.core.resourcefile.md#loadrawelementsstring). Format is taken from
 `resourceFormat` when supplied; otherwise inferred from the
 stream's filename if it has one.

```csharp
public static IEnumerable<ResourceElement> LoadRawElements(Stream stream, ResourceFormatType resourceFormat)
```

#### Parameters

`stream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

`resourceFormat` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

#### Returns

[IEnumerable&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

#### Exceptions

[UnknownResourceFormatException](./nresx.core.exceptions.unknownresourceformatexception.md)<br>
Thrown when the format cannot be determined.

### **Save(String, Boolean, ResourceFileOption)**

Saves to a file path using the format the file was loaded as
 (or constructed with). The extension is forced to match the format.

```csharp
public void Save(string path, bool createDir, ResourceFileOption options)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`createDir` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

### **Save(String, ResourceFormatType, Boolean, ResourceFileOption)**

Saves to `path` as `type`.

```csharp
public void Save(string path, ResourceFormatType type, bool createDir, ResourceFileOption options)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Destination path. The extension is rewritten to match `type` unless it already does.

`type` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>
Format to write.

`createDir` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
If true, missing parent directories are created.

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

#### Exceptions

[UnknownResourceFormatException](./nresx.core.exceptions.unknownresourceformatexception.md)<br>
Thrown when `type` is not registered.

[ResourceFormatMismatchException](./nresx.core.exceptions.resourceformatmismatchexception.md)<br>
Thrown when `path`'s extension belongs to a different known format than `type`.

### **Save(Stream, ResourceFileOption)**

```csharp
public void Save(Stream stream, ResourceFileOption options)
```

#### Parameters

`stream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

### **Save(Stream, ResourceFormatType, ResourceFileOption)**

```csharp
public void Save(Stream stream, ResourceFormatType type, ResourceFileOption options)
```

#### Parameters

`stream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

`type` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

### **SaveToStream()**

```csharp
public Stream SaveToStream()
```

#### Returns

[Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

### **LoadAsync(String, ResourceFileOption, CancellationToken)**

```csharp
public static Task<ResourceFile> LoadAsync(string path, ResourceFileOption options, CancellationToken cancellationToken)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

`cancellationToken` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task&lt;ResourceFile&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1)<br>

### **LoadAsync(Stream, ResourceFormatType, ResourceFileOption, CancellationToken)**

```csharp
public static Task<ResourceFile> LoadAsync(Stream stream, ResourceFormatType resourceFormat, ResourceFileOption options, CancellationToken cancellationToken)
```

#### Parameters

`stream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

`resourceFormat` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

`cancellationToken` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task&lt;ResourceFile&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1)<br>

### **LoadRawElementsAsync(String, CancellationToken)**

```csharp
public static Task<IEnumerable<ResourceElement>> LoadRawElementsAsync(string path, CancellationToken cancellationToken)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`cancellationToken` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task&lt;IEnumerable&lt;ResourceElement&gt;&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1)<br>

### **LoadRawElementsAsync(Stream, ResourceFormatType, CancellationToken)**

```csharp
public static Task<IEnumerable<ResourceElement>> LoadRawElementsAsync(Stream stream, ResourceFormatType resourceFormat, CancellationToken cancellationToken)
```

#### Parameters

`stream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

`resourceFormat` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

`cancellationToken` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task&lt;IEnumerable&lt;ResourceElement&gt;&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1)<br>

### **SaveAsync(String, Boolean, ResourceFileOption, CancellationToken)**

```csharp
public Task SaveAsync(string path, bool createDir, ResourceFileOption options, CancellationToken cancellationToken)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`createDir` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

`cancellationToken` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)<br>

### **SaveAsync(String, ResourceFormatType, Boolean, ResourceFileOption, CancellationToken)**

```csharp
public Task SaveAsync(string path, ResourceFormatType type, bool createDir, ResourceFileOption options, CancellationToken cancellationToken)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`type` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

`createDir` [Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

`cancellationToken` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)<br>

### **SaveAsync(Stream, ResourceFileOption, CancellationToken)**

```csharp
public Task SaveAsync(Stream stream, ResourceFileOption options, CancellationToken cancellationToken)
```

#### Parameters

`stream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

`cancellationToken` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)<br>

### **SaveAsync(Stream, ResourceFormatType, ResourceFileOption, CancellationToken)**

```csharp
public Task SaveAsync(Stream stream, ResourceFormatType type, ResourceFileOption options, CancellationToken cancellationToken)
```

#### Parameters

`stream` [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)<br>

`type` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

`options` [ResourceFileOption](./nresx.core.resourcefileoption.md)<br>

`cancellationToken` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task)<br>

### **SaveToStreamAsync(CancellationToken)**

```csharp
public Task<Stream> SaveToStreamAsync(CancellationToken cancellationToken)
```

#### Parameters

`cancellationToken` [CancellationToken](https://docs.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)<br>

#### Returns

[Task&lt;Stream&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1)<br>

---

[`< Back`](./)
