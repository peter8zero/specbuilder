# FPP (Final Pensionable Pay) Bespoke Modules Spec

| Field | Value |
|-------|-------|
| Scheme | |
| Author | |
| Date | |
| Version | 1.0 |
| Status | Draft / Review / Approved |

---

## How to use this template

This spec covers the bespoke FPP override classes used when the global `FPPCalculator` builder cannot produce the required Final Pensionable Pay result out of the box.

These classes are called from within `SalarySchemeModule` (see `SalarySchemeModule.md`). Complete this spec first, then reference it when setting up `GetFPS[x]` methods in the Salary Scheme Module.

**For analysts:** Complete Section 1 to determine whether bespoke modules are needed at all. If yes, complete the remaining sections. If the standard global builder is sufficient, this spec may be short.
**For coders:** Each section header shows the matching class in `FPP-modules.cs`. The note in the file recommends starting with the global builder — it will output which internal classes it uses, giving you a starting point for any bespoke overrides needed.

---

## 1. FPP Definition `[FPP-modules.cs: FPPCalculator setup]`

### 1.1 FPP Definition

What is the definition of Final Pensionable Pay for this scheme?

**Note to Analysts:** Describe the rule in plain English as it appears in the scheme rules. Example: "The highest pensionable salary in any 12-month period during the 3 years immediately preceding the calculation date." This drives all the decisions below.

> FPP Definition:

### 1.2 Are Bespoke FPP Modules Required?

The global FPP calculator builder should be tried first. Bespoke modules are only needed when the standard builder cannot replicate the required FPP logic.

- [ ] No — the global `FPPCalculator` builder is sufficient (standard Best-of-N-out-of-Y logic with standard pro-rating)
    - If no, document the builder configuration in Section 2 only, then move to sign-off.
- [ ] Yes — one or more bespoke override classes are required
    - If yes, complete all remaining sections.

*Note to Coders:*
*TODO: Attempt to configure the scheme FPP using the global `FPPCalculator` builder first. Once built, the builder will indicate which internal classes it is using. Use those as the basis for any bespoke overrides.*

---

## 2. FPP Calculator Setup `[FPP-modules.cs: IFPPCalculator fppCalculator setup]`

### 2.1 Core Parameters

**Table 2.1: FPP Calculator Parameters**

| Parameter | Value | Notes |
|-----------|-------|-------|
| Best | e.g. 1 | The number of years taken as the FPP (e.g. best 1 year) |
| OutOf | e.g. 3 | The look-back window in years (e.g. out of the last 3 years) |
| Salary Type | e.g. FTSAL | The Aurora salary type key to draw salaries from |
| Calculation Date | e.g. Date of Leaving / Date of Retirement | The end date for the FPP look-back period |
| Minimum Total Period | e.g. 1 year | Minimum period required for an FPP candidate to be valid (leave blank if not applicable) |

*Note to Coders:*
*TODO: Set `fppCalculator.Best`, `fppCalculator.OutOf`, `fppCalculator.salaryType`, and `calculationDate` per the above*
*TODO: Set `FPPMethod.MinimumTotalPeriod` if applicable (leave unset if not required)*

### 2.2 Calculation Date Source

What date is used as the end point for the FPP look-back period?

**Note to Analysts:** For an A>D this is typically the Date of Leaving or NRD (whichever is earlier). For an A>R this is typically the Date of Retirement or NRD. Confirm per the scheme rules.

- [ ] Date of Leaving (A>D)
- [ ] Date of Retirement (A>R)
- [ ] Normal Retirement Date (where earlier)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Set `calculationDate` in `SalarySchemeModule.GetFPS[x]` to the correct date. The `FPSCalcDate` parameter passed into the method is set in the main scheme calc — confirm it is set correctly there.*

---

## 3. FPP Method `[FPP-modules.cs: MySchemeFPPMethod]`

The FPP Method defines how the FPP is calculated from a list of candidates.

### 3.1 Is the Standard FPP Method Sufficient?

The standard `FPPBestMethod` selects the highest FPP from the candidate periods. This is the most common approach.

- [ ] Yes — standard best-of logic is sufficient (no bespoke `MySchemeFPPMethod` needed)
- [ ] No — bespoke FPP calculation logic is required
    - If no, describe what the bespoke logic should do:

> Bespoke FPP Method Description:

### 3.2 Rounding

What rounding should be applied to the final FPP value?

- [ ] 2 decimal places (to the penny — default: `.Round(2)`)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: If standard method is sufficient, remove `MySchemeFPPMethod` class and use the appropriate global method in the builder*
*TODO: If bespoke, implement the required logic in `MySchemeFPPMethod.Calculate()` and update the `.Round()` call as needed*

---

## 4. Candidate Creator `[FPP-modules.cs: MySchemeCandidateCreator]`

The Candidate Creator defines what periods are considered as potential FPP candidates.

### 4.1 Candidate Creation Rule

The standard `YearlyCandidateCreator` produces yearly candidates starting from each salary change date within the look-back window, plus a candidate for the final X years immediately before the end date.

**Note to Analysts:** This matches the most common rule: "highest salary in any 12-month period during the last N years." Confirm whether the standard rule applies or whether candidates should be defined differently.

- [ ] Yes — standard yearly candidate logic is sufficient
- [ ] No — bespoke candidate periods are required
    - If no, describe how candidates should be defined:

> Bespoke Candidate Creator Description:

*Note to Coders:*
*TODO: If standard candidate creation is sufficient, remove `MySchemeCandidateCreator` and use `YearlyCandidateCreator` in the builder*
*TODO: If bespoke, implement the required logic in `MySchemeCandidateCreator.PopulateCandidateDates()`*

---

## 5. Pro Rate `[FPP-modules.cs: MySchemeProRate]`

Pro-rating handles salary periods that partially overlap a candidate window. The denominator and numerator determine what fraction of a salary applies to the candidate.

### 5.1 Pro Rate Method

The standard `ProRateDays` method pro-rates salaries by the number of days they fall within the candidate period relative to the total days in the salary year.

- [ ] Yes — pro-rating by days is correct for this scheme
- [ ] No — a different pro-rate method is required
    - If no, describe the required pro-rating logic:

> Bespoke Pro Rate Description:

### 5.2 Part-Time Considerations

Does the scheme require part-time salary adjustments within the FPP pro-rating?

- [ ] No — full-time salary equivalents are used throughout
- [ ] Yes — part-time hours/fraction needs to be applied
    - If yes, describe how: ______________________

*Note to Coders:*
*TODO: If standard pro-rate by days is sufficient, remove `MySchemeProRate` and use `ProRateDays` in the builder*
*TODO: If bespoke, implement the required logic in `MySchemeProRate.ProRateFPPCandidate()`*

---

## 6. FPP Dates `[FPP-modules.cs: MySchemeFPPDates]`

FPP Dates define the earliest and end dates of the look-back window.

### 6.1 End Date

The end date is the calculation date — the point up to which salaries are considered.

- [ ] The calculation date (default — `calculationDate` as passed in from `SalarySchemeModule`)
- [ ] Other
    - If other, specify: ______________________

### 6.2 Earliest Date

The earliest date is the furthest back in time salaries are considered. This is typically `calculationDate` minus `OutOf` years plus 1 day.

- [ ] Calculation date minus OutOf years plus 1 day (default — e.g. 3 years back + 1 day)
- [ ] Other
    - If other, specify: ______________________

**Note to Analysts:** The "+ 1 day" is important for correct pro-rating at the boundary. Confirm whether the scheme rules specify inclusive or exclusive start dates.

*Note to Coders:*
*TODO: If standard date logic is sufficient, confirm `MySchemeFPPDates` constructor matches: `_earliestDate = calculationDate.PlusYears(-OutOf).PlusDays(1)` and `_endDate = calculationDate`*
*TODO: If bespoke, update the constructor logic in `MySchemeFPPDates` accordingly*

---

## 7. FPP Summary

Once sections 3–6 are complete, confirm which override classes are actually needed for this scheme.

**Table 7: Bespoke Class Summary**

| Class | Required? | Standard Alternative if Not Required |
|-------|-----------|--------------------------------------|
| `MySchemeFPPMethod` | Yes / No | `FPPBestMethod` |
| `MySchemeCandidateCreator` | Yes / No | `YearlyCandidateCreator` |
| `MySchemeProRate` | Yes / No | `ProRateDays` |
| `MySchemeFPPDates` | Yes / No | Standard builder date logic |

*Note to Coders:*
*TODO: Remove any bespoke class that is not required and replace with the listed standard alternative in the builder configuration*
*TODO: Any bespoke classes that ARE required should be implemented in `FPP-modules.cs` and referenced in `SalarySchemeModule.GetFPS[x]`*

---

## 8. Dependencies & Setup Checklist

### 8.1 Coder Checklist

**Table 8.1**

| # | Location | TODO | Status | Notes |
|---|-----------|------|--------|-------|
| 1 | SalarySchemeModule | Confirm `FPSCalcDate` is passed in correctly from the main A>D / A>R scheme calc | | See Section 2.2 |
| 2 | SalarySchemeModule | Set `calculationDate`, `Best`, `OutOf`, and `salaryType` per Section 2 | | |
| 3 | FPP-modules.cs | Implement or remove `MySchemeFPPMethod` per Section 3 and Table 7 | | |
| 4 | FPP-modules.cs | Implement or remove `MySchemeCandidateCreator` per Section 4 and Table 7 | | |
| 5 | FPP-modules.cs | Implement or remove `MySchemeProRate` per Section 5 and Table 7 | | |
| 6 | FPP-modules.cs | Implement or remove `MySchemeFPPDates` per Section 6 and Table 7 | | |
| 7 | SalarySchemeModule | Wire up the required classes into the `IFPPCalculator` setup | | |
| 8 | SalarySchemeModule | Confirm rounding on `fppCalculator.Calculate()` result per Section 3.2 | | |

---

## 9. Additional Misc Notes

Note any scheme-specific quirks in the salary history, unusual salary types, or edge cases the coder should be aware of.

>

---

## 10. Sign-Off

**Table 10**

| Role | Name | Date |
|------|------|------|
| Analyst | | |
| Coder | | |
| Reviewer | | |
