# aaPatch

`aaPatch` is a simple command-line tool for patching and modifying AVEVA System Platform Galaxy dump (CSV) files. It
enables bulk attribute updates, find-replace operations, and automated modifications across multiple objects in Galaxy
exports, making it a handy tool for automation engineers working with ArchestrA-based systems.

## Features

- **Bulk Attribute Updates**: Update object attributes across many objects simultaneously.
- **Find and Replace**: Perform targeted string replacements within specific attributes.
- **Flexible Filtering**: Target objects by Template name or Tag name using wildcard patterns (e.g., `$Pump*`).
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

| Option        | Shorthand | Description                                                           |
|---------------|-----------|-----------------------------------------------------------------------|
| `--input`     | `-i`      | Path to the input Galaxy dump CSV file. If omitted, reads from stdin. |
| `--output`    | `-o`      | Path to the output CSV file. If omitted, writes to stdout.            |
| `--attribute` | `-a`      | Patch to apply. Can be used multiple times.                           |
| `--template`  |           | Filter objects by template name (supports wildcards like `*`).        |
| `--tag`       |           | Filter objects by tag name (supports wildcards like `*`).             |

### Patch Formats

There are two primary ways to modify attributes:

1. **Direct Assignment**: `Attribute=Value`
    - Sets the specified attribute to the exact value provided.
    - Example: `-a "Description=New Pump Description"`

2. **Find and Replace**: `Attribute:Find=Replace`
    - Searches for the `Find` string within the current attribute value and replaces it with `Replace`.
    - Example: `-a "Address:192.168.1=10.0.0"`

## Examples

### 1. Simple Attribute Update

Update the description for all objects in a dump file:

```bash
aapatch -i GalaxyExport.csv -o PatchedExport.csv -a "Description=Standardized Description"
```

### 2. Filtering by Template and Tag

Update the PLC address for all pumps that match a specific naming convention:

```bash
aapatch -i Export.csv -a "ShortDesc:OldSystem=NewSystem" --template "$Pump_Base" --tag "P_10*"
```

### 3. Multiple Operations

You can apply multiple patches in a single command:

```bash
aapatch -i Export.csv -a "Area=Production" -a "ScanGroup=Fast" -a "Comment:FIXME=DONE"
```

### 4. Pipelining

Use `aaPatch` in a command-line pipeline:

```bash
cat GalaxyExport.csv | aapatch -a "Engine=AppEngine_002" > UpdatedExport.csv
```

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
