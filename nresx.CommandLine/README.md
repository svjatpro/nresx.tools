Command line tool
================

### Convert resource file(s) to another format

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


### Format text entries in a resource file.

```
nresx format [-s] <source file path> [-d <destination file path>] [-f <format>]  
[--start-with | --end-with] [--culture-code | --language-code] [-r] [-p <pattern to be added>]
```

- Add prefix to all texts in a resource file.

```
nresx format [--source | -s] <source file path>
```

### Get basic information about resource file(s).

```
nresx info [-s] <source file path> <source file path 2> ... 
```
-s\
--source\
source file path/name

also this is default command, so one can put just list of files

```
nresx <source file path> <source file path 2> ... 
```


### List text elements from resource file.

```
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


### Add resource item to resource file

```
nresx add <resource file path> -k <element key> -v <element value> [-c <element comment>]
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
```nresx add file1 -k key1 -v value1```
```nresx add file1 -k key1 -c "the comment1" -v "value1"```

### Remove resource item from resource file

```
nresx remove <resource file path> [-k] <element key>
```

examples: 
```nresx add file1 key1```
```nresx add file1 -k key1```



