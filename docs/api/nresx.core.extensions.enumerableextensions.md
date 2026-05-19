[`< Back`](./)

---

# EnumerableExtensions

Namespace: nresx.Core.Extensions

```csharp
public static class EnumerableExtensions
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [EnumerableExtensions](./nresx.core.extensions.enumerableextensions.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md), [ExtensionAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.extensionattribute)

## Methods

### **TryTake&lt;T&gt;(List&lt;T&gt;, T&)**

```csharp
public static bool TryTake<T>(List<T> source, T& item)
```

#### Type Parameters

`T`<br>

#### Parameters

`source` List&lt;T&gt;<br>

`item` T&<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **TryTakeRange&lt;T&gt;(List&lt;T&gt;, List`1&, Int32)**

```csharp
public static bool TryTakeRange<T>(List<T> source, List`1& items, int maxCount)
```

#### Type Parameters

`T`<br>

#### Parameters

`source` List&lt;T&gt;<br>

`items` List`1&<br>

`maxCount` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Take&lt;T&gt;(List&lt;T&gt;)**

```csharp
public static T Take<T>(List<T> source)
```

#### Type Parameters

`T`<br>

#### Parameters

`source` List&lt;T&gt;<br>

#### Returns

T<br>

---

[`< Back`](./)
