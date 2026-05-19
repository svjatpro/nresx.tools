[`< Back`](./)

---

# StringExtensions

Namespace: nresx.Core.Extensions

```csharp
public static class StringExtensions
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [StringExtensions](./nresx.core.extensions.stringextensions.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md), [ExtensionAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.extensionattribute)

## Methods

### **ToFirstCapital(String)**

```csharp
public static string ToFirstCapital(string source)
```

#### Parameters

`source` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ReplaceNewLine(String)**

```csharp
public static string ReplaceNewLine(string source)
```

#### Parameters

`source` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **SplitLines(String)**

```csharp
public static String[] SplitLines(string source)
```

#### Parameters

`source` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

#### Returns

[String[]](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ForEachLine(String, Action&lt;Int32, Int32&gt;)**

```csharp
public static int ForEachLine(string source, Action<int, int> lineAction)
```

#### Parameters

`source` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`lineAction` [Action&lt;Int32, Int32&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.action-2)<br>

#### Returns

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

---

[`< Back`](./)
