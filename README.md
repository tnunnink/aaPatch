# aaPatch

`aaPatch` is a unified patch-artifact generator for AVEVA System Platform Galaxy dump (CSV) files. It follows a
structured transformation pipeline to filter, modify, and format Galaxy exports, enabling robust bulk attribute updates
and automated modifications.

## Transformation Pipeline

The command follows a deterministic pipeline:

``` text
read → filter → patch → select → format → write
```

1. **Read**: Loads AVEVA Galaxy dump CSV (from file or stdin).
2. **Filter**: Restricts which objects proceed through the pipeline.
3. **Patch**: Applies optional modifications to filtered objects.
4. **Select**: Controls the final fields and applies aliases.
5. **Format**: Emits the result as AVEVA (CSV) or JSON.
6. **Write**: Outputs the result (to file or stdout).

## Features

- **Unified Artifact Generation**: Transform Galaxy dumps into customized outputs.
- **Bulk Attribute Updates**: Update object attributes across many objects simultaneously.
- **Find and Replace**: Perform targeted string replacements within specific attributes or globally.
- **Advanced Filtering**: Restrict output by Template, TagName, or any attribute using wildcards.
- **Selection and Aliasing**: Project specific fields and rename them in the output.
- **Multiple Formats**: Support for native AVEVA CSV and structured JSON output.
- **Robust Galaxy Parsing**: Case-insensitive template detection with support for CRLF and LF line endings.
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

| Option         | Shorthand | Description                                                                                        |
|----------------|-----------|----------------------------------------------------------------------------------------------------|
| `--input`      | `-i`      | Path to the input Galaxy dump CSV file. If omitted, reads from stdin.                              |
| `--output`     | `-o`      | Path to the output CSV file. If omitted, writes to stdout.                                         |
| `--filter`     | `-f`      | Filter which objects are included in the output. Default attribute is TagName. Supports wildcards. |
| `--patch`      | `-p`      | Patch to apply to filtered objects. Can be used multiple times.                                    |
| `--select`     | `-s`      | Select and optionally alias fields for output. Can be used multiple times.                         |
| `--format`     |           | Output format: `aveva` (default) or `json`.                                                        |
| `--match-case` | `-m`      | Perform case-sensitive matching for find-replace operations.                                       |

### Patch Formats

There are three primary ways to modify attributes:

| Type                       | Syntax                   | Description                                                 | Example                     |
|:---------------------------|:-------------------------|:------------------------------------------------------------|:----------------------------|
| **Direct Assignment**      | `Attribute=Value`        | Sets the specified attribute to the exact value provided.   | `-p "Description=New Pump"` |
| **Attribute Find/Replace** | `Attribute:Find=Replace` | Replaces `Find` with `Replace` within a specific attribute. | `-p "Address:192=10"`       |
| **Global Find/Replace**    | `:Find=Replace`          | Replaces `Find` with `Replace` across **all** attributes.   | `-p ":OldSite=New"`         |

By default, find and replace operations are **case-insensitive**. Use the `--match-case` or `-m` flag for case-sensitive
matching:

```bash
aapatch -i Export.csv -p "Description:Pump=Motor" --match-case
```

### Selection and Aliases

The `--select` or `-s` option controls which attributes are included in the final output. If no selections are provided,
all fields are retained. Field order is preserved as specified in the command.

| Feature              | Syntax                | Description                                                           |
|:---------------------|:----------------------|:----------------------------------------------------------------------|
| **Simple Selection** | `--select TagName`    | Includes the specified attribute in the output.                       |
| **Aliasing**         | `--select Attr=Alias` | Renames the attribute. In JSON output, this becomes the property key. |

*Note: Duplicate projected names are rejected.*

### Output Formats

| Format              | Description                                              | Requirements / Behavior                             |
|:--------------------|:---------------------------------------------------------|:----------------------------------------------------|
| **AVEVA** (default) | Reconstructs native AVEVA template sections and columns. | Requires `Template` and `TagName` in the selection. |
| **JSON**            | Emits a formatted array of flat objects.                 | Values are mapped to numbers, Booleans, or strings. |

## Examples

### 1. Simple Attribute Update

Update the description for all objects in a dump file:

```bash
aapatch -i GalaxyExport.csv -o PatchedExport.csv -p "Description=Standardized Description"
```

### 2. Filtering Output

Update a PLC address for specific objects and only include them in the output:

```bash
aapatch -i Export.csv -p "ShortDesc:OldSystem=NewSystem" -f "Template=$Pump_Base" -f "P_10*"
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

### 6. Selection and Aliasing

Rename the description field and only include identity fields:

```bash
aapatch -i Export.csv -s Template -s TagName -s Description=displayName
```

### 7. JSON Output

Generate a JSON representation of specific attributes:

```bash
aapatch -i Export.csv -f "$Pump*" -s TagName=id -s Area --format json
```

### 5. Pipelining

Use `aaPatch` in a command-line pipeline:

```bash
cat GalaxyExport.csv | aapatch -p "Engine=AppEngine_002" > UpdatedExport.csv
```

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
