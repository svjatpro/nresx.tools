[`< Back`](./)

---

# ResourceFileHeaders

Namespace: nresx.Core

Well-known header keys used inside [ResourceFile.Headers](./nresx.core.resourcefile.md#headers).
 Add new entries here rather than scattering string literals across the codebase.

```csharp
public static class ResourceFileHeaders
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceFileHeaders](./nresx.core.resourcefileheaders.md)

## Fields

### **Language**

Standard `Language` header (BCP 47 code), used by PO/JSON/YAML files and by nresx to derive [ResourceFile.Culture](./nresx.core.resourcefile.md#culture).

```csharp
public static string Language;
```

---

[`< Back`](./)
