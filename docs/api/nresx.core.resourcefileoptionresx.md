[`< Back`](./)

---

# ResourceFileOptionResx

Namespace: nresx.Core

ResX-specific save options. All default to `true` for parity with the legacy writer (RSX-117).

```csharp
public class ResourceFileOptionResx : ResourceFileOption
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceFileOption](./nresx.core.resourcefileoption.md) → [ResourceFileOptionResx](./nresx.core.resourcefileoptionresx.md)

## Properties

### **WriteRootComment**

If true (default), writes the multi-line Microsoft commentary at the top of the resx file. Turn off for diff-friendly / compact output.

```csharp
public bool WriteRootComment { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **WriteEmbeddedSchema**

If true (default), writes the embedded XSD schema (~50 lines). Turn off for compact output; .NET `ResXResourceReader` still reads the file fine without it.

```csharp
public bool WriteEmbeddedSchema { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **WriteStandardResHeaders**

If true (default), writes the four standard `<resheader>` entries (resmimetype, version, reader, writer). Required for .NET `ResXResourceReader` compatibility - turn off only if writing a "bare" resx for a custom reader.

```csharp
public bool WriteStandardResHeaders { get; set; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **LoadMode**

How the file's loader should treat validation findings. Defaults to [LoadMode.Strict](./nresx.core.loadmode.md#strict).

```csharp
public LoadMode LoadMode { get; set; }
```

#### Property Value

[LoadMode](./nresx.core.loadmode.md)<br>

## Constructors

### **ResourceFileOptionResx()**

```csharp
public ResourceFileOptionResx()
```

---

[`< Back`](./)
