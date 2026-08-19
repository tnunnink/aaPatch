# Next Release Plan

## Goals

This release focuses on three major capabilities:

1. Bidirectional AVEVA, standard CSV, and JSON formats.
2. Expression-based object filtering.
3. Universal attribute references and value interpolation.

The transformation pipeline remains:

```text
read → where/has → patch → add/interpolate → select → write
```

Making the tool fully generic or renaming it is outside the scope of this release.

## 1. Bidirectional Format Support

Support reading and writing:

- AVEVA Galaxy CSV
- Standard CSV
- Flat JSON arrays

All readers must produce the existing normalized `ObjectData` model so the transformation pipeline works consistently regardless of input format.

### Input Format Detection

Automatically detect the input format:

- AVEVA template marker → AVEVA CSV
- First non-whitespace character is `[` or `{` → JSON
- Otherwise → standard CSV

Consider adding an optional `--input-format` override if automatic detection proves ambiguous.

### AVEVA Input

- Preserve complete AVEVA headers, including type metadata.
- Normalize the template declaration into the `Template` attribute.
- Normalize `:Tagname` into the `TagName` attribute.
- Preserve duplicate logical names when complete headers differ.
- Parse typed values once when constructing attributes.

### Standard CSV Input

- Treat each row as an object.
- Treat each column header as an attribute.
- Preserve source column order.
- Treat values as strings initially unless explicit type inference is intentionally added.
- Allow `Template` and `TagName` columns to supply AVEVA identity fields.

### JSON Input

- Accept an array of flat objects.
- Use JSON property names as attribute names and headers.
- Preserve native JSON value types:
    - String
    - Boolean
    - Integer
    - Floating-point/decimal
    - Null
- Decide whether a single JSON object should be accepted as a one-object collection.

### AVEVA Output

When writing AVEVA:

- Require every object to contain `Template`.
- Require every object to contain `TagName`.
- Group objects by `Template`.
- Emit `TagName` as the first CSV column.
- Allow ordinary headers without AVEVA type metadata.
- Preserve original typed AVEVA headers when available.
- Produce clear errors identifying objects missing required identity fields.
- Validate `Template` and `TagName` only when AVEVA output is requested.

### Standard CSV Output

Define behavior for heterogeneous schemas:

- Build the union of headers across all objects.
- Preserve headers in first-seen order.
- Write an empty field when an object does not contain a header.
- Treat `Template` and `TagName` as ordinary columns.
- Preserve complete headers where available.

### JSON Output

- Continue emitting an array of flat objects.
- Use logical attribute names rather than AVEVA type-qualified headers.
- Preserve native value types.
- Continue rejecting duplicate JSON property names unless selection aliases resolve them.

## 2. Typed Attribute Values

Refactor `AttributeData` so its value is stored as its actual type rather than repeatedly derived from AVEVA header metadata.

Supported values:

- String
- Boolean
- Integer
- Floating-point/decimal
- Null

### Expected Behavior

- AVEVA input parses header metadata and converts the raw CSV value once.
- JSON input stores native JSON values directly.
- Standard CSV input stores strings unless type inference is explicitly introduced.
- `Value` returns the stored typed value.
- AVEVA writing formats typed values correctly.
- JSON writing serializes typed values naturally.
- Updates and additions preserve or intentionally assign value types.
- Attribute duplication and renaming preserve the typed value.

### Design Considerations

Keep these concepts distinct:

- `Header`: original or output column identity
- `Name`: logical attribute name
- `Value`: stored typed value
- AVEVA type metadata: source/output metadata parsed from the header when present

Do not require AVEVA type metadata to write valid AVEVA output.

## 3. Expression-Based Filtering

Replace the current filter option with:

```text
--where / -w
```

Examples:

```bash
--where '{Area} == "Packaging"'
--where '{Priority} > 10'
--where '{SignalType} == "AI" || {SignalType} == "AO"'
--where '{Area} == "Packaging" && ({Priority} < 5 || {Priority} > 10)'
```

### Requirements

- Investigate using `System.Linq.Dynamic.Core`.
- Parse or compile each expression once during command binding or setup.
- Do not parse the expression separately for every object.
- Rewrite `{Attribute}` references into `ObjectData` attribute access.
- Support:
    - Equality and inequality
    - Numeric comparisons
    - Boolean operators
    - Negation
    - Parentheses
- Multiple `--where` arguments use AND semantics.

### Attribute Lookup Semantics

- Exact complete-header matches take precedence.
- A unique logical-name match resolves successfully.
- Missing attributes cause the expression to evaluate as false.
- Ambiguous logical names throw a clear error.
- Exact AVEVA headers can disambiguate attributes.

Example:

```bash
--where '{Precision(MxInteger)} > 2'
```

### Expression Library Investigation

Verify how the selected expression library handles:

- `ObjectData` indexer access
- Indexer values typed as `object?`
- Numeric comparisons
- Boolean comparisons
- Null comparisons
- Missing values
- String operations
- Safe restriction of accessible members and methods

Avoid exposing unrestricted object APIs to user expressions.

## 4. Universal Attribute Reference Syntax

Adopt braces as the universal syntax for referencing an attribute inside expressions and value templates:

```text
{Attribute}
{Precision(MxInteger)}
```

Use this syntax for:

- `--where` expressions
- `--add` interpolation
- Direct-assignment `--patch` interpolation

Selection keeps its existing syntax:

```bash
--select TagName=name
--select Description
```

### Reference Resolution

- Lookup is case-insensitive.
- Exact complete-header matches take precedence.
- A unique logical-name match resolves successfully.
- Ambiguous logical names throw.
- Missing references follow the behavior of the containing operation.

### Escaping

Support doubled braces for literal braces:

```text
{{ → {
}} → }
```

Example:

```bash
--add 'Example={{TagName}}'
```

Result:

```text
{TagName}
```

Unmatched braces should produce a clear validation error.

## 5. Added-Field Interpolation

Extend `--add` so values may contain attribute references:

```bash
--add 'Source=AVEVA'
--add 'TagName={Area}_{DeviceNumber}'
--add 'InputSource=PLC1.{Address}'
```

### Rules

- A value without references remains a static value.
- Mixed text and references produce a string.
- A value consisting entirely of one reference should preserve the referenced value’s native type when practical.
- Additions execute sequentially.
- Later additions may reference fields created by earlier additions.
- Missing interpolation references skip that addition for the current object.
- Ambiguous interpolation references throw.
- Existing-header collisions remain errors.

Example:

```bash
--add 'DeviceName={Area}_{Number}' \
--add 'QualifiedName=Plant1.{DeviceName}'
```

## 6. Patch Interpolation

Support attribute references in direct-assignment patches:

```bash
--patch 'Description={Area} {DeviceType} {DeviceNumber}'
--patch 'InputSource=PLC1.{Address}'
```

### Rules

- Resolve interpolation separately for every object.
- Earlier patches are visible to later patches.
- Missing patch targets remain skipped.
- Missing interpolation references skip that patch for the current object.
- Ambiguous target or referenced attributes throw.
- A direct assignment consisting entirely of one reference should preserve its native type when practical.
- Mixed text and references produce a string.

Keep find/replace behavior unchanged initially:

```text
Attribute:Find=Replacement
:Find=Replacement
```

Do not add interpolation to find/replace until there is a concrete requirement.

## 7. CLI Option Changes

Use:

```text
--where  / -w
--has    / -h
--format / -f
--patch  / -p
--add    / -a
--select / -s
```

Verify whether CliFx reserves `-h` for help. If it does, expose `--has` without a short alias or choose another shortcut.

### `--has` Behavior

Preserve schema-existence filtering:

```bash
--has IOAddress
--has 'Precision(MxInteger)'
```

Rules:

- Repeatable with AND semantics.
- Accept logical names or exact complete headers.
- Missing attributes exclude the object.
- Multiple logical-name matches count as existing because the operation only tests presence.
- Exact complete headers test for that specific field.

## 8. Strongly Typed CLI Arguments

Continue parsing command arguments once through CliFx-compatible types.

Potential types:

- `WhereExpression`
- `ObjectPatch`
- `FieldSelection`
- Shared interpolation/value-template type for additions and patches

Requirements:

- Constructors or `Parse` methods validate syntax once.
- Per-object operations evaluate already parsed structures.
- Keep object-specific lookup and mutation behavior in `ObjectData`.
- Do not repeatedly split or parse raw command strings for every object.

`--has` may remain a string collection because it has no additional expression syntax.

## 9. Error and Lax-Behavior Semantics

Apply these rules consistently.

### Missing Attributes

- `--where`: expression evaluates false.
- `--has`: object is excluded.
- `--patch` target: patch is skipped.
- `--select`: field is omitted from that object.
- Interpolation source: containing addition or patch is skipped for that object.

### Ambiguous Attributes

Ambiguous logical-name access should throw for:

- `--where`
- `--patch`
- `--select`
- Interpolation

The error should list complete headers that can be used to disambiguate:

```text
Attribute 'Precision' is ambiguous. Use an exact header:
  Precision(MxInteger)
  Precision(MxChoice)
```

For `--has`, ambiguity should count as existence rather than throw.

## 10. Testing

Add focused tests following the existing test organization and naming conventions.

### Format Tests

- Detect AVEVA input.
- Detect standard CSV input.
- Detect JSON input.
- Read standard CSV into `ObjectData`.
- Read JSON into `ObjectData`.
- Preserve JSON string, Boolean, numeric, and null values.
- Convert AVEVA to JSON.
- Convert AVEVA to standard CSV.
- Convert standard CSV to AVEVA.
- Convert JSON to AVEVA.
- Reject AVEVA output when `Template` is missing.
- Reject AVEVA output when `TagName` is missing.
- Preserve typed AVEVA headers.
- Write untyped standard headers to AVEVA.
- Handle heterogeneous schemas in standard CSV output.
- Handle empty input and empty output consistently.

### Expression Tests

- String equality and inequality.
- Numeric comparisons.
- Boolean comparisons.
- Null comparisons.
- AND conditions.
- OR conditions.
- Negation.
- Nested parentheses.
- Multiple `--where` options use AND semantics.
- Missing attributes evaluate false.
- Unique logical names resolve.
- Ambiguous logical names throw.
- Exact complete headers resolve ambiguous attributes.
- Expressions are parsed or compiled only once.

### Interpolation Tests

- Static addition remains unchanged.
- Addition with one reference.
- Addition with multiple references.
- Addition with mixed static text and references.
- Whole-value reference preserves native type.
- Mixed interpolation produces a string.
- Sequential additions can reference earlier additions.
- Patch interpolation uses current object values.
- Sequential patches see earlier changes.
- Missing interpolation references skip the operation.
- Ambiguous interpolation references throw.
- Literal braces are escaped with doubled braces.
- Unmatched braces throw a clear error.

### Existing Behavior Regression Tests

- `--has` remains repeatable with AND semantics.
- Missing patch targets are skipped.
- Missing selections are omitted.
- Selection aliases preserve order.
- JSON duplicate logical names still require aliases.
- AVEVA identity requirements remain output-format specific.
- Existing find/replace patch behavior remains unchanged.

## Suggested Implementation Order

1. Refactor `AttributeData` to store typed values.
2. Update AVEVA reading and writing for the typed model.
3. Add standard CSV reading and writing.
4. Add JSON reading.
5. Add input-format detection.
6. Add the shared attribute-reference parser.
7. Add interpolation templates.
8. Apply interpolation to additions.
9. Apply interpolation to direct-assignment patches.
10. Investigate and implement expression-based `--where`.
11. Update CLI names and shortcuts.
12. Add end-to-end tests and documentation.

## Acceptance Criteria

- AVEVA, standard CSV, and JSON can all be read.
- AVEVA, standard CSV, and JSON can all be written.
- JSON and standard CSV can produce valid AVEVA imports when `Template` and `TagName` are present.
- Typed JSON values remain typed through the transformation pipeline.
- `--where` supports useful Boolean expressions with attribute references.
- `{Attribute}` works consistently in filters and interpolation.
- Additions and direct-assignment patches support interpolation.
- Missing attributes follow the documented lax behavior.
- Ambiguous attribute access fails clearly and can be resolved with exact headers.
- Existing patch, add, selection, and schema filtering behavior does not regress.

## Deferred

- Making the tool fully generic or renaming it
- Expression-based selections
- A full custom expression language
- Advanced interpolation functions
- Interpolation in find/replace operations
- Configuration files
- Arbitrary operation ordering within one invocation
- Ignition-specific output
- Public configurable grouped-CSV profiles
