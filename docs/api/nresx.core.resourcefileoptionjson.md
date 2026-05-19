[`< Back`](./)

---

# ResourceFileOptionJson

Namespace: nresx.Core

JSON-specific load/save options (RSX-120).

```csharp
public class ResourceFileOptionJson : ResourceFileOption
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceFileOption](./nresx.core.resourcefileoption.md) → [ResourceFileOptionJson](./nresx.core.resourcefileoptionjson.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Properties

### **Path**

Optional dotted path inside the JSON document where elements live (e.g. `messages.en`). Null means "auto-detect / document root".

```csharp
public string Path { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **KeyName**

Optional property name read/written for the element key. Null means "auto-detect from a set of common names".

```csharp
public string KeyName { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ValueName**

Optional property name read/written for the element value. Null means "auto-detect from a set of common names".

```csharp
public string ValueName { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **CommentName**

Optional property name read/written for the element comment. Null means "auto-detect from a set of common names".

```csharp
public string CommentName { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ElementType**

JSON shape this options instance is intended for (used to disambiguate ambiguous inputs).

```csharp
public JsonElementType ElementType { get; set; }
```

#### Property Value

[JsonElementType](./nresx.core.formatters.jsonelementtype.md)<br>

### **LoadMode**

How the file's loader should treat validation findings. Defaults to [LoadMode.Strict](./nresx.core.loadmode.md#strict).

```csharp
public LoadMode LoadMode { get; set; }
```

#### Property Value

[LoadMode](./nresx.core.loadmode.md)<br>

## Constructors

### **ResourceFileOptionJson()**

```csharp
public ResourceFileOptionJson()
```

---

[`< Back`](./)
