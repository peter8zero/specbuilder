# Pension Calc Specbuilder

A guided configurator for building pension calculation specs. Non-technical users walk through a stepped form to describe what a calculation needs, then export a filled-in spec document (Markdown, text, or PDF) that the dev team can use to build the calc.

**This is a living tool** — it's not in its final form. The whole point is that anyone on the team can tweak fields, options, steps, and wording without needing to rebuild anything. This README explains how.

---

## Quick Start

1. Open `index.html` in a browser. No server, no build step, no dependencies.
2. Walk through the steps, filling in the scheme details.
3. Hit **Export Markdown** to get a `.md` spec file ready for Azure DevOps.

---

## File Structure

```
specbuilder/
  index.html              ← The entire app (HTML + CSS + JS in one file)
  module-library.js       ← Dropdown options and selectable data (edit separately)
  code-options.js         ← Generated — spec options extracted from C# code
  code-capabilities.js    ← Generated — spec capabilities extracted from C# code
  spec-option-extractor/  ← Python tool that generates the code-*.js files
  README.md               ← You are here
```

| File | What it controls | Who should edit it |
|------|-----------------|-------------------|
| `module-library.js` | The **options** in dropdowns and card selections (calc types, benefit types, revaluation methods, GMP handling options, validations, dependencies) | Anyone — it's just a list of options with names and descriptions |
| `index.html` | The **form structure** — which fields appear, what order, how they're laid out, and the export format | Anyone comfortable with HTML — all patterns are copy-paste |
| `code-options.js` | **Generated** — C# classes decorated with `[SpecOption]` | Don't edit by hand — re-run the extractor |
| `code-capabilities.js` | **Generated** — C# methods decorated with `[SpecCapability]` | Don't edit by hand — re-run the extractor |

---

## How the App Works

### The 7 Steps

| Step | Name | What it captures |
|------|------|-----------------|
| 0 | Scheme Info | Scheme name, requester, date, version, status, valid categories |
| 1 | Calc Type | Which calculation type (Def>Ret, Act>Ret, Transfer Out, etc.) |
| 2 | Service Tranches | Per-tranche config: benefit type, accrual, revaluation, factors, GMP, element keys |
| 3 | GMP & Special Rules | GMP settings, handling options (skipped if no tranche has GMP) |
| 4 | Commutation / PCLS | Commutation style, spouse %, factor keys (skipped for transfer/death calcs) |
| 5 | Validations & Deps | Pre/post-calc checks, setup dependencies, free-text notes |
| 6 | Review & Export | Full review, sign-off fields, export buttons |

### Smart Behaviour

- **Step 3 (GMP)** is automatically skipped if no tranche has "Includes GMP" ticked
- **Step 4 (Commutation)** is automatically skipped for Transfer Out, Death Before Ret, Death After Ret calc types
- The **order panel** (sidebar) updates live as you fill things in
- **Save/Load** uses your browser's localStorage — specs persist between sessions

### State Model

Everything the user enters is stored in a single `state` object in JavaScript. When you add a new field, you add it to this object. The state is what gets saved, loaded, and exported.

---

## Editing `module-library.js` (Dropdown Options)

This file holds a single `MODULE_LIBRARY` object. Each section is an array of items that power the dropdowns and selection cards in the app.

### Adding a new calc type

Find the `calcTypes` array and add a new entry:

```javascript
calcTypes: [
    // ... existing types ...
    {
        id: 'my_new_calc',           // Unique ID (used in code, never shown to users)
        name: 'My New Calculation',   // Display name
        shortName: 'New Calc',        // Short label
        description: 'What this calculation does, in plain English.',
        whyItMatters: 'Why a scheme might need this.',
        icon: 'some-icon'            // Not currently used, but reserved
    }
]
```

### Adding a new benefit type / accrual rate / revaluation method

Same pattern — find the relevant array and add an entry with `id`, `name`, `description`:

```javascript
benefitTypes: [
    // ... existing ...
    { id: 'hybrid', name: 'Hybrid (FS + CARE)', description: 'A combination of final salary and CARE accrual.' }
]
```

### Adding a new GMP handling option

GMP options can have sub-options (radio buttons). Here's the pattern:

```javascript
gmpHandling: [
    // ... existing ...
    {
        id: 'my_gmp_thing',
        name: 'My GMP Rule',
        description: 'What this rule does.',
        whyItMatters: 'Why the coder needs to know.',
        complexity: 'medium',    // 'low', 'medium', or 'high' — shows a coloured badge
        // Optional: add sub-options as radio buttons
        options: [
            { id: 'option_a', name: 'Option A', description: 'What A means.' },
            { id: 'option_b', name: 'Option B', description: 'What B means.' }
        ]
    }
]
```

### Adding a new validation or dependency

```javascript
validations: {
    preCalc: [
        // ... existing ...
        {
            id: 'my_check',
            name: 'My Custom Check',
            description: 'What this check does.',
            whyItMatters: 'Why it matters.',
            isStandard: false    // true = shows a green "Standard" badge
        }
    ]
}
```

### Things to know about module-library.js

- **IDs must be unique** within their array. Use snake_case.
- **Names and descriptions** are shown directly to users — write in plain English.
- The `whyItMatters` field appears in italics below the description on selection cards.
- Changes take effect immediately on refresh — no build step.

---

## Editing `index.html` (Form Structure)

The HTML file contains everything: styles (in `<style>`), structure (HTML), and logic (in `<script>`). It's deliberately kept in one file so there's nothing to wire up.

### Code Inventory (sidebar panel)

If `code-options.js` and `code-capabilities.js` exist in the same directory as `index.html`, the specbuilder shows a **Code Inventory** panel in the sidebar below "Your Order". This is a read-only reference showing what calculation code already exists in the codebase.

- **Collapsed by default** — click to expand, state saved to localStorage
- **Grouped by category** (GMP, Revaluation, Commutation, Transfers, etc.)
- **Options** show: display name, code class name, scheme, last modified date
- **Capabilities** are nested under their parent option
- **Included in exports** — Markdown, text, and review all get a Code Inventory section at the bottom
- **Graceful when missing** — if the files don't exist, the panel doesn't appear, no console errors, the app works identically

These files are generated by the extractor tool in `spec-option-extractor/`. See [README_spec-option-extractor.md](README_spec-option-extractor.md) for the full end-to-end process.

### Architecture Overview

```
<style>        ← All CSS (theming, layout, responsive, print)
<body>         ← Static HTML (header, container, modal, print header)
<script>       ← All JavaScript:
  STEPS[]         - Step definitions (names, labels)
  newTranche()    - Default values for a new tranche
  state {}        - All user data (the single source of truth)
  Theme           - Dark/light toggle
  Code Inventory  - Dynamic loader, inventory panel, export sections
  Step Engine     - Navigation, skip logic, completion checks
  Render Steps    - renderStep0() through renderStep6()
  Order Panel     - Sidebar live summary
  Export          - exportMarkdown(), exportText(), exportPDF()
  Save/Load       - localStorage
  Utils           - Toast, escape, slug, download helpers
```

### Adding a New Field to an Existing Step

This is the most common change. Here's the complete checklist:

**Example: Adding a "Scheme Reference Number" field to Step 0.**

#### 1. Add the state key

Find the `state` object (search for `let state = {`) and add your field:

```javascript
let state = {
    // ... existing fields ...
    schemeRefNumber: '',    // ← Add here
};
```

#### 2. Add the form field to the render function

Find `renderStep0` (search for `function renderStep0`) and add a form group. Copy an existing one as your template:

```html
<div class="form-group">
    <label class="form-label">Scheme Reference Number</label>
    <input class="form-input" value="${esc(state.schemeRefNumber)}"
           placeholder="e.g. SCH-001"
           oninput="state.schemeRefNumber=this.value; updateOrderPanel()">
    <div class="form-hint">Optional hint text shown below the field.</div>
</div>
```

**Field types you can use:**

| Type | HTML |
|------|------|
| Text input | `<input class="form-input" value="${esc(state.myField)}" oninput="state.myField=this.value; updateOrderPanel()">` |
| Date input | `<input class="form-input" type="date" value="${state.myDate}" oninput="state.myDate=this.value; updateOrderPanel()">` |
| Number input | `<input class="form-input" type="number" min="0" max="100" value="${state.myNum}" oninput="state.myNum=this.value; updateOrderPanel()">` |
| Dropdown | `<select class="form-select" onchange="state.myField=this.value; updateOrderPanel()"><option value="a" ${state.myField==='a'?'selected':''}>Option A</option></select>` |
| Textarea | `<textarea class="form-textarea" oninput="state.myField=this.value; updateOrderPanel()">${esc(state.myField)}</textarea>` |
| Two fields side-by-side | Wrap two `form-group` divs in `<div class="tranche-row">...</div>` |

#### 3. Add to the review

Find `buildReviewHTML()` and add a review line in the appropriate section:

```javascript
html += `<div class="review-item">
    <span class="review-item-label">Scheme Ref</span>
    <span class="review-item-value">${esc(state.schemeRefNumber) || 'Not set'}</span>
</div>`;
```

#### 4. Add to exports

**Markdown export** — find `exportMarkdown()` and add a row to the header table:
```javascript
md += `| Scheme Ref | ${state.schemeRefNumber || 'TBC'} |\n`;
```

**Text export** — find `exportText()` and add a line:
```javascript
text += `Scheme Ref: ${state.schemeRefNumber || 'Not set'}\n`;
```

**PDF export** — find `exportPDF()` and add:
```javascript
txt(`Scheme Ref: ${state.schemeRefNumber || 'Not set'}`, 10);
```

#### 5. Add to resetAll()

Find `resetAll()` and add your field to the reset state object:
```javascript
schemeRefNumber: '',
```

**That's it. Five places to touch for any new field.**

---

### Adding a New Field to a Tranche

Tranches are per-service-period configs. Each tranche is an object created by `newTranche()`.

#### 1. Add to `newTranche()`

```javascript
function newTranche() {
    return {
        // ... existing ...
        myNewField: '',    // ← Add here with a sensible default
    };
}
```

#### 2. Add to `renderStep2()`

Inside the tranche card template (search for `tranche-body`), add your field. Use `updateTranche(${idx},'myNewField',this.value)` for the oninput:

```html
<div class="form-group">
    <label class="form-label">My New Field</label>
    <input class="form-input" value="${esc(t.myNewField)}"
           placeholder="e.g. something"
           oninput="updateTranche(${idx},'myNewField',this.value)">
</div>
```

#### 3. Add to review, markdown export, text export, PDF export

Same pattern as above, but you'll find the tranche loop in each function (`state.tranches.forEach`).

---

### Adding a New Step

This is a bigger change but still follows a pattern.

#### 1. Add to the STEPS array

```javascript
const STEPS = [
    // ... existing steps 0-5 ...
    { id: 6, name: 'My New Step', short: 'New' },    // ← Insert before Review
    { id: 7, name: 'Review & Export', short: 'Review' }  // ← Bump Review to 7
];
```

#### 2. Create the render function

```javascript
function renderStep6(el) {
    let html = `
        <div class="step-title">My New Step</div>
        <div class="step-subtitle">What does the user do here?</div>
        <!-- Your form fields here -->
        ${stepNav(6)}`;
    el.innerHTML = html;
}
```

#### 3. Add to the render switch

In `renderStep()`, add `case 6: renderStep6(el); break;` and bump the old step 6 to 7.

#### 4. Update step completion check

In `isStepCompleted()`, add a case for your new step number.

#### 5. Update references

- The Review step number changes (was 6, now 7) — update `goToStep(6)` references in the sidebar and anywhere that hardcodes the review step number
- Search for `goToStep(6)` and `=== 6` to find all references

---

### Making a Step Conditionally Skippable

Find `isStepSkipped()` and add a condition:

```javascript
function isStepSkipped(i) {
    if (i === 3 && !hasGMP()) return true;           // Skip GMP if no GMP
    if (i === 4 && ct && noCommCalcs.includes(ct.id)) return true;  // Skip commutation
    if (i === 6 && someCondition()) return true;      // ← Your new skip rule
    return false;
}
```

The step engine automatically handles skipping in both forward and backward navigation.

---

### Changing the Markdown Export Template

The markdown export (`exportMarkdown()`) generates a filled-in spec that mirrors the DtoR template structure. Each section is clearly labelled with comments. To change what the exported markdown looks like:

1. Find `function exportMarkdown()`
2. Each section starts with `md += '## X. Section Name\n\n'`
3. Tables use standard markdown: `| Header | Header |\n|--------|--------|\n| Value | Value |`
4. Edit the structure, add/remove rows, change headings

The exported `.md` file is designed to be committed to Azure DevOps and edited directly in the repo from that point on.

---

## Theming

The app uses CSS custom properties for theming. To change colours, edit the `:root` block at the top of the `<style>` section:

```css
:root {
    --xps-blue: #4A9FD9;        /* Primary accent */
    --xps-success: #4CAF50;     /* Completed steps, success messages */
    --xps-warning: #FF9800;     /* Post-calc warnings */
    --xps-danger: #F44336;      /* Errors, delete buttons */
    --xps-purple: #9C27B0;      /* GMP tags, markdown export button */
    /* ... more variables ... */
}
```

Light theme overrides are in `[data-theme="light"] { ... }`.

---

## Export Formats

| Format | Button | What you get |
|--------|--------|-------------|
| **Markdown** | Purple "Export Markdown" | A `.md` file structured like the DtoR spec template. **This is the primary output** — commit it to Azure DevOps and edit from there. |
| **Text** | "Export Text" | A plain `.txt` file with the same content, for quick sharing. |
| **PDF** | "Export PDF" | A formatted PDF using jsPDF (loaded from CDN). Good for printing or emailing. |
| **Print** | "Print" | Opens the browser print dialog. CSS hides the header, sidebar, and nav for clean output. |

---

## Save & Load

- **Save** stores the current state to `localStorage` (browser-only, per device)
- **Load** shows a list of previously saved specs
- **New** resets everything to defaults

Saved specs persist across browser sessions but are **not** synced between devices or users. For team sharing, use the Markdown export and commit to the repo.

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| Changes not showing after editing | Hard refresh: Cmd+Shift+R (Mac) or Ctrl+Shift+R (Windows) |
| Date fields look weird on mobile | The app uses native date pickers — appearance varies by browser/OS |
| PDF export says "library not loaded" | Check internet connection (jsPDF loads from CDN) |
| Saved specs disappeared | localStorage was cleared (browser settings, private browsing, or different browser) |
| A dropdown is missing an option | Add it to the relevant array in `module-library.js` |
| A field isn't appearing in the export | Check you added it to `exportMarkdown()`, `exportText()`, and `exportPDF()` |

---

## Summary: Where to Make Common Changes

| I want to... | Edit this file | Search for |
|--------------|---------------|------------|
| Add/remove a dropdown option | `module-library.js` | The array name (e.g. `calcTypes`, `benefitTypes`) |
| Change option descriptions | `module-library.js` | The option's `id` |
| Add a new field to a step | `index.html` | `renderStepX` (where X is the step number) |
| Add a new field to tranches | `index.html` | `newTranche()` and `renderStep2` |
| Change what the markdown export looks like | `index.html` | `exportMarkdown` |
| Change the review page layout | `index.html` | `buildReviewHTML` |
| Change colours or styling | `index.html` | `:root` (CSS variables) |
| Add a new step | `index.html` | `STEPS`, `renderStep`, `isStepCompleted` |
| Change which steps are skipped | `index.html` | `isStepSkipped` |
| Change the sidebar summary | `index.html` | `updateOrderPanel` |
| Update the code inventory | Run the extractor | See [README_spec-option-extractor.md](README_spec-option-extractor.md) |
| Mark a C# class as a spec option | Your `.cs` file | Add `[SpecOption(...)]` — see [README_spec-option-extractor.md](README_spec-option-extractor.md) |

---

## What's Next

This is v1. Things we'll probably want to refine:

- [ ] Field names and descriptions — do they make sense to analysts?
- [ ] Are we missing any fields that coders need?
- [ ] Should any fields have different defaults?
- [ ] Are the GMP options comprehensive enough?
- [ ] Does the markdown export match what we want in the repo?
- [ ] Do we need more calc types?

Raise a PR or just edit the files directly. The whole point is that this evolves with us.
