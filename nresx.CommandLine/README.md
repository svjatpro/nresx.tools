Command line tool
================

- [Info](#info)
- [Add](#add)
- [Remove](#remove)

## Convert resource file(s) to another format

```
nresx convert [-s] <source file path> [-d <destination file path>] [-f <format>]
```

-s\
--source\
source file path/name

-d\
--destination\
destination file path/name, if no format is given, then format will be detected by destination file extension

-f\
--format\
destination file format


- Convert single resource file (res1.resx) to *.po format and save with new name (res2.po)

```nresx convert -s path1/res1.resx -d path2/res2.po```

- Convert single resource file to another format and save it in the same location with the original name, but with appropriate extension

```
nresx convert -s path1/res1.resx -f yaml
```
the result will be 'path1/res1.yaml' file in yaml format


## Format text entries in a resource file.

```
nresx format [-s] <source file path> [-d <destination file path>] [-f <format>]  
[--start-with | --end-with] [--culture-code | --language-code] [-r] [-p <pattern to be added>]
```

- Add prefix to all texts in a resource file.

```
nresx format [--source | -s] <source file path>
```

## Info
Get basic information about resource file(s).


```sh
nresx [info] [-s] <pathspec> [-r]
```

#### Options

**-s | --source** Resource file(s) to process, can be a pathspec\
**-r | --recursive** Process resource files in subdirectories\

#### Examples

```sh
# Will put information about two files to the stdout
nresx <file1> <file2>

# Will put to the stdout information about all *.yaml files in the current directory, including all subdirectories
nresx info *.resx -r
```


## List text elements from resource file.

```sh
nresx list [-s] <source file path> [-t <output template>]
```

-s\
--source\
source file path/name


-t\
--template\
Output line template for each text element in a resource file,\
possible tags are:\
- \k - element key
- \v - element value
- \c - element comment

the default template is "\k: \v"


examples: 

will list all elements from the <file1> in "<key>: <value>" format:

```
nresx list <file1>
```

will list all elements from the <file1> in "some prefix <key>: <value>, (<comment>)" format:

```
nresx list <file1> -t "some prefix \k: \v, (\c)"
```

## Add
Add resource item to resource file(s)

```sh
nresx add [-s] <pathspec> -k <element key> -v <element value> [-c <element comment>] [--dry-run] [--recursive]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-k | --key**  Element key\
**-v | --value**  Element value\
**-c | --comment**  Element comment\
**-n | --new** Will create resource file, if it not exist (with --recursive it will also create all subdirectories)
**--dry-run**

#### Examples

```sh
# will insert single element with "key1" key and "value1" value to the "file1" resource file
nresx add file1 -k key1 -v value1

# will insert single element with a comment
nresx add file1 -k key1 -v value1 -c "the comment1"

# will insert single element to two resource files
nresx add file1 file2 -k key1 -v value1

# will insert single element to all resource files, which match the pathspec, beginning from current directory, including all subdirectories
nresx add *.resw -r file2 -k key1 -v value1
```

## Remove
Remove resource elements(s) from resource file(s)

```sh
nresx remove [-s] <pathspec> [<pathspec> ..] [-k <element key> [<element key> ..]] [--empty | --empty-key | --empty-value] [--dry-run] [--recursive]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-k | --key**  element keys\
**--empty**  Will remove all elements with empty key OR value\
**--empty-key**  Will remove all elements with empty key\
**--empty-value**  Will remove all elements with empty value\
**--dry-run** 

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


## Update
resource item in resource file

```
nresx update <resource file path> -k <element key> [-v <element value>] [-c <element comment>]
```

-k\
--key\
element key

-v\
--value\
element value

-c\
--comment\
element comment

examples: 

```nresx update file1 -k key1 -v value1```

```nresx update file1 -k key1 -c "the comment1" -v "value1"```
