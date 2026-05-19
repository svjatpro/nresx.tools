[`< Back`](./)

---

# LoadMode

Namespace: nresx.Core

Controls how a [ResourceFile](./nresx.core.resourcefile.md) reacts to per-element parse anomalies and post-load validation findings.
 Structural parse errors (malformed file) always throw regardless of mode.

```csharp
public enum LoadMode
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [LoadMode](./nresx.core.loadmode.md)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Strict | 0 | Default. Validation runs after load; if any Error-severity findings are present, throws [ValidationException](./nresx.core.exceptions.validationexception.md). |
| Lenient | 1 | Validation runs after load; findings are collected on [ResourceFile.ValidationErrors](./nresx.core.resourcefile.md#validationerrors) without throwing. |
| Raw | 2 | Skip validation entirely. [ResourceFile.ValidationErrors](./nresx.core.resourcefile.md#validationerrors) stays empty. Closest to a bulk dump. |

---

[`< Back`](./)
