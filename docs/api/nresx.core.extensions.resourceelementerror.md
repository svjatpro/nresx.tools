[`< Back`](./)

---

# ResourceElementError

Namespace: nresx.Core.Extensions

A single validation finding produced by `Validate` extension methods.

```csharp
public class ResourceElementError
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceElementError](./nresx.core.extensions.resourceelementerror.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Properties

### **ErrorType**

The kind of validation issue.

```csharp
public ResourceElementErrorType ErrorType { get; }
```

#### Property Value

[ResourceElementErrorType](./nresx.core.extensions.resourceelementerrortype.md)<br>

### **ElementKey**

Key of the element the error is attached to (may be empty for file-level findings).

```csharp
public string ElementKey { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Message**

Human-readable detail. May be empty/null when [ResourceElementError.ErrorType](./nresx.core.extensions.resourceelementerror.md#errortype) alone is enough.

```csharp
public string Message { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **ResourceElementError(ResourceElementErrorType, String, String)**

Creates a new validation finding.

```csharp
public ResourceElementError(ResourceElementErrorType errorType, string elementKey, string message)
```

#### Parameters

`errorType` [ResourceElementErrorType](./nresx.core.extensions.resourceelementerrortype.md)<br>

`elementKey` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`message` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

---

[`< Back`](./)
