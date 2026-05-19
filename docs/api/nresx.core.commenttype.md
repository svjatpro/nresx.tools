[`< Back`](./)

---

# CommentType

Namespace: nresx.Core

Classification of a comment line associated with a resource element or
 a whole resource file. Modeled after PO/gettext comment conventions; other
 formats default to [CommentType.Translator](./nresx.core.commenttype.md#translator).

```csharp
public enum CommentType
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [CommentType](./nresx.core.commenttype.md)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| None | 0 | Unspecified or unknown comment type. |
| Translator | 1 | Translator-facing comment (default for formats that have a single comment kind). |
| Extracted | 2 | Comment extracted from source code (PO `#.`). |
| Reference | 3 | Source reference, e.g. file:line (PO `#:`). |
| Flags | 4 | Format/processing flags (PO `#,`, e.g. `fuzzy`). |
| PreviousValue | 5 | Previous untranslated value before the most recent edit (PO `#|`). |

---

[`< Back`](./)
