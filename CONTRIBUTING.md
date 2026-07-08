# Contributing to nresx

Thanks for considering a contribution. Bug reports, docs fixes, new format
support, and CLI improvements are all welcome.

## Where things go

- **Bugs** - [open an issue](https://github.com/svjatpro/nresx.tools/issues/new/choose) with the bug template.
- **Questions / ideas** - [Discussions](https://github.com/svjatpro/nresx.tools/discussions).
- **Code** - pull requests target the `develop` branch (`main` is the release branch).
- **First contribution?** Look for [`good first issue`](https://github.com/svjatpro/nresx.tools/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22) labels.

## Dev setup

You need the .NET 9 SDK. Everything works on Windows, Linux, and macOS:

```sh
git clone https://github.com/svjatpro/nresx.tools.git
cd nresx.tools
dotnet build src/nresx.tools.sln
dotnet test src/nresx.tools.sln
```

The CLI binary lands at `src/nresx.CommandLine/bin/Debug/net9.0/nresx` (`nresx.exe`
on Windows).

## Repo layout

| Path                     | What it is                                          |
| ------------------------ | --------------------------------------------------- |
| `src/nresx.Core/`        | The library (`netstandard2.0`): formats, model, validation |
| `src/nresx.CommandLine/` | The CLI (`net9.0`): commands, argument handling     |
| `src/nresx.Core.Tests/`  | Library tests (NUnit + FluentAssertions)            |
| `src/nresx.CommandLine.Tests/` | CLI integration tests (run the built `nresx` binary) |
| `.test_files/`           | Resource-file fixtures used by both test projects   |
| `docs/`                  | Getting started, format reference, generated API docs, recipes |
| `scripts/`               | Build / release / docs-generation scripts           |

## Guidelines

- **Match the existing code style.** The codebase uses spaces inside parentheses
  (`Method( arg1, arg2 )`) and K&R-ish braces; just make new code look like its
  neighbors.
- **Every behavior change needs a test.** New CLI options get integration tests
  in `src/nresx.CommandLine.Tests`; library changes get tests in `src/nresx.Core.Tests`.
  Run `dotnet test` before pushing - the full suite must pass on your platform
  (CI runs it on all three OSes).
- **Error messages and exit codes live in two places.** If you touch the error
  templates or exit-code constants in `src/nresx.CommandLine/Commands/Base/BaseCommand.cs`,
  update the mirrored copies in `src/nresx.Core.Tests/TestBase/TestBase.cs` in the
  same commit - they are intentional duplicates so the test project doesn't
  reference the CLI project.
- **Docs follow the change.** New/changed CLI options belong in
  `src/nresx.CommandLine/README.md`; format behavior changes belong in the matching
  `docs/formats/*.md` page. If you edit XML doc comments in `src/nresx.Core`,
  regenerate the API docs: `dotnet tool restore` once, then
  `powershell -File scripts/gen-api-docs.ps1` (or `pwsh -File ...`), and commit
  the regenerated `docs/api/` output.
- **Short flags are contested territory.** `-v` is `--value` / `--version`,
  `-V` is `--verbose`. Check existing commands before claiming a new short flag.
- **Keep commits focused** with a single-line title that says what changed.

## PR checklist

- [ ] `dotnet test` passes locally
- [ ] Tests cover the change
- [ ] Docs updated (command reference / format pages / API regen, as applicable)
- [ ] No unrelated reformatting
