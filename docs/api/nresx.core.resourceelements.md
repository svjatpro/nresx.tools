[`< Back`](./)

---

# ResourceElements

Namespace: nresx.Core

Ordered, mutable collection of [ResourceElement](./nresx.core.resourceelement.md). Indexable by position,
 by [ResourceElement.Key](./nresx.core.resourceelement.md#key), or by `(key, context)` for PO-style disambiguation.

```csharp
public sealed class ResourceElements : System.Collections.Generic.IEnumerable`1[[nresx.Core.ResourceElement, nresx.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Collections.IEnumerable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceElements](./nresx.core.resourceelements.md)<br>
Implements [IEnumerable&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1), [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/system.collections.ienumerable)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md), [DefaultMemberAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.defaultmemberattribute)

## Properties

### **Item**

```csharp
public ResourceElement Item { get; set; }
```

#### Property Value

[ResourceElement](./nresx.core.resourceelement.md)<br>

### **Item**

```csharp
public ResourceElement Item { get; set; }
```

#### Property Value

[ResourceElement](./nresx.core.resourceelement.md)<br>

### **Item**

```csharp
public ResourceElement Item { get; set; }
```

#### Property Value

[ResourceElement](./nresx.core.resourceelement.md)<br>

## Constructors

### **ResourceElements(IEnumerable&lt;ResourceElement&gt;)**

Ordered, mutable collection of [ResourceElement](./nresx.core.resourceelement.md). Indexable by position,
 by [ResourceElement.Key](./nresx.core.resourceelement.md#key), or by `(key, context)` for PO-style disambiguation.

```csharp
public ResourceElements(IEnumerable<ResourceElement> elements)
```

#### Parameters

`elements` [IEnumerable&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

## Methods

### **GetEnumerator()**

```csharp
public IEnumerator<ResourceElement> GetEnumerator()
```

#### Returns

[IEnumerator&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerator-1)<br>

### **Add(String, String, String, String, String, ValueTuple`2[])**

Appends a new string element. Duplicate keys are not checked here - pass through validation
 (see `Validate` extension) to detect duplicates after loading.

```csharp
public void Add(string key, string value, string comment, string context, string keyPlural, ValueTuple`2[] plurals)
```

#### Parameters

`key` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`comment` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`context` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`keyPlural` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`plurals` [ValueTuple`2[]](https://docs.microsoft.com/en-us/dotnet/api/system.valuetuple-2)<br>

### **Remove(String)**

Removes the element with the given key.

```csharp
public void Remove(string key)
```

#### Parameters

`key` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Exceptions

[KeyNotFoundException](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.keynotfoundexception)<br>
Thrown when no element with that key exists. Use [ResourceElements.TryRemove(String, ResourceElement&)](./nresx.core.resourceelements.md#tryremovestring-resourceelement&) for no-throw semantics.

### **TryRemove(String, ResourceElement&)**

Removes the element with the given key, returning `false` when not found.

```csharp
public bool TryRemove(string key, ResourceElement& element)
```

#### Parameters

`key` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`element` [ResourceElement&](./nresx.core.resourceelement&.md)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

---

[`< Back`](./)
