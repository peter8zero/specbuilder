# Specbuilder & Code Inventory — End-to-End Process

How the spec builder knows what code exists, from C# attribute to exported spec sheet.

---

## Overview

```
C# codebase          extract_spec_options.py       specbuilder (index.html)
─────────────        ───────────────────────       ────────────────────────
[SpecOption]    ──►  Parses attributes        ──►  Loads code-options.js
[SpecCapability]     Generates JS files            Loads code-capabilities.js
                                                   Shows inventory panel
                                                   Includes in exports
```

There are three roles in this flow:

| Role | What they do |
|------|-------------|
| **Developer** | Decorates C# classes and methods with `[SpecOption]` and `[SpecCapability]` attributes |
| **Extractor operator** | Runs `extract_spec_options.py` to generate JS files from the codebase |
| **Analyst** | Uses the specbuilder to configure a spec, referencing the code inventory panel as they go |

---

## Part 1: Decorating C# Code (Developer)

### Setup

Three files need to exist in your C# project. Copy them from `spec-option-extractor/`:

| File | Purpose |
|------|---------|
| `SpecOptionAttribute.cs` | Attribute class for marking classes |
| `SpecCapabilityAttribute.cs` | Attribute class for marking methods |
| `SpecCategories.cs` | Constants for category names (prevents typos) |

All live in the `Calculate.Attributes` namespace.

### Adding `[SpecOption]` to a class

Use this when a class represents a **selectable calculation strategy** — something an analyst would choose when building a spec.

```csharp
using Calculate.Attributes;

[SpecOption(
    Category = SpecCategories.Revaluation,
    Name = "CPI-Capped (s101)",
    Description = "Statutory revaluation capped at CPI rather than RPI.",
    WhyItMatters = "The standard for post-97 deferred benefits under most modern schemes."
)]
public class CpiCappedRevaluation : IRevaluationStrategy
{
    // ...
}
```

| Parameter | Required | Description |
|-----------|----------|-------------|
| `Category` | Yes | Use a `SpecCategories` constant: `Revaluation`, `Commutation`, `GMP`, `Transfers`, `Builder` |
| `Name` | Yes | Plain-English display name shown in the specbuilder UI |
| `Description` | No | What this option does — one sentence |
| `WhyItMatters` | No | Why an analyst would care — business context |

### Adding `[SpecCapability]` to a method

Use this when a method represents a **specific capability** that the analyst or coder should know exists — a sub-feature within a class.

```csharp
[SpecCapability(
    Category = SpecCategories.GMP,
    Name = "Anti-Franking Check",
    Description = "Checks whether excess pension above GMP is sufficient to cover GMP increases.",
    WhyItMatters = "Prevents schemes from using GMP step-ups to reduce the total pension paid."
)]
public bool CheckAntiFranking(decimal totalPension, decimal gmpAmount, decimal gmpIncrease)
{
    return (totalPension - gmpAmount) >= gmpIncrease;
}
```

Same parameters as `[SpecOption]`. The extractor automatically captures:
- Method name, return type, and parameter signature
- The parent class (if the class also has `[SpecOption]`, the capability is linked to it)

### Categories

Use the constants in `SpecCategories.cs` rather than raw strings:

```csharp
public static class SpecCategories
{
    public const string Revaluation = "Revaluation";
    public const string Commutation = "Commutation";
    public const string GMP = "GMP";
    public const string Transfers = "Transfers";
    public const string Builder = "Builder";
}
```

To add a new category: add a constant here, then use it in your attributes. The extractor and specbuilder handle new categories automatically — no changes needed downstream.

### What to decorate (and what not to)

**Do decorate:**
- Strategy classes that implement a calculation the analyst would configure (revaluation methods, GMP handling, commutation styles)
- Public methods that represent distinct capabilities a coder would want to know about

**Don't decorate:**
- Internal helper classes, utilities, base classes
- Private methods, constructors
- Classes that are purely infrastructure (DI registration, logging, etc.)

### Example: full class with both attributes

```csharp
[SpecOption(
    Category = SpecCategories.GMP,
    Name = "GMP Equalisation (Dual Record)",
    Description = "Applies GMP equalisation using the dual-record method per Lloyds.",
    WhyItMatters = "Required for schemes with members who have GMP service between 1990-1997."
)]
public class GmpEqualiser : IGmpStrategy
{
    [SpecCapability(
        Category = SpecCategories.GMP,
        Name = "Anti-Franking Check",
        Description = "Checks whether excess pension above GMP is sufficient to cover GMP increases.",
        WhyItMatters = "Prevents schemes from using GMP step-ups to reduce the total pension paid."
    )]
    public bool CheckAntiFranking(decimal totalPension, decimal gmpAmount, decimal gmpIncrease)
    {
        return (totalPension - gmpAmount) >= gmpIncrease;
    }

    [SpecCapability(
        Category = SpecCategories.GMP,
        Name = "Section 148 Revaluation",
        Description = "Applies s148 orders to revalue GMP between leaving and GMP pension age."
    )]
    public decimal ApplySection148(decimal gmp, decimal revaluationFactor)
    {
        return gmp * revaluationFactor;
    }

    // Not decorated — internal method, not relevant to spec
    public decimal Equalise(decimal malePension, decimal femalePension)
    {
        return Math.Max(malePension, femalePension);
    }
}
```

In the specbuilder, this will appear as:

```
GMP
• GMP Equalisation (Dual Record)
  GmpEqualiser
  Core · 2025-12-01
  ┗ Anti-Franking Check
  ┗ Section 148 Revaluation
```

---

## Part 2: Running the Extractor

### Prerequisites

- Python 3.6+
- No dependencies (stdlib only)

### Directory mode (recommended)

Point the extractor at a directory of `.cs` files. It walks the tree recursively.

```bash
cd spec-option-extractor

python3 extract_spec_options.py /path/to/csharp/modules/ \
  -o ../code-options.js \
  -c ../code-capabilities.js \
  -v
```

**How scheme is determined:** The first subdirectory under the input path becomes the scheme name. Files directly in the root have an empty scheme.

```
/path/to/csharp/modules/
  Core/
    GmpEqualiser.cs          → scheme: "Core"
    CpiCappedRevaluation.cs  → scheme: "Core"
  SchemeABC/
    CustomRule.cs            → scheme: "SchemeABC"
  SharedHelper.cs            → scheme: "" (empty — file in root)
```

**How lastModified is determined:** The file's OS modification time.

### JSON mode (backward compatibility)

If you have the modules as JSON (e.g. exported from a build pipeline):

```bash
python3 extract_spec_options.py sample-input.json \
  --json \
  -o ../code-options.js \
  -c ../code-capabilities.js \
  -v
```

JSON format:

```json
[
  {
    "moduleName": "GmpEqualiser",
    "scheme": "Core",
    "code": "using System;\n...[SpecOption(...)]...",
    "lastModified": "2025-12-01T16:45:00"
  }
]
```

### CLI flags

| Flag | Description |
|------|-------------|
| `input` | Directory of `.cs` files (default) or JSON file (with `--json`) |
| `--json` | Treat input as a JSON file |
| `-o`, `--output` | Output path for `code-options.js` (default: `./code-options.js`) |
| `-c`, `--capabilities-output` | Output path for `code-capabilities.js` (default: `./code-capabilities.js`) |
| `--preview` | Print a formatted summary of extracted options and capabilities (see below) |
| `--coverage` | Print a coverage report showing documented vs total public methods |
| `-v`, `--verbose` | Print diagnostic output showing every file scanned and every option found |

### Typical output

```
$ python3 extract_spec_options.py ./modules/ -o ../code-options.js -c ../code-capabilities.js --coverage -v

Scanning ./modules/ for .cs files...
  READ Core/CpiCappedRevaluation.cs (scheme=Core)
  READ Core/GmpEqualiser.cs (scheme=Core)
  READ Core/TrivialCommutation.cs (scheme=Core)
  READ Core/TransferCalculator.cs (scheme=Core)
  READ Core/InternalHelper.cs (scheme=Core)
Loaded 5 .cs files from ./modules/

--- Spec Options ---
  SKIP InternalHelper: no [SpecOption] attribute
  SKIP TransferCalculator: no [SpecOption] attribute
  FOUND CpiCappedRevaluation -> revaluation/cpi_capped_revaluation
  FOUND GmpEqualiser -> gmp/gmp_equaliser
  FOUND TrivialCommutation -> commutation/trivial_commutation

Extracted 3 spec options across 3 categories

--- Spec Capabilities ---
  FOUND capability CpiCappedRevaluation -> revaluation/calculate_pro_rata
  FOUND capability GmpEqualiser -> gmp/check_anti_franking
  FOUND capability GmpEqualiser -> gmp/apply_section148
  FOUND capability TransferCalculator -> transfers/calculate_partial_cetv
  FOUND capability TransferCalculator -> transfers/calculate_club_transfer

Extracted 5 spec capabilities across 3 categories

Coverage Report:
  CpiCappedRevaluation: 1/2 methods documented (50%)
  GmpEqualiser: 2/3 methods documented (67%)
  TrivialCommutation: 0/1 methods documented (0%)
  ---
  Overall: 3/6 methods documented (50%)
```

### Preview output

Use `--preview` to get a quick formatted summary of what was extracted, without opening the specbuilder or reading the JS files:

```
$ python3 extract_spec_options.py ./modules/ -o ../code-options.js -c ../code-capabilities.js --preview

Options (4):
  Commutation    Trivial Commutation (TrivialCommutation)
  GMP            GMP Equalisation (Dual Record) (GmpEqualiser)
  Revaluation    CPI-Capped (s101) (CpiCappedRevaluation), Fixed Rate (FixedRateRevaluation)

Capabilities (5):
  GMP            Anti-Franking Check, Section 148 Revaluation  -> parent: GmpEqualiser
  Revaluation    Pro-rata CPI Revaluation  -> parent: CpiCappedRevaluation
  Transfers      Partial CETV, Club Transfer Value  (standalone)
```

Options are grouped by category with their code class name in brackets. Capabilities show their parent option relationship or `(standalone)` if the method's class doesn't have `[SpecOption]`.

Combines with other flags — `--preview --coverage` gives both outputs.

### What gets generated

**`code-options.js`** — one entry per `[SpecOption]` class:

```javascript
const CODE_OPTIONS = {
    commutation: [
        {
            id: "trivial_commutation",
            name: "Trivial Commutation",
            description: "Full commutation of small pots below the trivial commutation limit.",
            whyItMatters: "Allows members with very small benefits to take a one-off lump sum.",
            codeClass: "TrivialCommutation",
            scheme: "Core",
            lastModified: "2025-09-20"
        }
    ],
    gmp: [ /* ... */ ],
    revaluation: [ /* ... */ ]
};
```

**`code-capabilities.js`** — one entry per `[SpecCapability]` method:

```javascript
const CODE_CAPABILITIES = {
    gmp: [
        {
            id: "check_anti_franking",
            name: "Anti-Franking Check",
            description: "Checks whether excess pension above GMP is sufficient...",
            whyItMatters: "Prevents schemes from using GMP step-ups...",
            methodName: "CheckAntiFranking",
            returnType: "bool",
            parameters: "decimal totalPension, decimal gmpAmount, decimal gmpIncrease",
            parentOption: { id: "gmp_equaliser", name: "GMP Equalisation (Dual Record)" },
            codeClass: "GmpEqualiser",
            scheme: "Core",
            lastModified: "2025-12-01"
        }
    ],
    revaluation: [ /* ... */ ],
    transfers: [ /* ... */ ]
};
```

Key points:
- IDs are auto-generated as snake_case from the class/method name
- `parentOption` is set automatically when a capability's class also has `[SpecOption]`
- Categories are sorted alphabetically
- `lastModified` is truncated to date only (no time)

### When to re-run

Re-run the extractor whenever:
- A new `[SpecOption]` or `[SpecCapability]` is added
- An existing attribute's text (Name, Description, WhyItMatters) changes
- A decorated class or method is renamed or deleted
- Code moves between scheme directories

The generated JS files should be committed alongside the specbuilder. They are the bridge between the C# codebase and the specbuilder UI.

---

## Part 3: The Specbuilder (Analyst)

### How the code inventory gets loaded

When `index.html` opens, it dynamically loads `code-options.js` and `code-capabilities.js` from the same directory. No `<script>` tags in the HTML — it uses a runtime loader:

```javascript
function loadCodeInventory() {
    return Promise.allSettled([
        loadScript('code-options.js'),
        loadScript('code-capabilities.js')
    ]).then(() => {
        updateCodeInventoryPanel();
    });
}
```

- If both files exist: the Code Inventory panel appears in the sidebar
- If one file is missing: the panel shows whatever data is available
- If both files are missing: the panel doesn't appear at all, no errors in the console
- The specbuilder works identically with or without the code files

### The Code Inventory panel

The panel appears below the "Your Order" card in the right sidebar:

```
┌─────────────────────────┐
│ Your Order              │
│ ...                     │
└─────────────────────────┘

┌─────────────────────────┐
│ ▸ Code Inventory    7   │  ← collapsed by default
└─────────────────────────┘
```

Click to expand:

```
┌─────────────────────────┐
│ ▾ Code Inventory    7   │
├─────────────────────────┤
│ COMMUTATION             │
│ • Trivial Commutation   │
│   TrivialCommutation    │
│   Core · 2025-09-20     │
│                         │
│ GMP                     │
│ • GMP Equalisation      │
│   GmpEqualiser          │
│   Core · 2025-12-01     │
│   ┗ Anti-Franking Check │
│   ┗ Section 148 Reval   │
│                         │
│ REVALUATION             │
│ • CPI-Capped (s101)     │
│   CpiCappedRevaluation  │
│   Core · 2025-11-15     │
│   ┗ Pro-rata CPI Reval  │
│ • Fixed Rate            │
│   FixedRateRevaluation  │
│   Core · 2025-10-01     │
│                         │
│ TRANSFERS               │
│ • Partial CETV          │
│   CalculatePartialCetv  │
│   Core · 2025-11-20     │
│ • Club Transfer Value   │
│   CalculateClubTransfer │
│   Core · 2025-11-20     │
└─────────────────────────┘
```

- Options are grouped by category with capabilities nested under their parent option
- Each item shows: display name, code class/method (monospace), scheme, last modified date
- Collapsed/expanded state is saved to localStorage
- The panel is read-only reference — no interaction with the spec form

### How to use the inventory while building a spec

The inventory panel is a reference. The analyst uses it to:

1. **Check what code already exists** before adding items to the spec
2. **See which categories have implementations** and which are gaps
3. **Note class names and method names** for the coder
4. **Compare scheme coverage** — is this capability in Core or scheme-specific?

There is no formal mapping between inventory items and spec items. The analyst uses their judgement to cross-reference.

### Code Inventory in exports

When exporting (Markdown, text, or PDF review), a Code Inventory section is appended at the bottom if code files were loaded.

**Markdown export** (section 11):

```markdown
## 11. Code Inventory

The following code options and capabilities were found in the codebase at time of export.

### Options

| Category | Name | Code Class | Scheme | Last Modified |
|----------|------|-----------|--------|---------------|
| Commutation | Trivial Commutation | TrivialCommutation | Core | 2025-09-20 |
| GMP | GMP Equalisation (Dual Record) | GmpEqualiser | Core | 2025-12-01 |
| Revaluation | CPI-Capped (s101) | CpiCappedRevaluation | Core | 2025-11-15 |
| Revaluation | Fixed Rate | FixedRateRevaluation | Core | 2025-10-01 |

### Capabilities

| Category | Name | Method | Parent Option | Scheme | Last Modified |
|----------|------|--------|---------------|--------|---------------|
| GMP | Anti-Franking Check | CheckAntiFranking | GMP Equalisation (Dual Record) | Core | 2025-12-01 |
| GMP | Section 148 Revaluation | ApplySection148 | GMP Equalisation (Dual Record) | Core | 2025-12-01 |
| Revaluation | Pro-rata CPI Revaluation | CalculateProRata | CPI-Capped (s101) | Core | 2025-11-15 |
| Transfers | Partial CETV | CalculatePartialCetv | | Core | 2025-11-20 |
| Transfers | Club Transfer Value | CalculateClubTransfer | | Core | 2025-11-20 |
```

**Text export:**

```
----------------------------------------
CODE INVENTORY
----------------------------------------
Code options and capabilities found in the codebase at time of export.

  Options:
  - [Commutation] Trivial Commutation (TrivialCommutation) — Core, 2025-09-20
  - [GMP] GMP Equalisation (GmpEqualiser) — Core, 2025-12-01
  ...

  Capabilities:
  - [GMP] Anti-Franking Check — CheckAntiFranking (parent: GMP Equalisation) — Core, 2025-12-01
  ...
```

**Review step (step 6):** The same inventory data appears at the bottom of the review sheet, visible on screen and in print.

If no code files were loaded, all three exports omit the section entirely — the output is identical to a specbuilder without code files.

---

## Part 4: File Layout

```
specbuilder/
├── index.html              ← Main specbuilder app
├── module-library.js       ← Spec dropdown options (static, hand-edited)
├── code-options.js         ← Generated by extractor (SpecOption data)
├── code-capabilities.js    ← Generated by extractor (SpecCapability data)
└── spec-option-extractor/
    ├── extract_spec_options.py       ← The extractor script
    ├── SpecOptionAttribute.cs        ← Copy into your C# project
    ├── SpecCapabilityAttribute.cs    ← Copy into your C# project
    ├── SpecCategories.cs             ← Copy into your C# project
    ├── sample-input.json             ← Example JSON input for testing
    └── test_extract_spec_options.py  ← Unit tests
```

### Which files are generated vs hand-edited

| File | Managed by | When to update |
|------|-----------|----------------|
| `index.html` | Developer | When adding specbuilder features |
| `module-library.js` | Analyst / developer | When adding new spec dropdown options |
| `code-options.js` | Extractor (generated) | Re-run extractor after C# changes |
| `code-capabilities.js` | Extractor (generated) | Re-run extractor after C# changes |
| `SpecOptionAttribute.cs` | Developer | Rarely — only if adding new attribute parameters |
| `SpecCapabilityAttribute.cs` | Developer | Rarely — only if adding new attribute parameters |
| `SpecCategories.cs` | Developer | When adding a new category |

---

## Part 5: Quick Reference

### Developer cheat sheet

```csharp
// Mark a class as a spec option
[SpecOption(
    Category = SpecCategories.GMP,           // Required
    Name = "GMP Equalisation (Dual Record)", // Required
    Description = "...",                     // Optional
    WhyItMatters = "..."                     // Optional
)]
public class GmpEqualiser : IGmpStrategy { }

// Mark a method as a spec capability
[SpecCapability(
    Category = SpecCategories.GMP,           // Required
    Name = "Anti-Franking Check",            // Required
    Description = "...",                     // Optional
    WhyItMatters = "..."                     // Optional
)]
public bool CheckAntiFranking(decimal totalPension, decimal gmpAmount, decimal gmpIncrease) { }
```

### Extractor cheat sheet

```bash
# Standard extraction from C# directory
python3 extract_spec_options.py /path/to/modules/ -o ../code-options.js -c ../code-capabilities.js

# Quick preview of what was extracted
python3 extract_spec_options.py /path/to/modules/ -o ../code-options.js -c ../code-capabilities.js --preview

# With verbose output and coverage report
python3 extract_spec_options.py /path/to/modules/ -o ../code-options.js -c ../code-capabilities.js --coverage -v

# From JSON (legacy/pipeline mode)
python3 extract_spec_options.py input.json --json -o ../code-options.js -c ../code-capabilities.js
```

### Adding a new category

1. Add a constant to `SpecCategories.cs`:
   ```csharp
   public const string DeathBenefits = "DeathBenefits";
   ```
2. Use it in your attributes:
   ```csharp
   [SpecOption(Category = SpecCategories.DeathBenefits, Name = "Lump Sum on Death", ...)]
   ```
3. Re-run the extractor — the new category appears automatically in the specbuilder.

No changes needed to `index.html`, `module-library.js`, or the extractor script.
