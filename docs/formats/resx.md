# .NET resx / resw

Extensions: `.resx` (.NET Framework, .NET Core, .NET 5+), `.resw` (UWP / WinRT).
Both use the same XML schema; nresx treats them as the same format with a
different default extension.

## File shape

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <!-- ... long Microsoft schema commentary + XSD ... -->
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, ...</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, ...</value></resheader>
  <data name="greeting" xml:space="preserve">
    <value>Hello</value>
    <comment>shown on the home page</comment>
  </data>
</root>
```

## What's preserved

- Key (`<data name="...">`).
- String value (`<value>`).
- Comment (`<comment>`).
- `xml:space="preserve"` is always written, so leading/trailing whitespace in
  values round-trips.

## What's dropped

- Non-string `<data>` entries (binary blobs, serialised .NET objects, fonts,
  colors, icons). nresx is text-only.
- `<assembly>` and `<metadata>` siblings of `<data>`.
- Pretty-print indentation chosen by the original writer (we emit Microsoft's
  default).

## Save options

```csharp
var options = new ResourceFileOptionResx
{
    WriteRootComment = false,      // skip the ~60-line schema commentary
    WriteEmbeddedSchema = false,   // skip the embedded XSD
    WriteStandardResHeaders = false, // skip the four standard <resheader> entries
};
file.Save("strings.resx", options);
```

The defaults match what Visual Studio produces. Turning everything off
produces a much smaller, diff-friendly file that `ResXResourceReader` still
reads correctly - useful in CI-managed resource sets.

## CLI

```sh
nresx convert strings.resx -f po
nresx convert strings.json strings.resx
```

## Notes

- The `.resw` extension is identical XML; .NET's runtime distinguishes them
  via project metadata, not file content.
- For .NET applications that consume resources via `ResXResourceReader` or
  the auto-generated `*.Designer.cs`, always save with the standard
  `<resheader>` block intact (the default).
