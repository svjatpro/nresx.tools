# WinForms - resx ↔ po roundtrip

**Scenario.** You have a WinForms app with localized `.resx` files
(`Form1.resx`, `Form1.de.resx`). The translator works in [POEdit](https://poedit.net/)
or a service like Crowdin / Lokalise / Phrase, all of which expect the
gettext `.po` format. You need to round-trip: export resx → po, hand to the
translator, take the translated po → satellite resx.

## What's in this directory

- [`Form1.resx`](Form1.resx) - the English source, modelled after a typical
  designer-generated WinForms resx (form title, label texts, button captions,
  one runtime-only message). Comments carry developer notes for translators.
- [`Form1.de.resx`](Form1.de.resx) - the German satellite, fully translated.

This is what `Form1.resx` looks like as a real WinForms resource: each
control property is a separate entry (`confirmButton.Text`, `customerLabel.Text`),
plus the form title (`$this.Text`) and any runtime-only strings
(`emptyCartMessage`).

## Step 1 - export to po

```sh
nresx convert Form1.resx -f po
# writes Form1.po next to Form1.resx
```

The result, ready for POEdit:

```po
msgid ""
msgstr ""

#  Form title shown in the title bar.
msgid "$this.Text"
msgstr "Order Details"

#  
msgid "customerLabel.Text"
msgstr "Customer:"

#  Primary action. Triggers checkout.
msgid "confirmButton.Text"
msgstr "Confirm order"

#  Shown when the order has no line items.
msgid "emptyCartMessage"
msgstr "Your cart is empty."
```

Two things to notice:

- The resx **comments become po comments** (the `#` lines). Anything you
  wrote as developer context for the translator survives the round-trip.
- The **keys stay as keys** (`$this.Text`, `confirmButton.Text`). They're
  not visible to end users; they're the join axis between English and
  German. The translator must not modify them.

## Step 2 - hand the po to the translator

Send `Form1.po`. They open it in POEdit (or whatever), translate every
`msgstr`, save as e.g. `Form1.de.po`.

A finished translation looks like:

```po
msgid "$this.Text"
msgstr "Bestelldetails"

msgid "confirmButton.Text"
msgstr "Bestellung bestätigen"
```

## Step 3 - import the translated po as a satellite resx

```sh
nresx convert Form1.de.po Form1.de.resx
```

The result is a valid WinForms satellite resource - same key shape as
`Form1.resx`, German values. Drop it next to `Form1.resx` and the WinForms
runtime picks it up automatically when the user's culture is `de-*`.

`Form1.de.resx` in this directory was produced by exactly this flow (then
hand-checked).

## Step 4 - validate the satellite before shipping

```sh
nresx validate Form1.resx Form1.de.resx
# Exit code 0 = no issues. Non-zero = something to fix.
```

`nresx validate` detects:

- Keys present in the base file but missing in the translation
  (translator skipped one).
- Keys present in the translation but not the base (typo in a key).
- Entries where the German value equals the English (translator forgot).

For the files in this directory, validate exits 0 - the German file is
complete and every value is distinct from the English. To see what a
failure looks like, delete one `<data>` entry from `Form1.de.resx` and
re-run.

## Notes

- **WinForms layout properties** (`okButton.Size`, `okButton.Location`,
  font / image / icon resources) are serialized objects, not localizable
  strings. `nresx` skips them on convert - only `*.Text` and plain string
  resources end up in the po. That's the correct behavior; you don't want
  translators editing pixel coordinates.
- The same flow works for `.resw` (UWP / WinUI) - everywhere you see
  `.resx` in this walkthrough, `.resw` works identically.
- For richer translation-management workflows (XLIFF, glossaries,
  translation memory), use [XLIFF](../../docs/formats/xliff.md):
  `nresx convert Form1.resx -f xliff`.
