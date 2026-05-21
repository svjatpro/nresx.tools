Commands
================

- [Convert](#convert)
- [Format](#format)
- [Info](#info)
- [List](#list)
- [Add](#add)
- [Update](#update)
- [Rename](#rename)
- [Remove](#remove)
- [Copy](#copy)
- [Validate](#validate)
- [Generate](#generate)
- [Exit codes](#exit-codes)
- [Scripting nresx](#scripting-nresx)

## Convert
Convert resource file(s) to another format

```sh
nresx convert
  [-s | --source] <pathspec>
  [-r | --recursive]
  [-d | --destination <pathspec>]
  [-f | --format <format>]
  [--dry-run]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-d | --destination**  Destination resource file(s), can be a pathspec\
**-f | --format**  Format of destination file(s)\
**--dry-run** Execute the command in test mode. No modifications will be made to existing files or the creation of new files.

#### Examples

```sh
# will convert single resource file (res1.resx) to .po format and save with new name (res2.po)
nresx convert path1/res1.resx path2/res2.po

# will convert single resource file (res1.resx) to .yaml format and save as (res1.yaml) in the same folder
nresx convert res1.resx -f yaml

# will convert two resource files to .yaml format and save as (res1.yaml, res2.yaml) in the same folder
nresx convert -s res1.resx res2.resx -f yaml

# will convert all resource files in current folder to .yaml format 
#   and save with the same name (but with .yaml extension) in the current folder
nresx convert *.resx -f yaml

# will convert all resource files in current folder and all subdirectories
#   to .yaml format and save with the same name (but with .yaml extension) in an appropriate folder
nresx convert *.resx -f yaml --recursive

# will convert all resource files in current folder and all subdirectories
#   to .yaml format and save with the same name (but with .yaml extension) in "dir1/dir2" folder
#   if there is a duplicated file names, then second one will be failed with "duplicated name" exception
nresx convert *.resx dir1/dir2/*.yaml -r
```


## Format
Format text entries in a resource file(s).

```sh
nresx format
  [-s | --source] <pathspec>
  [-r | --recursive]
  [-p | --pattern <pattern>] [-c | --culture-code] [-l | --language-code]
  [--start-with | --end-with]
  [--delete]
  [--dry-run]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-p | --pattern**  Custom pattern to apply to element value\
**-l | --language-code**  Use language code (two letter ISO name) as a format pattern\
**-c | --culture-code**  Use culture code as a format pattern\
**--start-with**  Add or remove new part at the beginning of elements value\
**--end-with**  Add or remove new part at the end of elements value\
**--delete**  remove format pattern from element value\
**--dry-run** Execute the command in test mode. No modifications will be made to existing files or the creation of new files.

#### Examples

```sh
# will format all elements in res1.resx file as 'fr_<value>'
nresx format Resources\fr-CA\res1.resx --start-with --language-code

# will revert previous formatting - remove 'fr_' prefix from all elements
nresx format Resources\fr-CA\res1.resx --start-with --language-code --delete

# will format all elements in all *.resx file starting from Resources\ dir as 'cultureName_<value>'
#  with appropriate culture: all elements in fr-CA\res1.resx will be formatted as 'fr-CA_<value>' etc.
nresx format Resources\*.resx --start-with --culture-code --recursive
```

## Info
Get basic information about resource file(s).

```sh
nresx [info]
  [-s | --source] <pathspec> 
  [-r | --recursive]
```

#### Options

**-s | --source** Resource file(s) to process, can be a pathspec\
**-r | --recursive** Process resource files in subdirectories
#### Examples

```sh
# Will put information about two files to the stdout
nresx <file1> <file2>

# Will put to the stdout information about all *.yaml files in the current directory, including all subdirectories
nresx info *.resx -r
```


## List
List text elements from resource file.

```sh
nresx list
  [-s | --source] <pathspec> 
  [-t | --template <output template>]
```

#### Options

**-s | --source** Resource file(s) to process, can be a pathspec\
**-t | --template** Output row template for each text element in a resource file,possible tags are:- \k - element key- \v - element value- \c - element comment

the default template is "\k: \v"

#### Examples

```sh
# Will list all elements from the <file1> in "<key>: <value>" format:
nresx list <file1>

# will list all elements from the <file1> in "some prefix <key>: <value>, (<comment>)" format:
nresx list <file1> -t "some prefix \k: \v, (\c)"
```


## Add
Add resource item to resource file(s)

```sh
nresx add 
  [-s | --source] <pathspec>
  [-r | --recursive]
  [-k | --key <element key>]
  [-v | --value <element value>]
  [-c | --comment <element comment>] 
  [--new-file]
  [--dry-run]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-k | --key**  Element key\
**-v | --value**  Element value\
**-c | --comment**  Element comment\
**--new-file** Will create resource file, if it not exist (with --recursive it will also create all subdirectories)\
**--dry-run** Execute the command in test mode. No modifications will be made to existing files or the creation of new files.

#### Examples

```sh
# will insert single element with "key1" key and "value1" value to the "file1" resource file
nresx add file1 -k key1 -v value1

# will insert single element with a comment
nresx add file1 -k key1 -v value1 -c "the comment1"

# will insert single element to two resource files
nresx add file1 file2 -k key1 -v value1

# will insert single element to all resource files, which match the pathspec, 
#  beginning from current directory, including all subdirectories
nresx add *.resw -r -k key1 -v value1
```


## Update
Update resource item in resource file(s)

```sh
nresx update
  [-s | --source] <pathspec>
  [-r | --recursive]
  [-k | --key <element key>]
  [-v | --value <element value>]
  [-c | --comment <element comment>] 
  [--new-element]
  [--dry-run]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-k | --key**  Element key\
**-v | --value**  Element value\
**-c | --comment**  Element comment\
**--new-element** Will create new element, if it not exist\
**--dry-run** Execute the command in test mode. No modifications will be made to existing files or the creation of new files.

#### Examples

```sh
# will update single element with new value in the "file1" resource file
nresx update file1 -k key1 -v value1

# will update single element with new value and comment resource file
nresx update file1 -k key1 -c "the comment1" -v "value1"

# will update single element value in two resource files
nresx update file1 file2 -k key1 -v value1

# will update single element in all resource files, which match the pathspec, 
#  beginning from current directory, including all subdirectories
nresx update *.resw -r -k key1 -v value1
```


## Rename
Rename resource item in resource file(s)

```sh
nresx rename
  [-s | --source] <pathspec>
  [-r | --recursive]
  [-k | --key <element key>]
  [-n | --new-key <new key>]
  [--new-file]
  [--dry-run]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-k | --key**  Element key\
**-n | --new-key**  New key\
**--new-file** Will create destination file(s) if they do not exist\
**--dry-run** Execute the command in test mode. No modifications will be made to existing files or the creation of new files.

#### Examples

```sh
# will rename single element in the "file1" resource file
nresx rename file1 -k key1 -n key2

# will rename single element in all *.resx files, starting from current directory
nresx rename *.resx -k key1 -n key2 --recursive
```


## Remove
Remove resource elements(s) from resource file(s)

```sh
nresx remove
  [-s | --source] <pathspec>
  [-r | --recursive]
  [-k | --key <element key> [<element key> ..]]
  [--empty | --empty-key | --empty-value]
  [--dry-run]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-k | --key**  element keys\
**--empty**  Will remove all elements with empty key OR value\
**--empty-key**  Will remove all elements with empty key\
**--empty-value**  Will remove all elements with empty value\
**--dry-run** Execute the command in test mode. No modifications will be made to existing files or the creation of new files.

#### Examples

```sh
# will remove single element with "key1" key from the "file1" resource file
nresx remove <file1> -k <key1>

# will remove two elements by key from the "file1" and "file2" resource file
nresx remove -s <file1> <file2> -k <key1> <key2>

# will remove from "file1" all items, which have empty value
nresx remove <file1> --empty-value

# will remove from all *.yaml files in current dir, including subirectories all items, which have empty key or value
nresx remove *.yaml -r --empty
```


## Copy
Copy resource elements(s) from one resource file to another

```sh
nresx copy
  [-s | --source] <pathspec>
  [-r | --recursive]
  [-d | --destination <pathspec>]
  [--skip | --overwrite]
  [--new-file]
  [--dry-run]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-d | --destination**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**--skip**  Will skip duplicated elements (default option)\
**--overwrite**  Will overwrite duplicated elements\
**--new-file** Will create destination file(s) if they do not exist\
**--dry-run** Execute the command in test mode. No modifications will be made to existing files or the creation of new files.

#### Examples

```sh
# will copy all elements from the "file1" to "file2", if "file2" is not exist, it will be created
nresx copy <file1> <file2>

# will copy all elements from the "file1" to "file2", duplicated elements will be overwriten
nresx copy <file1> <file2> --overwrite
```


## Validate
Validate resource(s) in order to find any errors, such as duplicated elements, missed elements or not translated elements.

```sh
nresx validate
  [-s | --source] <pathspec>
  [-r | --recursive]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories
#### Examples

```sh
# will validate elements within single resource file: emptly or duplicated elements
nresx validate <file1>

# will validate elements in all matched resource files: including cross resource validates
#  if some element has the same value in two resource files for different cultures, 
#  then there will be 'not translated' error in the result.
nresx validate dir1\*.resw -r
```

## Generate
Extract potential texts from source code, replace with placeholder code and generate new resource file

```sh
nresx generate
  [-s | --source] <pathspec>
  [-r | --recursive]
  [-d | --destination <pathspec>]
  [-f | --format <format>]
  [--new-file]
  [--link]
  [--exclude <dirs>]
  [--dry-run]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-d | --destination**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-f | --format**  Format of destination file(s)\
**--new-file** Will create resource file, if it not exist (with --recursive it will also create all subdirectories)\
**--link** Replace existing texts in project source code with links to localized resources.\
**--exclude** Exclude directories when generating resources. Accepts a comma-separated list or quoted names. Default: ".git,.vs,bin,obj"
**--dry-run** Execute the command in test mode. No modifications will be made to existing files or the creation of new files.\

#### Examples

```sh
# will search all source files in current dir and all subdirs, extract all appropriate tests, replace with placeholder code and generate new resource file with extracted elements
nresx generate * <file1> -r
```


## Exit codes

Every command returns one of the codes below. Use them in CI and shell scripts to react to specific failure modes; the error message on stderr explains the details.

| Code | Name                  | When it happens                                                                                                  |
| ---- | --------------------- | ---------------------------------------------------------------------------------------------------------------- |
| 0    | success               | Command completed without errors.                                                                                |
| 1    | general failure       | Catch-all runtime error after arguments parsed (unexpected exception, write failure, etc.).                      |
| 2    | usage error           | Argument parser rejected the command line, or a required option was missing.                                    |
| 3    | not found             | Source file mask matched nothing, a directory does not exist, or a requested element key is missing from a file. |
| 4    | format error          | Resource format could not be determined, or a file failed to load because its content was not recognized.       |
| 5    | destination conflict  | Destination already exists, or a destination file does not exist and `--new-file` was not passed.                |

When multiple errors occur in a single run, the first one wins - the exit code reflects the earliest failure.

#### Per-command exit codes

| Command  | 0 | 1 | 2 | 3 | 4 | 5 |
| -------- |---|---|---|---|---|---|
| info     | x | x | x | x | x |   |
| list     | x | x | x | x | x |   |
| convert  | x | x | x | x | x | x |
| format   | x | x | x | x | x |   |
| copy     | x | x | x | x | x | x |
| add      | x | x | x | x | x |   |
| update   | x | x | x | x | x |   |
| rename   | x | x | x | x | x |   |
| remove   | x | x | x | x | x |   |
| validate | x | x | x | x | x |   |
| generate | x | x | x | x | x | x |
| version  | x | x |   |   |   |   |
| help     | x |   | x |   |   |   |

## Scripting nresx

```sh
# branch on exit code in bash
nresx info "$file"
case $? in
  0) echo "ok" ;;
  3) echo "missing - skipping" ;;
  4) echo "unrecognized format - logging and continuing" ;;
  *) echo "unexpected failure" >&2; exit 1 ;;
esac
```

```powershell
# branch on exit code in PowerShell
nresx info $file
switch ($LASTEXITCODE) {
    0 { Write-Host "ok" }
    3 { Write-Host "missing - skipping" }
    4 { Write-Host "unrecognized format - logging and continuing" }
    default { Write-Error "unexpected failure"; exit 1 }
}
```

Diagnostic output (the per-file result lines, summaries) is on stdout. Fatal errors are on stderr. Redirect them independently when scripting:

```sh
nresx convert *.resx -f yaml 2> errors.log > converted.log
```
