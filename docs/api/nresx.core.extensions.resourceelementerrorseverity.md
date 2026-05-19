[`< Back`](./)

---

# ResourceElementErrorSeverity

Namespace: nresx.Core.Extensions

Severity bucket for a [ResourceElementErrorType](./nresx.core.extensions.resourceelementerrortype.md). Drives `nresx validate`'s exit code (RSX-229).

```csharp
public enum ResourceElementErrorSeverity
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [ResourceElementErrorSeverity](./nresx.core.extensions.resourceelementerrorseverity.md)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| Warning | 0 | Non-fatal: quality/heuristic finding. Validate exits 0 unless `--warnings-as-errors` is set. |
| Error | 1 | Fatal: file would fail at runtime. Validate exits non-zero. |

---

[`< Back`](./)
