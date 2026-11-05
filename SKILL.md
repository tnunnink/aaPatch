---
name: aapatch
description: Official CLI skill for inspecting, filtering, patching, and transforming AVEVA System Platform Galaxy dump CSV files.
triggers:
  - AVEVA Galaxy dump
  - Galaxy export CSV
  - :TEMPLATE=
  - patching tag attributes
  - bulk editing galaxy objects
---

# AVEVA Galaxy Patch Tool (`aapatch`)

When working with AVEVA System Platform Galaxy dump exports (multi-template CSV files with `:TEMPLATE=` headers and `:Tagname` columns), **ALWAYS use the `aapatch` CLI tool**.

Do **NOT** write custom regex scripts, ad-hoc Python/PowerShell CSV parsers, or multi-line text replacements for Galaxy dumps. Galaxy dumps have heterogeneous schemas per template and type-annotated headers (e.g. `MyColumn(MxInteger)`) that ad-hoc scripts frequently corrupt. `aapatch` guarantees valid AVEVA parsing, type preservation, and re-import serialization.

---

## Availability & Installation

Verify if `aapatch` is available in the environment:

```bash
aapatch --version
```

If not installed, install it globally using the .NET SDK:

```bash
dotnet tool install --global aaPatch
```

To view runtime rules and expression syntax:

```bash
aapatch info
```

---

## Core Agent Workflows

### 1. Surgical Patch Generation (Galaxy Re-Import)

When generating patch files to re-import into AVEVA Galaxy, **always pass `--prune` (or `-u`)**. This prunes unmodified columns and drops instances with zero changes, creating clean, surgical update artifacts:

```bash
aapatch -i Dump.csv -p '{Description} := {Description}.Replace("OldText", "NewText")' --prune -o Patch.csv
```

### 2. Cross-Field References in Patches

Targeted patches (`{Target} := Expression`) evaluate in the context of the object. Reference any attribute on the object using `{AttributeName}`:

Derive an IO reference dynamically from `TagName`:

```bash
aapatch -i Dump.csv -p '{InputSource} := "PLC_" + {TagName} + ".PV"' --prune -o Patch.csv
```

Cross-field find-and-replace:

```bash
aapatch -i Dump.csv -p '{InputSource} := {InputSource}.Replace("OLD", {TagName})' --prune -o Patch.csv
```

### 3. Global Find-and-Replace Across All Attributes

Replace values across all attributes without specifying target columns:

```bash
aapatch -i Dump.csv -p 'it.Replace("BuildingA", "BuildingB")' --prune -o Patch.csv
```

### 4. Wildcard Column Selection & Trimming

When extracting specific columns, use wildcards (`*`, `%` for zero or more characters, `?` for single character) without listing every column name. Identity columns (`Template`, `TagName`) are preserved automatically for AVEVA output:

```bash
aapatch -i Dump.csv -s '%10%InputSource' -o Inputs.csv
```

### String Helpers in Expressions

Use the built-in `AttributeValue` methods directly in `--patch`, `--select`, and `--where` expressions for common string transformations:

| Method | Description |
| :--- | :--- |
| `.Extract(pattern)` / `.Extract(pattern, group)` | Returns a regex capture group (group 1 by default), or null when no capture is found. |
| `.Split(separator)` / `.Split(separator, index)` | Returns a token by index; negative indices count from the end. |
| `.Slice(start)` / `.Slice(start, length)` | Returns a bounds-clamped substring; negative starts count backward from the end. |
| `.Before(delimiter)` / `.After(delimiter)` | Returns text before or after the first delimiter occurrence. |
| `.BeforeLast(delimiter)` / `.AfterLast(delimiter)` | Returns text before or after the last delimiter occurrence. |
| `.PadLeft(totalWidth, [paddingChar])` | Left-pads to the requested width, using `"0"` by default. |
| `.Trim()` | Removes leading and trailing whitespace. |
| `.ToUpper()` / `.ToLower()` | Converts using invariant casing. |

Missing captures, tokens, or delimiters return null; null attribute values remain null. For example, derive an input source from the portion of a fictional device tag before `_MOTOR`:

```bash
aapatch -i Dump.csv -p '{InputSource} := "PLC_" + {TagName}.Before("_MOTOR") + ".PV"' --prune -o Patch.csv
```

To extract the motor number with a regular expression:

```bash
aapatch -i Dump.csv -p '{InputSource} := "Motor_" + {TagName}.Extract("MOTOR_?(\\d+)")' --prune -o Patch.csv
```

---

## Quick Reference CLI Options

| Option | Flag | Description |
| :--- | :--- | :--- |
| `--input` | `-i` | Input file path (defaults to standard input). |
| `--output` | `-o` | Output file path (defaults to standard output). |
| `--where` | `-w` | Filter objects by boolean expression (e.g. `-w '{Area} == "Utility"'`). |
| `--has` | `-c` | Filter objects containing attribute name (e.g. `-c InputSource`). |
| `--patch` | `-p` | Patch expression (`{Target} := Expr` or `it.Replace(...)`). |
| `--select` | `-s` | Field selection, alias (`Alias := Expr`), or wildcard pattern (`%Input%`). |
| `--prune` | `-u` | Prune unchanged columns and instances (surgical patch mode). |
| `--format` | `-f` | Output format: `aveva` (default), `json`, or `csv`. |