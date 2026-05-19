[`< Back`](./)

---

# ResourceFormatHelper

Namespace: nresx.Core.Helpers

```csharp
public class ResourceFormatHelper
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceFormatHelper](./nresx.core.helpers.resourceformathelper.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Constructors

### **ResourceFormatHelper()**

```csharp
public ResourceFormatHelper()
```

## Methods

### **DetectFormatByExtension(String, ResourceFormatType&)**

```csharp
public static bool DetectFormatByExtension(string path, ResourceFormatType& type)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`type` [ResourceFormatType&](./nresx.core.resourceformattype&.md)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **DetectExtension(ResourceFormatType, String&)**

```csharp
public static bool DetectExtension(ResourceFormatType type, String& extension)
```

#### Parameters

`type` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

`extension` [String&](https://docs.microsoft.com/en-us/dotnet/api/system.string&)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **GetFormatType(String)**

```csharp
public static ResourceFormatType GetFormatType(string path)
```

#### Parameters

`path` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[ResourceFormatType](./nresx.core.resourceformattype.md)<br>

### **GetExtension(ResourceFormatType)**

```csharp
public static string GetExtension(ResourceFormatType type)
```

#### Parameters

`type` [ResourceFormatType](./nresx.core.resourceformattype.md)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

---

[`< Back`](./)
