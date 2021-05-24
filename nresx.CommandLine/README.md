Command line tool
================

- [Info](#info)
- [List](#list)
- [Add](#add)
- [Update](#update)
- [Rename](#rename)
- [Remove](#remove)
- [Copy](#copy)

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


## List
List text elements from resource file.

```sh
nresx list [-s] <pathspec> [-t <output template>]
```

#### Options

**-s | --source** Resource file(s) to process, can be a pathspec\
**-r | --recursive** Process resource files in subdirectories\
**-t | --template** Output row template for each text element in a resource file,\
possible tags are:\
- \k - element key
- \v - element value
- \c - element comment

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
nresx add [-s] <pathspec> -k <element key> -v <element value> [-c <element comment>] 
  [--dry-run] [--recursive]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**--new-file** Will create resource file, if it not exist (with --recursive it will also create all subdirectories)\
**-k | --key**  Element key\
**-v | --value**  Element value\
**-c | --comment**  Element comment\
**--dry-run**

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
nresx update [-s] <pathspec> -k <element key> [-v <element value>] [-c <element comment>] 
  [--dry-run] [--recursive] [--new-element]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-k | --key**  Element key\
**-v | --value**  Element value\
**-c | --comment**  Element comment\
**--new-element** Will create new element, if it not exist\
**--dry-run**

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
nresx rename [-s] <pathspec> -k <element key> -n <new key>
  [--dry-run] [--recursive]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-k | --key**  Element key\
**-n | --new-key**  New key\
**--dry-run**

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
nresx remove [-s] <pathspec> [<pathspec> ..] [-k <element key> [<element key> ..]] 
  [--empty | --empty-key | --empty-value] 
  [--dry-run] [--recursive]
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


## Copy
Copy resource elements(s) from one resource file to another

```sh
nresx copy [-s] <pathspec> [-d] <pathspec>
  [--skip | --overwrite]
  [--dry-run]
```

#### Options

**-s | --source**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**-r | --recursive**  Process resource files in subdirectories\
**-d | --destination**  Resource file(s) to process, can be a pathspec, or a list of pathspec\
**--skip**  Will skip duplicated elements (default option)
**--overwrite**  Will overwrite duplicated elements
**--dry-run**

#### Examples

```sh
# will copy all elements from the "file1" to "file2", if "file2" is not exist, it will be created
nresx copy <file1> <file2>

# will copy all elements from the "file1" to "file2", duplicated elements will be overwriten
nresx copy <file1> <file2> --overwrite
```
