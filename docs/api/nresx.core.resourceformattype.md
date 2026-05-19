[`< Back`](./)

---

# ResourceFormatType

Namespace: nresx.Core

Identifies a supported resource file format. Each value corresponds to a registered
 [FormatRegistry](./nresx.core.formatters.formatregistry.md) entry with a canonical file extension.

```csharp
public enum ResourceFormatType
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ValueType](https://docs.microsoft.com/en-us/dotnet/api/system.valuetype) → [Enum](https://docs.microsoft.com/en-us/dotnet/api/system.enum) → [ResourceFormatType](./nresx.core.resourceformattype.md)<br>
Implements [IComparable](https://docs.microsoft.com/en-us/dotnet/api/system.icomparable), [ISpanFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.ispanformattable), [IFormattable](https://docs.microsoft.com/en-us/dotnet/api/system.iformattable), [IConvertible](https://docs.microsoft.com/en-us/dotnet/api/system.iconvertible)

## Fields

| Name | Value | Description |
| --- | --: | --- |
| NA | 0 | Not applicable / not specified. Used for unsaved blank instances. |
| Resx | 1 | .NET `.resx` XML format. |
| Resw | 2 | UWP/WinRT `.resw` format (same shape as .resx). |
| Yml | 3 | YAML, `.yml` extension. |
| Yaml | 4 | YAML, `.yaml` extension. |
| Json | 5 | JSON, `.json`. |
| PlainText | 6 | Flat `key=value` text file, `.txt`. |
| Po | 7 | GNU gettext `.po`. |
| Xlf | 8 | XLIFF 1.2, `.xlf` extension. |
| Xliff | 9 | XLIFF 1.2, `.xliff` extension. |
| AndroidStrings | 10 | Android `strings.xml` (`<resources>` root, `<string name="...">` entries). |
| IosStrings | 11 | iOS `.strings` (`"key" = "value";` syntax). |
| JavaProperties | 12 | Java `.properties` (`key=value`, `\uXXXX` escapes). |
| Ini | 13 | INI `.ini` (`key=value`, optional `[sections]`). |
| Csv | 14 | Comma-separated values `.csv` (translator exchange). |
| Tsv | 15 | Tab-separated values `.tsv` (translator exchange). |
| Arb | 16 | Flutter Application Resource Bundle `.arb` (JSON with optional `@key` metadata). |
| Xlsx | 17 | Microsoft Excel `.xlsx` (translator-friendly exchange, key/value/comment columns). |

---

[`< Back`](./)
