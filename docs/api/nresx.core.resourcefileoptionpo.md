[`< Back`](./)

---

# ResourceFileOptionPo

Namespace: nresx.Core

PO-specific load/save options.

```csharp
public class ResourceFileOptionPo : ResourceFileOption
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceFileOption](./nresx.core.resourcefileoption.md) → [ResourceFileOptionPo](./nresx.core.resourcefileoptionpo.md)

## Properties

### **IgnoreEmptyHeaders**

If true (default), PO headers with empty values are dropped when loading.

```csharp
public bool IgnoreEmptyHeaders { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LoadMode**

How the file's loader should treat validation findings. Defaults to [LoadMode.Strict](./nresx.core.loadmode.md#strict).

```csharp
public LoadMode LoadMode { get; set; }
```

#### Property Value

[LoadMode](./nresx.core.loadmode.md)<br>

## Constructors

### **ResourceFileOptionPo()**

```csharp
public ResourceFileOptionPo()
```

---

[`< Back`](./)
