[`< Back`](./)

---

# ValidationException

Namespace: nresx.Core.Exceptions

Thrown by [ResourceFile](./nresx.core.resourcefile.md) constructors in [LoadMode.Strict](./nresx.core.loadmode.md#strict) when post-load validation finds
 one or more Error-severity issues (duplicate keys, empty keys, etc.). The full list of findings - including
 warnings that did not by themselves trigger the throw - is exposed via [ValidationException.Errors](./nresx.core.exceptions.validationexception.md#errors).

```csharp
public class ValidationException : NresxException, System.Runtime.Serialization.ISerializable
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception) → [NresxException](./nresx.core.exceptions.nresxexception.md) → [ValidationException](./nresx.core.exceptions.validationexception.md)<br>
Implements [ISerializable](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.iserializable)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Properties

### **Errors**

The full set of validation findings produced for the file (both errors and warnings).

```csharp
public IReadOnlyList<ResourceElementError> Errors { get; }
```

#### Property Value

[IReadOnlyList&lt;ResourceElementError&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

### **TargetSite**

```csharp
public MethodBase TargetSite { get; }
```

#### Property Value

[MethodBase](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodbase)<br>

### **Message**

```csharp
public string Message { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Data**

```csharp
public IDictionary Data { get; }
```

#### Property Value

[IDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.idictionary)<br>

### **InnerException**

```csharp
public Exception InnerException { get; }
```

#### Property Value

[Exception](https://docs.microsoft.com/en-us/dotnet/api/system.exception)<br>

### **HelpLink**

```csharp
public string HelpLink { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Source**

```csharp
public string Source { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **HResult**

```csharp
public int HResult { get; set; }
```

#### Property Value

[Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

### **StackTrace**

```csharp
public string StackTrace { get; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **ValidationException(IReadOnlyList&lt;ResourceElementError&gt;)**

Creates a new [ValidationException](./nresx.core.exceptions.validationexception.md) carrying the given findings.

```csharp
public ValidationException(IReadOnlyList<ResourceElementError> errors)
```

#### Parameters

`errors` [IReadOnlyList&lt;ResourceElementError&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

---

[`< Back`](./)
