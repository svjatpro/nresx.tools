[`< Back`](./)

---

# ResourceElementErrorTypeExtensions

Namespace: nresx.Core.Extensions

Extension that maps each [ResourceElementErrorType](./nresx.core.extensions.resourceelementerrortype.md) to its [ResourceElementErrorSeverity](./nresx.core.extensions.resourceelementerrorseverity.md).

```csharp
public static class ResourceElementErrorTypeExtensions
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceElementErrorTypeExtensions](./nresx.core.extensions.resourceelementerrortypeextensions.md)<br>
Attributes [ExtensionAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.extensionattribute)

## Methods

### **GetSeverity(ResourceElementErrorType)**

Maps an error type to its severity per the RSX-229 policy:

- - `Error` = file is broken / will fail at runtime (duplicate keys, empty keys)
- - `Warning` = quality issue / heuristic / common in-progress state (everything else)

```csharp
public static ResourceElementErrorSeverity GetSeverity(ResourceElementErrorType errorType)
```

#### Parameters

`errorType` [ResourceElementErrorType](./nresx.core.extensions.resourceelementerrortype.md)<br>

#### Returns

[ResourceElementErrorSeverity](./nresx.core.extensions.resourceelementerrorseverity.md)<br>

---

[`< Back`](./)
