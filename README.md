# aaPatch

`aaPatch` is a simple command-line tool for patching and modifying AVEVA System Platform Galaxy dump (CSV) files. It
enables bulk attribute updates, find-replace operations, and automated modifications across multiple objects in Galaxy
exports, making it a handy tool for automation engineers working with ArchestrA-based systems.

## Features

- **Bulk Attribute Updates**: Update object attributes across many objects simultaneously.
- **Find and Replace**: Perform targeted string replacements within specific attributes or globally across all attributes.
- **Advanced Filtering**: Target objects by Template name, Tag name, or any attribute value using wildcard patterns (e.g., `*`).
- **Preview Mode**: Review exactly what changes will be made before applying them.
- **Standard Stream Support**: Seamlessly integrates into pipelines using stdin and stdout.
- **Cross-Platform**: Built on .NET 10, running on Windows, Linux, and macOS.

## Installation

`aaPatch` is distributed as a .NET Tool. You can install it using the .NET SDK:

```bash
dotnet tool install --global aaPatch
```

*Note: Requires .NET 10.0 Runtime or SDK.*

## Usage

The basic syntax for `aaPatch` is:

```bash
aapatch [options]
```

### Options

| Option        | Shorthand | Description                                                                                                   |
|---------------|-----------|---------------------------------------------------------------------------------------------------------------|
| `--input`     | `-i`      | Path to the input Galaxy dump CSV file. If omitted, reads from stdin.                                         |
| `--output`    | `-o`      | Path to the output CSV file. If omitted, writes to stdout.                                                    |
| `--patch`     | `-p`      | Patch to apply. Can be used multiple times.                                                                   |
| `--filter`    | `-f`      | Filter objects by attribute value or TagName (e.g. `Description=Pump*` or just `P_10*`). Supports wildcards. |
| `--templates` | `-t`      | Filter objects by template name. Supports wildcards.                                                          |
| `--preview`   |           | Preview changes on stderr without modifying any data.                                                         |

### Patch Formats

There are three primary ways to modify attributes:

1. **Direct Assignment**: `Attribute=Value`
    - Sets the specified attribute to the exact value provided.
    - Example: `-p "Description=New Pump Description"`

2. **Attribute-specific Find and Replace**: `Attribute:Find=Replace`
    - Searches for the `Find` string within the specific attribute and replaces it with `Replace`.
    - Example: `-p "Address:192.168.1=10.0.0"`

3. **Global Find and Replace**: `:Find=Replace`
    - Searches for the `Find` string across **all** attributes of the matching object and replaces it with `Replace`.
    - Example: `-p ":OldSite=NewSite"`

## Examples

### 1. Simple Attribute Update

Update the description for all objects in a dump file:

```bash
aapatch -i GalaxyExport.csv -o PatchedExport.csv -p "Description=Standardized Description"
```

### 2. Filtering and Previewing

Preview a PLC address update for all pumps that match a specific naming convention without actually changing the file:

```bash
aapatch -i Export.csv -p "ShortDesc:OldSystem=NewSystem" -t "$Pump_Base" -f "P_10*" --preview
```

### 3. Attribute-based Filtering

Update an attribute only for objects where another attribute matches a pattern:

```bash
aapatch -i Export.csv -p "ScanGroup=Fast" -f "Area=Production*"
```

### 4. Multiple Operations and Global Replace

Apply multiple patches including a global find-replace in a single command:

```bash
aapatch -i Export.csv -p "Area=Production" -p "Comment:FIXME=DONE" -p ":OldServer=NewServer"
```

### 5. Pipelining

Use `aaPatch` in a command-line pipeline:

```bash
cat GalaxyExport.csv | aapatch -p "Engine=AppEngine_002" > UpdatedExport.csv
```

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
