# JSON

Extensions: `.json`. Used by web frameworks, i18n libraries (i18next, vue-i18n,
react-intl), and miscellaneous in-house schemas.

JSON has no single "translation file" shape - nresx auto-detects two common
ones, and lets you override via [`ResourceFileOptionJson`](#options).

## Supported shapes

**Flat `key: value` map** - the most common shape.

```json
{
  "greeting": "Hello",
  "farewell": "Goodbye"
}
```

**Object-per-key with named properties** - typical of formats that carry per-entry metadata.

```json
{
  "greeting": { "value": "Hello", "comment": "shown on the home page" },
  "farewell": { "value": "Goodbye" }
}
```

**Array of entry objects** - `{ key, value, comment }` rows.

```json
{
  "strings": [
    { "key": "greeting", "value": "Hello",  "comment": "home page" },
    { "key": "farewell", "value": "Goodbye" }
  ]
}
```

For the object/array shapes, nresx recognises a default vocabulary
(case-insensitive):

- key: `key`, `id`, `name`
- value: `value`, `message`, `string`, `text`, `content`, `translation`
- comment: `comment`, `description`, `context`, `developer_comment`

## What's preserved

- Keys and string values.
- Comments, when the shape carries them.
- The element's original property names - when you load a file using
  `message`/`description` and save it back, the same property names are
  reused. Cross-format conversion uses the default names (`key`/`value`/`comment`).

## What's dropped

- Numbers, booleans, nulls, nested objects beyond the recognised entry shapes.
- Whitespace, key ordering of unrelated structural keys, trailing commas.

## Options

```csharp
var options = new ResourceFileOptionJson
{
    Path = "messages.en",          // nest entries under messages.en in output
    KeyName = "id",                // override property names
    ValueName = "translation",
    CommentName = "context",
    ElementType = JsonElementType.Object, // force array-of-objects shape on save
};
var file = new ResourceFile("strings.json", options);
```

`Path` accepts a dotted path (`messages.en`). On save, intermediate objects
are created as needed.

## CLI

```sh
nresx convert messages.json -f resx
nresx convert messages.resx messages.json
```

The CLI does not currently surface the property-name options - use the
library when you need them.
