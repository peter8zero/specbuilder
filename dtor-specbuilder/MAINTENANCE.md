# DtoR Spec Builder — Maintenance Guide

This guide explains how to update the spec builder when the underlying DtoR markdown spec template changes.

---

## The Two-File Architecture

| File | What it is | When to edit it |
|------|-----------|----------------|
| `DtoR_template.js` | The markdown output — the spec document with `{{placeholders}}` instead of answers | Wording changes, adding/removing sections, restructuring prose |
| `index.html` | The form UI and all app logic | Adding/removing form fields, changing radio options, adding new dynamic table types |

**The golden rule:** Every `{{placeholder}}` in `DtoR_template.js` must have a matching entry in `buildData()` in `index.html`, and vice versa.

---

## Change Type 1: Wording / Prose Changes Only

**Examples:** Changing a section description, fixing a typo in a coder note, rewording a hint to analysts.

**Only edit:** `DtoR_template.js`

Find the relevant text and change it. The `{{placeholders}}` and `{{#each}}` blocks are untouched.

**Example — changing a coder TODO note:**
```
Before: *TODO: Populate Validation 1 with Category Keys*
After:  *TODO: Populate Validation 1 with valid Category Keys from Aurora*
```

**Test:** Open `index.html`, click Generate, check the output section looks right.

---

## Change Type 2: Adding a New Simple Text Field

A simple text field is one where the analyst types a free-text answer.

### Step 1 — Add the placeholder to `DtoR_template.js`

Find the right place in the template and add `{{yourFieldName}}`:

```markdown
| Scheme Pension Age | {{schemePensionAge}} |
```

### Step 2 — Add the form input to `index.html`

Find the relevant section in the HTML form and add an input:

```html
<div class="form-group">
  <label class="form-label">Scheme Pension Age</label>
  <input class="form-input" oninput="state.schemePensionAge=this.value;autoSave()">
</div>
```

### Step 3 — Add the field to `state` in `index.html`

In the `let state = { ... }` block, add a default value:

```js
schemePensionAge: '',
```

### Step 4 — Add the field to `buildData()` in `index.html`

In the `return { ... }` block of `buildData()`, add:

```js
schemePensionAge: mdCell(s.schemePensionAge),
```

Use `mdCell()` for any field that goes inside a markdown table cell (it escapes pipe characters). For fields outside tables, you can use the value directly.

### Step 5 — Handle load from saved state (optional but recommended)

If analysts will save and reload drafts, add the field to `rebuildFormFromState()`. Find a similar field in that function and follow the same pattern — for a simple text input you'd add:

```js
document.querySelector('#your-input-id').value = state.schemePensionAge;
```

(Add an `id` to your input element to make this easier.)

**Test:** Fill in the field, click Generate, confirm the value appears in the right place in the output.

---

## Change Type 3: Adding a New Radio / Checkbox Field

Radio groups are used for "choose one" questions. The form shows radio buttons; the generated markdown shows the checked/unchecked options as `[x]` / `[ ]` lists.

### Step 1 — Add the placeholder to `DtoR_template.js`

```markdown
**Revaluation end date basis:**

{{revalEndDateBasis}}
```

### Step 2 — Add the radio group to the form in `index.html`

```html
<div class="form-group">
  <label class="form-label">Revaluation end date basis</label>
  <div class="choice-group">
    <label class="choice-opt">
      <input type="radio" name="revalEndDateBasis" value="npa"
             onchange="state.revalEndDateBasis=this.value;toggleOther('revalEndDateOther',false);autoSave()" checked>
      <div><div class="choice-label">Date member reaches NPA</div></div>
    </label>
    <label class="choice-opt">
      <input type="radio" name="revalEndDateBasis" value="date_of_leaving"
             onchange="state.revalEndDateBasis=this.value;toggleOther('revalEndDateOther',false);autoSave()">
      <div><div class="choice-label">Date of Leaving</div></div>
    </label>
    <label class="choice-opt">
      <input type="radio" name="revalEndDateBasis" value="other"
             onchange="state.revalEndDateBasis=this.value;toggleOther('revalEndDateOther',true);autoSave()">
      <div><div class="choice-label">Other</div></div>
    </label>
  </div>
  <div class="other-input reveal" id="revalEndDateOther">
    <input class="form-input" placeholder="Please specify..."
           oninput="state.revalEndDateBasisOther=this.value;autoSave()">
  </div>
</div>
```

Notes:
- `name="..."` must be unique across the whole form
- `value="..."` on each radio is the internal state value (use lowercase with underscores)
- `toggleOther('id', true/false)` shows/hides the "other" text box
- If there is no "Other" option, remove the `toggleOther` calls and the `.other-input` div

### Step 3 — Add to `state`

```js
revalEndDateBasis: 'npa',      // default to the most common option
revalEndDateBasisOther: '',
```

### Step 4 — Add a render function to `buildData()`

Use the existing `radioBlock()` helper. Add this inside `buildData()`, before the `return`:

```js
const revalEndDateBasis = radioBlock(s.revalEndDateBasis, [
  { val: 'npa',           label: 'Date member reaches NPA' },
  { val: 'date_of_leaving', label: 'Date of Leaving' },
  { val: 'other',         label: 'Other' }
], 'revalEndDateBasisOther');  // <-- the state key for the "other" text, or omit if no Other option
```

Then add it to the `return`:

```js
revalEndDateBasis,
```

### Step 5 — Add to the radio rebuild in `rebuildFormFromState()`

In the `map` object inside `rebuildFormFromState()`, add an entry:

```js
revalEndDateBasis: 'revalEndDateBasis',
```

The format is `radioGroupName: stateKey`.

**Test:** Try each radio option, click Generate, confirm the `[x]` appears on the right option in the output.

---

## Change Type 4: Adding a New Dynamic Table Section (per-category)

Dynamic tables are the "add category table" sections used in sections 2, 4, 5 and 6. Each table has a category label and add/remove rows.

### Step 1 — Add the placeholder block to `DtoR_template.js`

```markdown
### 4.4 Pension Increase Factors — per category

{{#each piTables}}
**Table 4.4{{letter}}**
- [ ] Insert Category or Categories: {{categories}}

| Deferred Element Key | PI Table Male | PI Table Female | Notes |
|---------------------|---------------|-----------------|-------|
{{#each rows}}| {{elementKey}} | {{piTableMale}} | {{piTableFemale}} | {{notes}} |
{{/each}}

{{/each}}
```

Notes:
- `{{letter}}` is auto-generated (A, B, C...) — always include it
- `{{categories}}` is the label the analyst types for that table
- Inside `{{#each rows}}`, the keys must match the row definition in the next step

### Step 2 — Define the columns and default row in `index.html`

Add near the other `ColDef` / `RowDef` blocks (around line 800):

```js
const piColDef = [
  { k: 'elementKey',   label: 'Deferred Element Key', w: 160 },
  { k: 'piTableMale',  label: 'PI Table Male',         w: 120 },
  { k: 'piTableFemale',label: 'PI Table Female',        w: 120 },
  { k: 'notes',        label: 'Notes',                  w: 150 }
];
const piRowDef = () => ({ id: uid(), elementKey: '', piTableMale: '', piTableFemale: '', notes: '' });
```

For a dropdown column, add `type: 'select'` and `opts: ['Option A', 'Option B']` to the column definition.

### Step 3 — Add the container div and button to the form HTML

In the appropriate section in `index.html`:

```html
<div class="subsection-title">4.4 Pension Increase Factors — per category</div>
<div id="pi-blocks"></div>
<button class="btn btn-ghost btn-sm" onclick="addCatBlock('piBlocks','pi-blocks',piRowDef,piColDef)">+ Add category table</button>
```

### Step 4 — Add to `state`

```js
piBlocks: [],
```

### Step 5 — Add to `buildData()`

Inside `buildData()`, map the blocks:

```js
piTables: buildCatBlocks(s.piBlocks, r => ({
  elementKey:    mdCell(r.elementKey),
  piTableMale:   mdCell(r.piTableMale),
  piTableFemale: mdCell(r.piTableFemale),
  notes:         mdCell(r.notes)
})),
```

Then add `piTables` to the `return`.

### Step 6 — Add to `rebuildFormFromState()`

At the bottom of `rebuildFormFromState()` where the other `renderCatBlocks` calls are:

```js
renderCatBlocks('piBlocks', 'pi-blocks', piRowDef, piColDef);
```

**Test:** Add a category table, add a row, fill in values, click Generate, confirm the table appears correctly in the output.

---

## Change Type 5: Adding a New Simple Repeating Row Table

These are tables where rows are added/removed but there are no category groups (e.g. the AFR table, upload confirmation tables).

### Step 1 — Add the placeholder to `DtoR_template.js`

```markdown
| PI Increase Rate | Applicable From | Notes |
|-----------------|-----------------|-------|
{{#each piRates}}| {{rate}} | {{from}} | {{notes}} |
{{/each}}
```

### Step 2 — Add a row in the HTML with an add function call

Add a tbody and button in the right section:

```html
<div class="dyn-table-wrap">
  <table class="dyn-table">
    <thead><tr><th>PI Increase Rate</th><th>Applicable From</th><th>Notes</th><th></th></tr></thead>
    <tbody id="pirates-tbody"></tbody>
  </table>
</div>
<button class="btn btn-ghost btn-sm add-row-btn" onclick="addPiRateRow()">+ Add row</button>
```

### Step 3 — Add a row function to `index.html`

Follow the pattern of `addAfrRow()`:

```js
function addPiRateRow() {
  const id = uid();
  state.piRates.push({ id, rate: '', from: '', notes: '' });
  const tb = document.getElementById('pirates-tbody');
  const tr = document.createElement('tr');
  tr.id = 'pir-' + id;
  tr.innerHTML = `
    <td><input value="" oninput="setSimpleRow('piRates','${id}','rate',this.value)"></td>
    <td><input value="" oninput="setSimpleRow('piRates','${id}','from',this.value)"></td>
    <td><input value="" oninput="setSimpleRow('piRates','${id}','notes',this.value)"></td>
    <td><button class="del-btn" onclick="delSimpleRow('piRates','${id}','pir-${id}')">✕</button></td>`;
  tb.appendChild(tr);
  autoSave();
}
```

### Step 4 — Add to `state` and `buildData()`

```js
// state
piRates: [],

// buildData() return
piRates: s.piRates.filter(r => r.rate || r.from)
  .map(r => ({ rate: mdCell(r.rate), from: mdCell(r.from), notes: mdCell(r.notes) })),
```

---

## Change Type 6: Removing a Field

1. Remove the `{{placeholder}}` from `DtoR_template.js`
2. Remove the form input from `index.html`
3. Remove the entry from `state`
4. Remove the entry from `buildData()`
5. Remove from `rebuildFormFromState()` if present

---

## Change Type 7: Adding a New Section

Sections are just groupings of the above change types. Steps:

1. Add the section block to `DtoR_template.js` (new `## N. Section Title` heading)
2. Add a `<div class="spec-section" id="sN">` block in the HTML with the form fields
3. Add a nav link in the `<nav id="sidebar">`:
   ```html
   <a class="nav-item" href="#sN" onclick="setActive(this)"><span class="nav-num">N</span> Section Name</a>
   ```
4. Add fields to `state` and `buildData()` per the change types above

---

## Quick Reference: Template Placeholder Syntax

| Syntax | Meaning |
|--------|---------|
| `{{fieldName}}` | Insert the value of `fieldName` from `buildData()` |
| `{{#each arrayName}}...{{/each}}` | Repeat the block for each item in `arrayName` |
| Inside an `#each` block: `{{key}}` | Refers to a property of the current array item |
| Nested `{{#each rows}}` inside an outer `{{#each}}` | One level of nesting supported (e.g. rows inside a category table) |
| `` \` `` | Escaped backtick — needed because the template is a JS template literal |

---

## Quick Reference: `buildData()` Helpers

| Helper | Use for |
|--------|---------|
| `mdCell(value)` | Any value going inside a markdown table cell — escapes `\|` characters |
| `radioBlock(stateValue, options, otherKey)` | Renders a radio group as `[x]` / `[ ]` markdown lines |
| `buildCatBlocks(blocks, rowMapper)` | Maps a category block array to the format expected by `{{#each}}` |
| `chk(bool)` | Returns `- [x]` or `- [ ]` for a single checkbox |

---

## Testing After Any Change

1. Open `dtor-specbuilder/index.html` directly in a browser (no server needed)
2. Fill in the new/changed field(s)
3. Click **Generate ▶**
4. Check the relevant section in the output matches the expected markdown
5. Verify the rest of the spec is unaffected

If the placeholder appears literally (e.g. `{{myField}}` in the output), you have either:
- Forgotten to add it to `buildData()`
- Misspelled the key (template key and `buildData()` key must match exactly)
