[`< Back`](./)

---

# ResourceFileOption

Namespace: nresx.Core

Base type for format-specific options. Carries the shared [ResourceFileOption.LoadMode](./nresx.core.resourcefileoption.md#loadmode) knob;
 derived types add format-specific settings (see [ResourceFileOptionJson](./nresx.core.resourcefileoptionjson.md), [ResourceFileOptionPo](./nresx.core.resourcefileoptionpo.md), …).

```csharp
public class ResourceFileOption
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceFileOption](./nresx.core.resourcefileoption.md)

## Properties

### **LoadMode**

How the file's loader should treat validation findings. Defaults to [LoadMode.Strict](./nresx.core.loadmode.md#strict).

```csharp
public LoadMode LoadMode { get; set; }
```

#### Property Value

[LoadMode](./nresx.core.loadmode.md)<br>

## Constructors

### **ResourceFileOption()**

```csharp
public ResourceFileOption()
```

---

[`< Back`](./)
