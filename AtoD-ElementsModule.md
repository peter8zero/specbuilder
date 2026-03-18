# Active to Deferred Elements Scheme Module Spec

| Field | Value |
|-------|-------|
| Scheme | |
| Author | |
| Date | |
| Version | 1.0 |
| Status | Draft / Review / Approved |

---

## How to use this template

This is a structured checklist for analysts to complete before coding begins. Each section maps directly to a coding decision in the `ActiveToDeferredElementsSchemeModule.cs` template.

**For analysts:** Fill in every table. If you do not know the answer, please insert "TBC" to confirm this has been read and will be updated.
**For coders:** Each section header shows the matching code reference in brackets. Ctrl+F the TODO tag in `ActiveToDeferredElementsSchemeModule.cs` to find where each decision is used.

---

## 1. Service Tranches `[ActiveToDeferredElementsSchemeModule.cs: FPSBasisForDeferredElements]`

List all service tranche keys that exist for this scheme and group them by which Final Pensionable Salary (FPS) applies to them.

**Note to Analysts:** Tranche keys must match those set up in Aurora under Client > Scheme > Admin > Categories > Service Tranches. If all tranches share the same FPS, there is only one FPS group (FPS1). Add more groups if different tranches use different salary definitions.

**Table 1A: FPS Groupings**

| FPS Group | Tranche Key(s) (per Aurora) | Notes |
|-----------|----------------------------|-------|
| FPS1 | e.g. PRE97, POST97, POST05 | e.g. All tranches use highest salary in last 3 years |
| FPS2 | | |
| FPS3 | | |

**Note to Analysts:** Do not include GMP deferred element keys here (e.g. PRE88GMP, POST88GMP). These are created separately — see Section 5.

*Note to Coders:*
*TODO: Populate `FPSBasisForDeferredElements` dictionary with tranche keys grouped by FPS label (e.g. "FPS1", "FPS2")*
*TODO: Add or remove FPS cases in the `switch` statement within `CalculateServiceTrancheDeferredElement` to match the number of FPS groups above*
*TODO: In `CalculateServiceTrancheDeferredElement` Part 1a, call the correct `SalarySchemeModule.GetFPS[x]` method for each FPS group*

---

## 2. TVIN Service `[ActiveToDeferredElementsSchemeModule.cs: Part 3 — TVIN Service]`

Transfer In (TVIN) service from external schemes is held on the member record in Aurora (Membership → External Benefits → Historic Benefit Elements) and needs to be added to the relevant service tranche before calculating deferred element values.

### 2.1 Does this scheme have TVIN service?

- [ ] Yes — complete the table below
- [ ] No — TVIN section can be left as template default (no impact on calculation)

### 2.2 TVIN Aurora Reference Keys

For each tranche that receives TVIN service, provide the Aurora field keys for the years component and the days (or months) component.

**Note to Analysts:** These keys must match the field names held in Aurora under Membership → External Benefits → Historic Benefit Elements.

**Table 2.2: TVIN Reference Keys**

| Tranche Key | TVIN Years Aurora Key | TVIN Days/Months Aurora Key | Unit (Days or Months) |
|-------------|----------------------|-----------------------------|----------------------|
| e.g. PRE97 | e.g. PRE97ADYRS | e.g. PRE97ADDAY | Days |
| e.g. POST97 | e.g. PST97ADYRS | e.g. PST97ADDAY | Days |
| | | | |
| | | | |

*Note to Coders:*
*TODO: For each tranche in the table above, add an `if (tranche.Key == "[KEY]")` block in Part 3*
*TODO: Update the Aurora key strings in `TVINService(member, "[KEY]")` calls to match column 2 and 3 above*
*TODO: If unit is Months (not Days), change `/365m` to `/12m` in the `totalTvinService` calculation and update the `GetPeriod` and `GetTVINPeriod` helper methods accordingly*
*TODO: Update the Info Log variables `TVINForMessage[TRANCHE]` in Part 5 to match any new tranches added*

---

## 3. Deferred Element Values `[ActiveToDeferredElementsSchemeModule.cs: Part 4 — Deferred Element Conversion]`

### 3.1 Spouse Pension Ratio

What proportion of the member's deferred element value is used as the spouse deferred element value?

- [ ] 50% (default)
- [ ] Other
    - If other, specify the ratio: ______________________

*Note to Coders:*
*TODO: Update `spouseDeferredElementValue = deferredElementValue / 2` in Part 4c if ratio is not 50% (e.g. `/3` for 33.3%, or `* 0.6666m` for 66.7%)*

### 3.2 Rounding

What rounding should be applied to the deferred element value and spouse value?

**Table 3.2: Rounding**

| Value | Rounding Required | Notes |
|-------|------------------|-------|
| Deferred element value | e.g. 2 decimal places (to the penny) | |
| Spouse deferred element value | e.g. 2 decimal places | |

*Note to Coders:*
*TODO: Update `.Round()` and `.Round(2)` calls in Part 6 (`ActiveToDeferredElementValue` return block) to match the rounding requirements above*

### 3.3 Non-Standard Adjustments to Tranche Values

After calculating `serviceTrancheValue = service × salary × accrual`, does any element need a further adjustment before being output as a deferred element value? (e.g. uplift to Normal Retirement Date, a minimum benefit, or a scheme-specific rule.)

- [ ] No — standard formula only
- [ ] Yes
    - If yes, describe the adjustment required per element:

**Table 3.3: Bespoke Adjustments**

| Tranche Key | Adjustment Required | Notes |
|-------------|-------------------|-------|
| | | |
| | | |

*Note to Coders:*
*TODO: If adjustments are required, add bespoke logic in Part 4b of `CalculateServiceTrancheDeferredElement` for the relevant tranche keys*

---

## 4. GMP Adjustment `[ActiveToDeferredElementsSchemeModule.cs: AdjustDeferredElementsForGMP]`

GMP values must be separated out from the PRE97 service tranche element into their own deferred elements (Pre88 GMP and Post88 GMP), as they have different revaluation rules at retirement.

### 4.1 PRE97 Element Key

What is the Aurora key for the Pre-1997 excess deferred element? (This is the element GMP is deducted from.)

- [ ] PRE97 (default)
- [ ] Other
    - If other, specify key: ______________________

*Note to Coders:*
*TODO: Update the two `if (tranche.Key == "PRE97")` checks in `AdjustDeferredElementsForGMP` to match the key above*

### 4.2 GMP Deferred Element Keys

What are the Aurora keys for the Pre88 and Post88 GMP deferred elements?

**Table 4.2: GMP Element Keys**

| GMP Type | Deferred Element Key (per Aurora) |
|----------|---------------------------------|
| Pre-1988 GMP | e.g. PRE88GMP |
| Post-1988 GMP | e.g. POST88GMP |

*Note to Coders:*
*TODO: Update `"PRE88GMP"` and `"POST88GMP"` strings in the `elementValues.Add(new ActiveToDeferredElementValue(...))` calls within `AdjustDeferredElementsForGMP`*

### 4.3 GMP Values Basis

GMP values are taken at Date of Retirement by default (`GmpValues.Pre88GmpAtDateOfRetirement`, `GmpValues.Post88GmpAtDateOfRetirement`).

- [ ] Date of Retirement (default)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: If not at Date of Retirement, update the `pre88deduction` and `post88deduction` variable assignments in `AdjustDeferredElementsForGMP`*

### 4.4 GMP Spouse Values

Spouse GMP deferred element value defaults to 50% of the GMP value.

- [ ] 50% (default)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Update `gmpSpouseDeduction = gmpDeduction / 2` and the SpouseValue in `elementValues.Add(new ActiveToDeferredElementValue("PRE88GMP", pre88deduction, pre88deduction / 2, false))` if not 50%*

---

## 5. Deferred Element Display Order `[ActiveToDeferredElementsSchemeModule.cs: elementDisplayOrder]`

In what order should deferred element results be displayed in the Aurora calculation output?

**Note to Analysts:** List all deferred element keys in the order you want them to appear, including GMP elements.

**Table 5: Display Order**

| Priority | Deferred Element Key (per Aurora) |
|----------|----------------------------------|
| 1 | e.g. PRE97 |
| 2 | e.g. POST97 |
| 3 | e.g. POST05 |
| 4 | e.g. PRE88GMP |
| 5 | e.g. POST88GMP |
| 6 | |
| 7 | |
| 8 | |

*Note to Coders:*
*TODO: Populate `elementDisplayOrder()` method with the keys above in priority order*

---

## 6. Dependencies & Setup Checklist `[ActiveToDeferredElementsSchemeModule.cs: using statements, class name]`

### 6.1 Coder Checklist

*Note to Coders: Perform this checklist*

**Table 6.1**

| # | Location | TODO | Status | Notes |
|---|-----------|------|--------|-------|
| 1 | ActiveToDeferredElementsSchemeModule | Rename class to match module name in Aurora (e.g. `[SchemeName]ActiveToDeferredElementsSchemeModule`) | | |
| 2 | ActiveToDeferredElementsSchemeModule | Set `SalarySchemeModule` dependency (required to call `GetFPS[x]` methods) | | |
| 3 | ActiveToDeferredElementsSchemeModule | Set global `ServiceBuilder` dependency (allows `ServiceHistoryOverTrancheWithDuration` type to be used) | | |
| 4 | ActiveToDeferredElementsSchemeModule | Populate `FPSBasisForDeferredElements` per Section 1 | | |
| 5 | ActiveToDeferredElementsSchemeModule | Add/remove FPS values and `switch` cases per Section 1 | | |
| 6 | ActiveToDeferredElementsSchemeModule | Add/remove TVIN blocks per Section 2 | | |
| 7 | ActiveToDeferredElementsSchemeModule | Update spouse ratio and rounding per Section 3 | | |
| 8 | ActiveToDeferredElementsSchemeModule | Update PRE97 element key and GMP element keys per Section 4 | | |
| 9 | ActiveToDeferredElementsSchemeModule | Populate `elementDisplayOrder()` per Section 5 | | |
| 10 | ActiveToDeferredElementsSchemeModule | Publish module and assign as dependency in main A>D (and A>R) scheme calc | | |

---

## 7. Additional Misc Notes

Capture anything not covered above — scheme quirks, edge cases, or anything the coder should watch out for in this module.

>

---

## 8. Sign-Off

**Table 8**

| Role | Name | Date |
|------|------|------|
| Analyst | | |
| Coder | | |
| Reviewer | | |
