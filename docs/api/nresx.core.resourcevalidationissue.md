[`< Back`](./)

---

# ResourceValidationIssue

Namespace: nresx.Core

A single validation finding scoped to the file it was raised against. Wraps the element-level
 [ResourceElementError](./nresx.core.extensions.resourceelementerror.md) (which carries the error type, element key, and message) with
 the path of the file, so cross-file validation results can be reported as data.

```csharp
public class ResourceValidationIssue
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceValidationIssue](./nresx.core.resourcevalidationissue.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Properties

### **FilePath**

Absolute path of the file the finding belongs to.

```csharp
public string FilePath { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Error**

The underlying element-level validation error (type, element key, message).

```csharp
public ResourceElementError Error { get; }
```

#### Property Value

[ResourceElementError](./nresx.core.extensions.resourceelementerror.md)<br>

## Constructors

### **ResourceValidationIssue(String, ResourceElementError)**

Creates a new file-scoped validation issue.

```csharp
public ResourceValidationIssue(string filePath, ResourceElementError error)
```

#### Parameters

`filePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`error` [ResourceElementError](./nresx.core.extensions.resourceelementerror.md)<br>

---

[`< Back`](./)
