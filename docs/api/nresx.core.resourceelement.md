[`< Back`](./)

---

# ResourceElement

Namespace: nresx.Core

A single resource entry: key, value, optional context, optional plural variants, optional comments.
 Most formats use only [ResourceElement.Key](./nresx.core.resourceelement.md#key) and [ResourceElement.Value](./nresx.core.resourceelement.md#value); PO uses the full surface.

```csharp
public class ResourceElement
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [ResourceElement](./nresx.core.resourceelement.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Properties

### **Type**

Element kind. Currently [ResourceElementType.String](./nresx.core.resourceelementtype.md#string) for all formats.

```csharp
public ResourceElementType Type { get; set; }
```

#### Property Value

[ResourceElementType](./nresx.core.resourceelementtype.md)<br>

### **Key**

Element identifier (the resource name).

```csharp
public string Key { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Context**

Optional disambiguating context (e.g. PO `msgctxt`). Most formats ignore this.

```csharp
public string Context { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **KeyPlural**

Plural form of [ResourceElement.Key](./nresx.core.resourceelement.md#key) (e.g. PO `msgid_plural`). Null for formats that do not model plurals.

```csharp
public string KeyPlural { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Value**

The primary translated value (or the source string for the neutral file).

```csharp
public string Value { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **ValuePlurals**

Indexed plural translations (e.g. PO `msgstr[0]`, `msgstr[1]`, …). Empty for singular-only entries.
 Read-only view; use [ResourceElement.SetPlural(Int32, String)](./nresx.core.resourceelement.md#setpluralint32-string) / [ResourceElement.RemovePlural(Int32)](./nresx.core.resourceelement.md#removepluralint32) / [ResourceElement.ClearPlurals()](./nresx.core.resourceelement.md#clearplurals) to mutate.

```csharp
public IReadOnlyDictionary<int, string> ValuePlurals { get; }
```

#### Property Value

[IReadOnlyDictionary&lt;Int32, String&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlydictionary-2)<br>

### **Comment**

Convenience accessor for the translator comment (the most common case).
 Reads/writes the first [CommentType.Translator](./nresx.core.commenttype.md#translator) entry in [ResourceElement.Comments](./nresx.core.resourceelement.md#comments);
 setting to `null` is a no-op (use [ResourceElement.RemoveComment(Comment)](./nresx.core.resourceelement.md#removecommentcomment) to remove).

```csharp
public string Comment { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **Comments**

All comments attached to this element (translator, extracted, reference, flags, previous-value).
 Read-only view; use [ResourceElement.AddComment(Comment)](./nresx.core.resourceelement.md#addcommentcomment) / [ResourceElement.RemoveComment(Comment)](./nresx.core.resourceelement.md#removecommentcomment) / [ResourceElement.ClearComments()](./nresx.core.resourceelement.md#clearcomments) to mutate.

```csharp
public IReadOnlyList<Comment> Comments { get; }
```

#### Property Value

[IReadOnlyList&lt;Comment&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>

## Constructors

### **ResourceElement()**

```csharp
public ResourceElement()
```

## Methods

### **AddComment(Comment)**

Appends a comment to [ResourceElement.Comments](./nresx.core.resourceelement.md#comments).

```csharp
public void AddComment(Comment comment)
```

#### Parameters

`comment` [Comment](./nresx.core.comment.md)<br>

### **AddComment(CommentType, String)**

Convenience overload: creates and appends a [ResourceElement.Comment](./nresx.core.resourceelement.md#comment) of the given type and text.

```csharp
public void AddComment(CommentType type, string value)
```

#### Parameters

`type` [CommentType](./nresx.core.commenttype.md)<br>

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RemoveComment(Comment)**

Removes the given comment instance. Returns true if it was present.

```csharp
public bool RemoveComment(Comment comment)
```

#### Parameters

`comment` [Comment](./nresx.core.comment.md)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ClearComments()**

Removes all comments.

```csharp
public void ClearComments()
```

### **SetPlural(Int32, String)**

Sets the plural value at the given index (0, 1, 2, …). Replaces any existing entry at that index.

```csharp
public void SetPlural(int index, string value)
```

#### Parameters

`index` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **RemovePlural(Int32)**

Removes the plural at the given index. Returns true if it was present.

```csharp
public bool RemovePlural(int index)
```

#### Parameters

`index` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **ClearPlurals()**

Removes all plural values.

```csharp
public void ClearPlurals()
```

### **GetValue(Int32)**

Returns the singular [ResourceElement.Value](./nresx.core.resourceelement.md#value) when `plural` is `-1`,
 otherwise looks up the corresponding entry in [ResourceElement.ValuePlurals](./nresx.core.resourceelement.md#valueplurals);
 falls back to [ResourceElement.Value](./nresx.core.resourceelement.md#value) when no plural variant is registered.

```csharp
public string GetValue(int plural)
```

#### Parameters

`plural` [Int32](https://docs.microsoft.com/en-us/dotnet/api/system.int32)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

---

[`< Back`](./)
