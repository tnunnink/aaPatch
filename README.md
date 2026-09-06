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
-   **Expression-Based Logic**: Use C#-style expressions for advanced filtering and patching.
-   **Bulk Attribute Updates**: Update object attributes across many objects simultaneously.
-   **Dynamic Projections**: Select, rename, and transform fields in the output.
-   **Multiple Formats**: Full support for native AVEVA CSV, standard CSV, and structured JSON.
-   **Robust Galaxy Parsing**: Automatic format detection and handling of heterogeneous schemas.
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

## Expression Syntax

`aaPatch` uses `System.Linq.Dynamic.Core` for expressions in `--where`, `--patch`, and `--select`.

### Property Access
Use curly braces to reference object attributes: `{AttributeName}`.

### Filtering (`--where`)
Expressions should return a boolean value.
-   `--where '{TagName}.StartsWith("PLC_")'`
-   `--where '{Area} == "Production" && {IsRunning} == true'`

### Patching (`--patch`)
Patches can be global or targeted.
-   **Targeted**: `{Attribute} := Expression`
    -   `-p '{Description} := "Updated: " + it'`
    -   `-p '{Value} := {Value} * 1.1'`
-   **Global**: `Expression` (Replaces values across all attributes based on the expression).

### Projection (`--select`)
-   **Simple Selection**: `-s TagName`
-   **Aliasing/Transformation**: `Alias := Expression`
    -   `-s 'name := {TagName}'`
    -   `-s 'display := {Description} ?? {TagName}'`

## Examples

### 1. Simple Attribute Update
Update the description for all objects:
```bash
aapatch -i Export.csv -o Patched.csv -p '{Description} := "Standardized Description"'
```

### 2. Complex Filtering
Update a PLC address only for specific pumps:
```bash
aapatch -i Export.csv -w '{Template} == "$Pump" && {TagName}.Like("P_10*")' -p '{Address} := "10.0.0.1"'
```

### 3. Calculation and Type Awareness
Increase a value by 10% for running objects:
```bash
aapatch -i Export.csv -w '{Status} == "Running"' -p '{Setpoint} := {Setpoint} * 1.1'
```

### 4. Selection and Transformation
Generate a simplified JSON inventory:
```bash
aapatch -i galaxy.csv -f json -s 'id := {RecordId}' -s TagName -s 'area := {Area}.ToUpper()'
```

### 5. Pipelining
Use `aaPatch` in a command-line pipeline:
```bash
cat Export.csv | aapatch -w '{Area} == "Utility"' -s TagName > Tags.txt
```

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
