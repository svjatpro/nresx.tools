[`< Back`](./)

---

# ResourceFileExtensions

Namespace: nresx.Core.Extensions

```csharp
public static class ResourceFileExtensions
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceFileExtensions](./nresx.core.extensions.resourcefileextensions.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md), [ExtensionAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.extensionattribute)

## Methods

### **ConvertElements(ResourceFile, Action&lt;ResourceElement&gt;)**

```csharp
public static void ConvertElements(ResourceFile resourceFile, Action<ResourceElement> convertAction)
```

#### Parameters

`resourceFile` [ResourceFile](./nresx.core.resourcefile.md)<br>

`convertAction` [Action&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.action-1)<br>

### **ConvertElements(ResourceFile, Func&lt;ResourceElement, Boolean&gt;, Action&lt;ResourceElement&gt;)**

```csharp
public static void ConvertElements(ResourceFile resourceFile, Func<ResourceElement, bool> predicate, Action<ResourceElement> convertAction)
```

#### Parameters

`resourceFile` [ResourceFile](./nresx.core.resourcefile.md)<br>

`predicate` [Func&lt;ResourceElement, Boolean&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.func-2)<br>

`convertAction` [Action&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.action-1)<br>

### **AddPrefix(ResourceFile, String)**

```csharp
public static void AddPrefix(ResourceFile resourceFile, string prefix)
```

#### Parameters

`resourceFile` [ResourceFile](./nresx.core.resourcefile.md)<br>

`prefix` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RemovePrefix(ResourceFile, String)**

```csharp
public static void RemovePrefix(ResourceFile resourceFile, string prefix)
```

#### Parameters

`resourceFile` [ResourceFile](./nresx.core.resourcefile.md)<br>

`prefix` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **AddPostfix(ResourceFile, String)**

```csharp
public static void AddPostfix(ResourceFile resourceFile, string postfix)
```

#### Parameters

`resourceFile` [ResourceFile](./nresx.core.resourcefile.md)<br>

`postfix` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RemovePostfix(ResourceFile, String)**

```csharp
public static void RemovePostfix(ResourceFile resourceFile, string postfix)
```

#### Parameters

`resourceFile` [ResourceFile](./nresx.core.resourcefile.md)<br>

`postfix` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ValidateElements(IEnumerable&lt;ResourceElement&gt;, List`1&)**

```csharp
public static bool ValidateElements(IEnumerable<ResourceElement> elements, List`1& errors)
```

#### Parameters

`elements` [IEnumerable&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

`errors` [List`1&](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1&)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ValidateElements(ResourceFile, IEnumerable`1&)**

```csharp
public static bool ValidateElements(ResourceFile resourceFile, IEnumerable`1& errors)
```

#### Parameters

`resourceFile` [ResourceFile](./nresx.core.resourcefile.md)<br>

`errors` [IEnumerable`1&](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1&)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

---

[`< Back`](./)
