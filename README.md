# aaPatch

`aaPatch` is a unified patch-artifact generator for AVEVA System Platform Galaxy dump (CSV) files. It follows a structured transformation pipeline to filter, modify, and format Galaxy exports, enabling robust bulk attribute updates and automated modifications.

## Transformation Pipeline

The command follows a deterministic pipeline:

``` text
read → filter → patch → project → write
```

1.  **Read**: Loads AVEVA Galaxy dump (CSV), JSON, or plain CSV (from file or stdin).
2.  **Filter**: Restricts which objects proceed through the pipeline using attribute existence (`--has`) or complex expressions (`--where`).
3.  **Patch**: Applies modifications to object attributes using dynamic expressions.
4.  **Project**: Controls the final fields, applies aliases, and transforms values.
5.  **Write**: Outputs the result in AVEVA (CSV), JSON, or CSV format.

## Features

-   **Unified Artifact Generation**: Transform Galaxy dumps into customized outputs.
-   **Expression-Based Logic**: Use C#-style dynamic expressions powered by Dynamic LINQ for advanced filtering, patching, and projection.
-   **Bulk Attribute Updates**: Update object attributes across many objects simultaneously using targeted or global patches.
-   **Dynamic Projections**: Select, rename, and transform fields in the output.
-   **Multiple Formats & Auto-Detection**: Full support for native AVEVA CSV, standard CSV, and structured JSON with automatic format detection on input.
-   **Robust Galaxy Parsing**: Automatic handling of heterogeneous schemas and AVEVA column type metadata.
-   **Standard Stream Support**: Seamlessly integrates into shell pipelines.

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

| Option      | Shorthand | Description                                                                |
| :---------- | :-------- | :------------------------------------------------------------------------- |
| `--input`   | `-i`      | Path to the input file. If omitted, reads from stdin.                      |
| `--output`  | `-o`      | Path to the output file. If omitted, writes to stdout.                     |
| `--where`   | `-w`      | Filter objects using a dynamic expression.                                 |
| `--has`     | `-c`      | Filter objects that contain the specified attribute(s).                    |
| `--patch`   | `-p`      | Patch expression to apply to matching objects.                             |
| `--select`  | `-s`      | Select and transform fields for output.                                    |
| `--format`  | `-f`      | Output format: `aveva` (default), `json`, or `csv`.                        |

For detailed syntax rules, run:
```bash
aapatch info
```

## Input Format Auto-Detection

When reading from a file or standard input (`stdin`), `aaPatch` automatically inspects the raw text content to determine the input format:

1. **JSON**: Detected if the input text (after trimming leading whitespace) begins with an opening square bracket (`[`).
2. **AVEVA CSV**: Detected if the input text contains a line starting with `:TEMPLATE=` (case-insensitive), matching native AVEVA System Platform Galaxy dump exports.
3. **Standard CSV**: Defaulted if neither JSON nor AVEVA header markers are present.

You can convert between formats seamlessly by reading any supported format and specifying a different output format with `-f` / `--format` (`aveva`, `json`, or `csv`).

## Expression Syntax

`aaPatch` evaluates expressions in `--where`, `--patch`, and `--select` using [System.Linq.Dynamic.Core](https://dynamic-linq.net/) (Dynamic LINQ).

### Property Access & Attribute Values
Use curly braces to reference object attributes: `{AttributeName}`.

Under the hood, `{AttributeName}` translates to `it["AttributeName"]`, which returns an `AttributeValue` wrapper. Attribute lookups are case-insensitive and automatically resolve AVEVA column headers with type metadata (e.g., `{MyColumn}` resolves `MyColumn(MxInteger)`).

> **Important — Checking for Null Attributes (`IsNull`)**:
> Because `{Attribute}` returns an `AttributeValue` wrapper object, standard equality checks like `{Attribute} == null` will **not** work as expected. To check if an attribute is null or missing, always use the `.IsNull` property:
> - `--where '{Description}.IsNull'` (matches objects where Description is null or unset)
> - `--where '!{Description}.IsNull'` (matches objects where Description has a value)

### Custom `AttributeValue` API Methods

The `AttributeValue` class exposes several built-in helper methods that can be called directly within expressions:

| Method / Property | Return Type | Description |
| :--- | :--- | :--- |
| `.IsNull` | `bool` | Returns `true` if the underlying attribute value is null. |
| `.Contains(text, [matchCase])` | `bool` | Checks if the attribute value contains `text`. Case-insensitive by default; set `matchCase` to `true` for case-sensitive matching. |
| `.Like(pattern)` | `bool` | Performs SQL-style wildcard pattern matching (`%` matches zero or more characters, `?` matches a single character). |
| `.Matches(pattern)` | `bool` | Determines whether the attribute value matches a regular expression `pattern`. |
| `.Replace(find, replace, [match])` | `AttributeValue` | Replaces occurrences of `find` with `replace`. Case-insensitive by default; set `match` to `true` for case-sensitive matching. |
| `.As(type)` | `AttributeValue` | Converts the attribute value to the specified .NET type. |

### Filtering (`--where`)
Filter expressions must evaluate to a boolean (`bool`).
- `--where '{TagName}.StartsWith("PLC_")'`
- `--where '{Area} == "Production" && {IsRunning} == true'`
- `--where '{TagName}.Like("P_10?")'`
- `--where '{Description}.Contains("pump", false)'`
- `--where '{Address}.Matches(@"^192\.168\.\d+\.\d+$")'`
- `--where '!{Area}.IsNull'`

### Patching (`--patch`) & Find-and-Replace
Patches modify matching objects and support both targeted and global modes:

- **Targeted Patch**: `{Attribute} := Expression`
  Applies the expression strictly to the specified attribute.
  - Set static value: `-p '{Description} := "Standard Pump"'`
  - Calculation: `-p '{Setpoint} := {Setpoint} * 1.1'`
  - Find and replace (case-insensitive): `-p '{Description} := {Description}.Replace("OldText", "NewText")'`
  - Find and replace (case-sensitive): `-p '{TagName} := {TagName}.Replace("p_", "P_", true)'`

- **Global Patch**: `Expression`
  Evaluates the expression across all attributes on the object.
  - Global find and replace: `-p 'it.Replace("Area1", "Area2")'`

### Projection (`--select`)
- **Simple Selection**: `-s TagName`
- **Aliasing & Transformation**: `Alias := Expression`
  - `-s 'name := {TagName}'`
  - `-s 'display := !{Description}.IsNull ? {Description} : {TagName}'`

## Examples

### 1. Simple Attribute Update
Update the description for all objects:
```bash
aapatch -i Export.csv -o Patched.csv -p '{Description} := "Standardized Description"'
```

### 2. Targeted Find and Replace
Replace substring in specific attributes:
```bash
aapatch -i Export.csv -p '{TagName} := {TagName}.Replace("OLD_", "NEW_")'
```

### 3. Global Find and Replace
Replace text across all attributes in the dataset:
```bash
aapatch -i Export.csv -p 'it.Replace("BuildingA", "BuildingB")'
```

### 4. Filtering with `IsNull` and Pattern Matching
Find pumps with non-null areas matching a pattern and update setpoint:
```bash
aapatch -i Export.csv -w '!{Area}.IsNull && {TagName}.Like("P_10*")' -p '{Setpoint} := {Setpoint} * 1.1'
```

### 5. Selection and Format Conversion
Convert AVEVA CSV to formatted JSON with custom projections:
```bash
aapatch -i galaxy.csv -f json -s 'id := {RecordId}' -s TagName -s 'area := {Area}.ToUpper()'
```

### 6. Pipelining
Use `aaPatch` in a command-line pipeline:
```bash
cat Export.csv | aapatch -w '{Area} == "Utility"' -s TagName > Tags.txt
```

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
