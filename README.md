Resourse files tools set (nresx.tools )
================

This project helps you to 
Command line tool for analysing and manipulation of .net localization solution.
- Converting resource files(s) to another formats
- Manage resourses
- Extract rources from 
- Inspect & validate resource files
- Help to analyse project to extract, validate and figureout project's resources

### Supported resource formats: 
- resx (resw) 
- yaml (yml)
- json
- plain text
- po

## Command line usage

### Installing the command line tool

... 

### Resource file converter

Convert single resource file to another format.

```
nresx convert [-s] <source file path> [-d <destination file path>] [-f <format>]
```

-s
--source
source file path/name

-d
--destination
destination file path/name, if no format is given, then format will be detected by destination file extension

-f
--format
destination file format

<details>
  <summary>available formats:</summary>
- resx
- resw
- yaml
- yml
- json
- txt
- po
</details>

- Convert single resource file (res1.resx) to *.po format and save with new name (res2.po)

```nresx convert -s path1/res1.resx -d path2/res2.po```

- Convert single resource file to another format and save it in the same location with the original name, but with appropriate extension

```
nresx convert -s path1/res1.resx -f yaml
```
the result will be 'path1/res1.yaml' file in yaml format


### Format text entries in a resource file.

```
nresx format [-s] <source file path> [-d <destination file path>] [-f <format>] [--start-with | --end-with] [--culture-code | --language-code] [-r] [-p <pattern to be added>]
```

- Add prefix to all texts in a resource file.

```
nresx format [--source | -s] <source file path>



## Class library usage