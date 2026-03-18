# Salary Scheme Module Spec

| Field | Value |
|-------|-------|
| Scheme | |
| Author | |
| Date | |
| Version | 1.0 |
| Status | Draft / Review / Approved |

---

## How to use this template

This spec covers the `SalarySchemeModule`, which defines every Final Pensionable Salary (FPS) used by the scheme. Each FPS type is returned as a `FinalPensionableSalary` object and consumed by the `ActiveToDeferredElementsSchemeModule` (for element value calculations) and by the main A>D and A>R scheme calcs (for result output).

**For analysts:** Complete Section 1 to confirm how many FPS types exist, then complete one section per FPS type. If the FPS involves a Best-of-N-out-of-Y look-back that requires bespoke candidate or pro-rate logic, also complete `FPP-Modules.md`.
**For coders:** Each section header shows the matching method in `salary-scheme-module.cs`. Ctrl+F the TODO tags in that file to find where each decision is applied.

---

## 1. FPS Overview `[salary-scheme-module.cs: SalarySchemeModule class]`

### 1.1 How many FPS types does this scheme have?

Each distinct salary definition used in the calculation needs its own `GetFPS[x]` method. Common reasons for multiple FPS types: different categories use different salary definitions, some elements use a different look-back period, or a manually quoted salary is also required alongside the calculated one.

| FPS Method | Required? | Brief Description |
|------------|-----------|------------------|
| GetFPS1 | Yes / No | |
| GetFPS2 | Yes / No | |
| GetFPS3 | Yes / No | |
| GetFPS4 | Yes / No | |

*Note to Coders:*
*TODO: Add or remove `GetFPS[x]` methods in `SalarySchemeModule` to match the number of FPS types above*
*TODO: Ensure the number of FPS methods here matches the `FinalPensionableSalary` list in the A>D/A>R `Calculate()` return blocks (see `AtoD.md` Section 4.2 and `AtoR.md` Section 4.2)*

### 1.2 FPS Calc Date

The `FPSCalcDate` parameter is passed into each `GetFPS[x]` method from the main scheme calc. It is the date from which the salary look-back period is measured.

**Note to Analysts:** This is set in the main A>D and A>R calcs — see `AtoD.md` Section 4.1 and `AtoR.md` Section 4.1. Confirm here that the date passed in is correct for each FPS method, or note if different FPS types need a different date.

| FPS Method | FPS Calc Date Used | Notes |
|------------|-------------------|-------|
| GetFPS1 | e.g. Date of Leaving / NRD (whichever earlier) | |
| GetFPS2 | | |
| GetFPS3 | | |

*Note to Coders:*
*TODO: Confirm `FPSCalcDate` is set correctly in `SetupCalculationProperties()` in each main scheme calc for each FPS method*

---

## 2. GetFPS1 `[salary-scheme-module.cs: GetFPS1()]`

### 2.1 FPS1 Definition

Describe the FPS1 rule in plain English as it appears in the scheme rules.

> FPS1 Definition:

### 2.2 Calculation Method

How is FPS1 derived?

- [ ] **Salary history look-back** — query Aurora salary history to find the highest (or most recent) salary within a defined period
- [ ] **Aurora data field** — read a specific extra data field on the member record (e.g. a manually entered quoted salary)
- [ ] **FPP Calculator (Best-of-N)** — use the `FPPCalculator` with bespoke candidate/pro-rate logic (see `FPP-Modules.md`)
- [ ] Other
    - If other, describe: ______________________

### 2.3 Salary History Look-back (complete if method above is "Salary history look-back")

**Table 2.3: Salary History Parameters**

| Parameter | Value | Notes |
|-----------|-------|-------|
| Salary Type Key (per Aurora) | e.g. FTSAL | The salary type to filter on in Aurora salary history |
| Look-back Period | e.g. 3 years | How far back from FPSCalcDate to search |
| Selection Rule | e.g. Highest in any 12-month period | Highest value? Most recent? Average? |
| Include salaries that started before the look-back period but were still current? | Yes / No | i.e. salaries effective before the window but not yet ended |
| Default if no salary found | e.g. 0 | What to return if the salary history is empty |

*Note to Coders:*
*TODO: Update `threeYearsBeforeCalculationDate` (or equivalent) to match the look-back period above*
*TODO: Update `"FTSAL"` salary type key string to the correct Aurora key*
*TODO: Update the `OrderByDescending` / selection logic to match the selection rule (e.g. highest, most recent)*
*TODO: Update the `?? 0` fallback if a different default is required*

### 2.4 Aurora Data Field Look-up (complete if method above is "Aurora data field")

**Table 2.4: Data Field Parameters**

| Parameter | Value | Notes |
|-----------|-------|-------|
| Aurora Field Name (string) | e.g. "Quote FPS1" | The exact field name as held in Aurora Extra Data |
| Field Data Type | Decimal / String | |
| Default if field is empty | e.g. 0 | |

*Note to Coders:*
*TODO: Update `member.ExtraDataStringField("[FIELD NAME]")` and `member.ExtraDataDecimalField("[FIELD NAME]")` with the correct field name*
*TODO: Update the fallback `0m` if a different default is required*

### 2.5 FPP Calculator (complete if method above is "FPP Calculator")

Complete `FPP-Modules.md` for full configuration. Note here any FPS1-specific parameters.

| Parameter | Value |
|-----------|-------|
| Best | |
| OutOf | |
| Salary Type | |

*Note to Coders:*
*TODO: Wire up `IFPPCalculator` with the classes defined/selected in `FPP-Modules.md` and call `fppCalculator.Calculate(salaryHistories)` to get the FPS1 value*

### 2.6 FPS1 Output

**Table 2.6: FPS1 Output**

| Parameter | Value | Notes |
|-----------|-------|-------|
| Type string (per Aurora) | e.g. "FPS1" | Label that appears in the A>D / A>R result |
| Rounding | e.g. 2 decimal places | |
| EffectiveFrom date | e.g. FPSCalcDate | Date to attach to the FinalPensionableSalary object |

*Note to Coders:*
*TODO: Update `Type = "FPS1"` string if Aurora uses a different label*
*TODO: Update `.Round()` call to match the rounding requirement above*
*TODO: Update `EffectiveFrom = FPSCalcDate` if a different date is required*

---

## 3. GetFPS2 `[salary-scheme-module.cs: GetFPS2()]`

*(Complete this section only if GetFPS2 is required per Section 1.1. Copy and repeat this section for GetFPS3, GetFPS4, etc. as needed.)*

### 3.1 FPS2 Definition

> FPS2 Definition:

### 3.2 Calculation Method

- [ ] Salary history look-back
- [ ] Aurora data field
- [ ] FPP Calculator (Best-of-N) — see `FPP-Modules.md`
- [ ] Other — describe: ______________________

### 3.3 Salary History Look-back (complete if applicable)

**Table 3.3: Salary History Parameters**

| Parameter | Value | Notes |
|-----------|-------|-------|
| Salary Type Key (per Aurora) | | |
| Look-back Period | | |
| Selection Rule | | |
| Include salaries that started before look-back period but still current? | Yes / No | |
| Default if no salary found | | |

*Note to Coders:*
*TODO: Add `GetFPS2` method to `SalarySchemeModule` following the same structure as `GetFPS1`. Update all parameters to match the above.*

### 3.4 Aurora Data Field Look-up (complete if applicable)

**Table 3.4: Data Field Parameters**

| Parameter | Value | Notes |
|-----------|-------|-------|
| Aurora Field Name (string) | | |
| Field Data Type | Decimal / String | |
| Default if field is empty | | |

*Note to Coders:*
*TODO: Update field name strings and fallback value in `GetFPS2`*

### 3.5 FPP Calculator (complete if applicable)

| Parameter | Value |
|-----------|-------|
| Best | |
| OutOf | |
| Salary Type | |

### 3.6 FPS2 Output

**Table 3.6: FPS2 Output**

| Parameter | Value | Notes |
|-----------|-------|-------|
| Type string (per Aurora) | e.g. "FPS2" | |
| Rounding | | |
| EffectiveFrom date | | |

---

## 4. Additional FPS Types

If more than two FPS types are required, copy and complete the following table for each additional type (GetFPS3, GetFPS4, etc.).

**Table 4: Additional FPS Summary**

| FPS Method | Definition (plain English) | Method | Salary Type Key | Look-back | Selection Rule | Aurora Field (if field lookup) | Type String | Rounding |
|------------|---------------------------|--------|----------------|-----------|----------------|-------------------------------|-------------|---------|
| GetFPS3 | | | | | | | | |
| GetFPS4 | | | | | | | | |

*Note to Coders:*
*TODO: Add a `GetFPS[x]` method for each row above, following the pattern of `GetFPS1`*

---

## 5. Salary Type Keys in Aurora

List all salary type keys that will be queried in this module. These must be configured in Aurora.

**Note to Analysts:** These are the salary record type keys (e.g. "FTSAL", "PTSAL"). Confirm they exist in Aurora under the scheme salary history configuration.

**Table 5: Salary Type Keys**

| Salary Type Key (per Aurora) | Description | Uploaded / Confirmed? |
|------------------------------|-------------|----------------------|
| | | Yes / No / TBC |
| | | Yes / No / TBC |
| | | Yes / No / TBC |

---

## 6. Dependencies & Setup Checklist

### 6.1 Coder Checklist

*Note to Coders: Perform this checklist*

**Table 6.1**

| # | Location | TODO | Status | Notes |
|---|-----------|------|--------|-------|
| 1 | SalarySchemeModule | Rename class to match module name in Aurora (e.g. `[SchemeName]SalarySchemeModule`) | | |
| 2 | SalarySchemeModule | Add or remove `GetFPS[x]` methods per Section 1.1 | | |
| 3 | SalarySchemeModule | Configure `GetFPS1` per Section 2 (salary type key, look-back, selection rule, type string, rounding) | | |
| 4 | SalarySchemeModule | Configure `GetFPS2` per Section 3 (if required) | | |
| 5 | SalarySchemeModule | Configure any additional FPS methods per Section 4 (if required) | | |
| 6 | FPP-modules.cs | If FPP Calculator method is used for any FPS, wire up bespoke classes per `FPP-Modules.md` | | |
| 7 | Aurora | Confirm all salary type keys in Section 5 are configured in Aurora | | |
| 8 | ActiveToDeferredElementsSchemeModule | Confirm `SalarySchemeModule.GetFPS[x]` calls in Part 1a match the methods defined here | | |
| 9 | SalarySchemeModule | Publish module and assign as a dependency in `ActiveToDeferredElementsSchemeModule` | | |
| 10 | Main A>D / A>R Scheme Calc | Confirm `FPSCalcDate` is set correctly in `SetupCalculationProperties()` per Section 1.2 | | |

---

## 7. Additional Misc Notes

Note any salary history quirks — gaps in data, multiple concurrent salary records, unusual salary type configurations, or known data quality issues.

>

---

## 8. Sign-Off

**Table 8**

| Role | Name | Date |
|------|------|------|
| Analyst | | |
| Coder | | |
| Reviewer | | |
