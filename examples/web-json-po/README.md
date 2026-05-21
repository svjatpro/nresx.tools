# Web - JSON ↔ po

**Scenario.** Your web app uses flat-key JSON locale files (i18next, vue-i18n,
react-intl, etc.). The translation team prefers a gettext workflow with
POEdit / Crowdin gettext / Lokalise on `.po`. You need to round-trip:
JSON → po → translator → po → JSON.

## What's in this directory

- [`locales/en.json`](locales/en.json) - English source. Flat keys with
  dot-notation namespaces (`app.title`, `nav.cart`, `auth.signIn`) - the
  shape most web i18n libraries consume directly.
- [`locales/de.json`](locales/de.json) - German translation in the same
  flat shape, ready to drop into i18next as `locales/de.json` and load with
  `lng: 'de'`.

## Step 1 - export to po

```sh
nresx convert locales/en.json -f po
# writes locales/en.po
```

The result is a clean po file - one msgid per JSON key, English values as
msgstr placeholders:

```po
msgid ""
msgstr ""

msgid "app.title"
msgstr "Acme Store"

msgid "nav.home"
msgstr "Home"

msgid "cart.checkout"
msgstr "Proceed to checkout"
```

## Step 2 - hand the po to the translator

Standard gettext workflow: they open `en.po` in POEdit, translate every
`msgstr`, save as `de.po`. Keys (`msgid`) stay untouched - they're the join
axis.

## Step 3 - import the translated po as a satellite JSON

```sh
nresx convert locales/de.po -f json
# writes locales/de.json
```

**Note on shape:** the CLI's default JSON output uses the object-per-key
shape (`{"key": {"value": "..."}}`), which carries comments and metadata.
i18next supports both shapes via its parser config, but most projects want
flat. Two options:

- **Flatten via a tiny post-step** (one-liner in `jq`):
  ```sh
  nresx convert locales/de.po -f json
  jq 'with_entries(.value = .value.value)' locales/de.json > locales/de.flat.json
  mv locales/de.flat.json locales/de.json
  ```
- **Use the library** if you want the flat shape directly:
  ```csharp
  var po   = new ResourceFile("locales/de.po");
  var opts = new ResourceFileOptionJson { ElementType = JsonElementType.KeyValue };
  po.Save("locales/de.json", ResourceFormatType.Json, opts);
  ```

The CLI exposes the simple roundtrip; the library exposes the shape switch.

`locales/de.json` in this directory is the flat-shape version, hand-curated
to match the source layout 1:1.

## Step 4 - validate the satellite

```sh
nresx validate locales/en.json locales/de.json
# Exit code 0 = OK. Non-zero = missing keys, drift, or untranslated.
```

`validate` detects:

- Keys missing in `de.json` (translator skipped them).
- Keys in `de.json` not present in `en.json` (typo).
- Entries where the German value matches the English (translator forgot).

For the files in this directory, validate exits 0.

## Why this works for web projects

- **Keys with dots are preserved as-is.** nresx doesn't try to be clever
  about `app.title` - it stays one key, one entry, in po and back. No
  accidental nesting on roundtrip.
- **The po is purely textual** - safe to commit, diff, and review in PRs.
  Any translator tool that handles gettext will accept it.
- **JSON shape is configurable** (CLI default is metadata-rich; library
  can output flat). Pick whichever matches your i18n library's config.

## Notes

- For nested-object JSON (`{"app": {"title": "..."}}` instead of
  `{"app.title": "..."}`), nresx currently only loads flat or
  object-per-key shapes. Flatten your nested JSON first (a 3-line
  pre-processing script) and roundtrip works as shown above.
- For YAML-based projects (vue-i18n in `.yaml`, Rails locale files), the
  same flow works - swap `en.json` for `en.yaml` and everything else stays
  identical. See the [YAML format reference](../../docs/formats/yaml.md).
