[`< Back`](./)

---

# ResourceElementErrorType

Namespace: nresx.Core.Extensions

Categorizes a single validation finding raised against a [ResourceElement](./nresx.core.resourceelement.md).

```csharp
public enum ResourceElementErrorType
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [ResourceElementErrorType](./nresx.core.extensions.resourceelementerrortype.md)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| None | 0 | Unspecified. |
| Duplicate | 1 | Two or more elements share the same key (file is broken). |
| PossibleDuplicate | 2 | Heuristic match: keys differ only in case or whitespace. |
| EmptyKey | 3 | Element has no key (file is broken). |
| EmptyValue | 4 | Element has a key but no value. |
| MissedElement | 5 | Element is present in the base file but missing from a translation file. |
| NotTranslated | 6 | Translation file's value equals the base file's value - the element wasn't translated. |

---

[`< Back`](./)
