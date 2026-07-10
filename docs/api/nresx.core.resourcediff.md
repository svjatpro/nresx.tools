[`< Back`](./)

---

# ResourceDiff

Namespace: nresx.Core

The key-based difference between two resource files (or two element sets): which keys were added,
 removed, or changed. Elements present in both sides with equal values are omitted. Comparison is by
 [ResourceElement.Key](./nresx.core.resourceelement.md#key); when a side has duplicate keys, the first occurrence wins.

```csharp
public class ResourceDiff
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceDiff](./nresx.core.resourcediff.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Properties

### **AddedElements**

Elements whose key exists in the second side but not the first.

```csharp
public IReadOnlyList<ResourceElement> AddedElements { get; }
```

#### Property Value

[IReadOnlyList&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **RemovedElements**

Elements whose key exists in the first side but not the second.

```csharp
public IReadOnlyList<ResourceElement> RemovedElements { get; }
```

#### Property Value

[IReadOnlyList&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **ChangedElements**

Elements whose key exists on both sides but whose [ResourceElement.Value](./nresx.core.resourceelement.md#value) differs, paired (first, second).

```csharp
public IReadOnlyList<ValueTuple<ResourceElement, ResourceElement>> ChangedElements { get; }
```

#### Property Value

[IReadOnlyList&lt;ValueTuple&lt;ResourceElement, ResourceElement&gt;&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **AreSame**

True when there are no added, removed, or changed elements - the two sides are equivalent by key and value.

```csharp
public bool AreSame { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

## Methods

### **Compare(IEnumerable&lt;ResourceElement&gt;, IEnumerable&lt;ResourceElement&gt;)**

Compares two element sets by key. Added = in `second` only; Removed = in
 `first` only; Changed = same key, different value. Order follows the source
 documents (added in second's order, removed/changed in first's order).

```csharp
public static ResourceDiff Compare(IEnumerable<ResourceElement> first, IEnumerable<ResourceElement> second)
```

#### Parameters

`first` [IEnumerable&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

`second` [IEnumerable&lt;ResourceElement&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1)<br>

#### Returns

[ResourceDiff](./nresx.core.resourcediff.md)<br>

---

[`< Back`](./)
