[`< Back`](./)

---

# Comment

Namespace: nresx.Core

A single comment line attached to a [ResourceFile](./nresx.core.resourcefile.md) or [ResourceElement](./nresx.core.resourceelement.md).

```csharp
public class Comment
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Comment](./nresx.core.comment.md)<br>
Attributes [NullableContextAttribute](./system.runtime.compilerservices.nullablecontextattribute.md), [NullableAttribute](./system.runtime.compilerservices.nullableattribute.md)

## Properties

### **Type**

The kind of comment (see [CommentType](./nresx.core.commenttype.md)).

```csharp
public CommentType Type { get; set; }
```

#### Property Value

[CommentType](./nresx.core.commenttype.md)<br>

### **Value**

The comment text, without the leading marker.

```csharp
public string Value { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

## Constructors

### **Comment(CommentType, String)**

Creates a new comment of the given type and text.

```csharp
public Comment(CommentType type, string value)
```

#### Parameters

`type` [CommentType](./nresx.core.commenttype.md)<br>

`value` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

---

[`< Back`](./)
