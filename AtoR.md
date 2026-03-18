# Active to Retired (A>R) Calculation Spec

| Field | Value |
|-------|-------|
| Scheme | |
| Author | |
| Date | |
| Version | 1.0 |
| Status | Draft / Review / Approved |

---

## How to use this template

This is a structured checklist for analysts to complete before coding begins. Each section maps directly to a coding decision in the `AtoR_template.cs` template.

The A>R calculation combines two stages: an Active to Deferred (A>D) stage which builds deferred element values from service history, followed by a Deferred to Retired (D>R) stage which revalues those elements and applies PCLS options. This spec covers both.

**For analysts:** Fill in every table. If you do not know the answer, please insert "TBC" to confirm this has been read and will be updated.
**For coders:** Each section header shows the matching code reference in brackets. Ctrl+F the TODO tag in `AtoR_template.cs` to find where each decision is used. TODOs also included here.

**Note:** Where a scheme also has a standalone D>R calculation, sections 7–9 of this spec should align with the D>R spec. Confirm with the lead analyst if there are any differences between A>R and D>R retirement rules.

---

## 1. Valid Categories `[AtoR_template.cs: ValidateMemberForCalculation — Validation 1]`

Which scheme categories can this calculation run for?

**Note to Analysts:** Keys must match those available and configured within the Aurora Client.

| Category Key (per Aurora) | Description |
|---------------------------|-------------|
| | |
| | |
| | |
| | |
| | |

*Note to Coders:*
*TODO: Populate `categoriesCoded` HashSet in `ValidateMemberForCalculation()` with the category keys above*

---

## 2. Service Builder Configuration `[AtoR_template.cs: Section D — 2a. SERVICE BUILDER]`

### 2.1 Service Units

What unit of service measurement does this scheme use?

- [ ] Years and Complete Months (`ServiceUnits.YearsAndCompleteMonths`) — standard, no bespoke code needed
- [ ] Years and Months Rounded Up (`ServiceUnits.YearsAndMonthsRoundedUp`) — standard, no bespoke code needed
- [ ] Years and Months 15 Rounded Up (`ServiceUnits.YearsAndMonths15RoundedUp`) — standard, no bespoke code needed
- [ ] Years and Days 365 (`MySchemeServiceUnitCalculation`) — bespoke, code already included in template
- [ ] Other
    - If other, describe the required service unit calculation: ______________________

*Note to Coders:*
*TODO: Set the correct `.ServiceUnitCalculation(PARAMETER)` in the `ServiceBuilder` call*
*TODO: If a standard `ServiceUnits` option is selected, remove the `MySchemeServiceUnitCalculation` class from the bottom of the file and remove the instantiation line*
*TODO: If "Years and Days 365" or another bespoke method is required, keep or update `MySchemeServiceUnitCalculation`*

### 2.2 Service Method

- [ ] Default (`ServiceMethod.Default`) — standard
- [ ] Other (bespoke)
    - If other, describe the required method: ______________________

*Note to Coders:*
*TODO: Update `.WithMethod(PARAMETER)` in the `ServiceBuilder` call if not Default*

### 2.3 Service Fill Type

- [ ] Gaps to be Filled (`ServicePeriodFillType.GapsToBeFilled`) — standard
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Update `.WithFillType(PARAMETER)` in the `ServiceBuilder` call if not GapsToBeFilled*

### 2.4 Service End Date

For an A>R the service builder calculates service up to Date of Retirement.

- [ ] Date of Retirement (default — `CalculationEndDate` as set in `SetupCalculationProperties`)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Confirm `CalculationEndDate = Parameters.DateOfRetirement` in `SetupCalculationProperties()` per Section 5.1*

---

## 3. Service Tranches `[AtoR_template.cs: Section D — TODO: Set up tranche keys in Aurora]`

List all service tranches for this scheme. These need to be configured in Aurora **and** are referenced by key throughout the code.

**Note to Analysts:** Provide the tranche key, accrual fraction, and start/end dates. Leave "End Date" blank for open-ended tranches. Keys must exactly match Aurora.

**Table 3A**
- [ ] Insert Category or Categories: ______________________

| Tranche Key (per Aurora) | Accrual Nominator | Accrual Denominator | Start Date | End Date | Notes |
|--------------------------|-------------------|---------------------|------------|----------|-------|
| e.g. PRE97 | 1 | 60 | e.g. Date joined scheme | e.g. 05/04/1997 | |
| e.g. POST97 | 1 | 60 | e.g. 06/04/1997 | e.g. 05/04/2005 | |
| e.g. POST05 | 1 | 60 | e.g. 06/04/2005 | | Open-ended |
| | | | | | |
| | | | | | |

**Note to Analysts:** Copy and paste the above table for additional categories if required.

*Note to Coders:*
*TODO: Set up service tranches in Aurora under Client > Scheme > Admin > Categories > Service Tranches*
*TODO: Confirm tranche keys match those used in `ActiveToDeferredElementsSchemeModule`*

---

## 4. Final Pensionable Salary `[AtoR_template.cs: SetupCalculationProperties, Calculate() return]`

### 4.1 FPS Calc Date

What date is used as the reference point for calculating the Final Pensionable Salary?

**Note to Analysts:** For an A>R the FPS Calc Date is typically the Date of Retirement (or Normal Retirement Date if earlier), depending on scheme rules.

- [ ] Date of Retirement
- [ ] Normal Retirement Date (where earlier than Date of Retirement)
- [ ] Normal Retirement Date (always)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Set `FPSCalcDate` in `SetupCalculationProperties()` accordingly*

### 4.2 FPS Values Included in Results

Which FPS values should be returned in the result output? (These must match the methods in `SalarySchemeModule`.)

**Table 4.2: FPS Results**

| FPS Method | Include in A>R Result? (Y/N) |
|------------|------------------------------|
| GetFPS1 | |
| GetFPS2 | |
| GetFPS3 | |

*Note to Coders:*
*TODO: Add or remove `SalarySchemeModule.GetFPS[x](Member, FPSCalcDate)` entries in the `FinalPensionableSalary` list in the `Calculate()` return block (note: FinalPensionableSalary list is not in the A>R return block by default but can be added for logging purposes via InterimValues if required)*

---

## 5. Calculation Properties `[AtoR_template.cs: SetupCalculationProperties()]`

### 5.1 Calculation End Date

For an A>R, service and deferred element values are calculated up to Date of Retirement.

- [ ] Date of Retirement (default — `Parameters.DateOfRetirement`)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Confirm `CalculationEndDate = Parameters.DateOfRetirement` in `SetupCalculationProperties()`*

### 5.2 Pre-1997 Excess Pension Element Key

What is the key for the Pre-1997 excess deferred element?

- [ ] PRE97 (default)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Update `protected const string pre97ExcessPensionElement = "PRE97"` if not "PRE97"*

### 5.3 DC Fund

The DC fund is calculated by summing the most recent fund value per provider from `Member.ExternalFundValues`.

Are there any non-DC values held in `Member.ExternalFundValues` that should be excluded?

- [ ] No — all values are DC fund values (default)
- [ ] Yes — some values should be excluded
    - If yes, describe what should be excluded and how to identify them: ______________________

*Note to Coders:*
*TODO: Update the `DcFund` assignment in `SetupCalculationProperties()` if non-DC values need to be filtered*
*TODO: Also review Validation 5 (AVC check) in `PostCalculationValidation()` — it uses the same ExternalFundValues logic*

---

## 6. GMP Configuration `[AtoR_template.cs: SetupCalculationProperties() — GMP sections]`

### 6.1 GMP Type

Which GMP revaluation type applies?

- [ ] Fixed (`GmpType.Fixed`) — fixed rate revaluation
- [ ] Limited (`GmpType.Limited`) — limited revaluation
- [ ] S148 (`GmpType.S148`) — section 148 revaluation

*Note to Coders:*
*TODO: Update the `GmpType` parameter in `GmpGlobalModules.CalculateActiveGuaranteedMinimumPensionValues(...)` accordingly*

### 6.2 Future S148 Factor

If GMP type is S148, what future factor should be used for years not yet published?

- [ ] Not applicable — use `1.0m`
- [ ] Applicable
    - If applicable, specify the factor (e.g. `1.0325` for 3.25%): ______________________

*Note to Coders:*
*TODO: Update the final decimal parameter in `CalculateActiveGuaranteedMinimumPensionValues(...)` to the factor above*

### 6.3 GMP Missed Increases

**Date missed increases are calculated up to:**

- [ ] Date of Retirement (default — `Parameters.DateOfRetirement`)
- [ ] Other
    - If other, specify: ______________________

**Date missed increases are applied from:**

- [ ] 1 April (default — parameters `4, 1`)
- [ ] Other
    - If other, specify day and month: ______________________

**Assumed future missed increase rate:**

- [ ] 1.0 (no increase)
- [ ] 1.03 (3%)
- [ ] Other
    - If other, specify factor: ______________________

*Note to Coders:*
*TODO: Update `GmpGlobalModules.MissedIncreaseDateOfLeaving(...)` parameters per the above*

### 6.4 Additional GMP Notes

Note any non-standard GMP rules (e.g. alternative GMP date basis, bespoke uplift rules).

>

---

## 7. Revaluation of Deferred Elements `[AtoR_template.cs: CalculateRetirementElementsA2R()]`

This section defines how each deferred element is uplifted from the A>D value to its in-payment value at retirement. Elements are grouped by the type of uplift they receive.

### 7.1 Retirement Factor Basis

Are retirement factor periods calculated on an Age Attained or Term Basis?

> **Age Attained:** Only full months attained by Date of Retirement are counted.
> **Term Basis:** Calendar months between the retirement date and NPA are counted, regardless of whether the month has been completed.

- [ ] Age Attained Basis
- [ ] Term Basis

*Note to Coders:*
*TODO: If Age Attained Basis, update `Parameters.DateOfRetirement` to `DateModule.AgeAttainedRetirementDate(Member.Person.DateOfBirth, Parameters.DateOfRetirement)` in the `GetSchemeFactor(...)` call within `CalculateRetirementElementsA2R`*

### 7.2 Element Groupings

Group all deferred element keys by the type of uplift they receive at retirement.

**Note to Analysts:** The three standard groups are shown below. If any element has a different treatment (e.g. higher of scheme LRF and GMP uplift, or a scheme underpin), note it in the "Other / Bespoke" group.

**Table 7.2A: ERF/LRF Elements**
These elements receive standard scheme Early or Late Retirement Factors.

- [ ] Insert Category or Categories: ______________________

| Deferred Element Key | Receives ERF? | Receives LRF? | Notes |
|---------------------|---------------|---------------|-------|
| e.g. PRE97 | Yes | Yes | |
| e.g. POST97 | Yes | Yes | |
| e.g. POST05 | Yes | Yes | |
| | | | |
| | | | |

**Table 7.2B: Pre-88 GMP Elements**
These elements receive GMP increments (s148) only — no ERFs or LRFs.

| Deferred Element Key | Notes |
|---------------------|-------|
| e.g. PRE88GMP | |
| | |

**Table 7.2C: Post-88 GMP Elements**
These elements receive GMP increments (s148) and missed increases — no ERFs or LRFs.

| Deferred Element Key | Notes |
|---------------------|-------|
| e.g. POST88GMP | |
| | |

**Table 7.2D: Other / Bespoke Treatment**
Any elements with non-standard uplift rules (e.g. higher of two factors, underpin, nil uplift).

| Deferred Element Key | Uplift Rule | Notes |
|---------------------|-------------|-------|
| | | |
| | | |

*Note to Coders:*
*TODO: Update `nonGMPElements`, `gmpUpliftPRE88`, and `gmpUpliftPOST88` arrays in `CalculateRetirementElementsA2R()` with the keys from Tables 7.2A–C*
*TODO: Add bespoke `if` blocks for any elements in Table 7.2D*

### 7.3 ERF Application Detail

Please populate the table below with ERF rules applicable for each element.

**Note to Analysts:** "Application Date" is the date from which the early retirement factor period is measured FROM (e.g. NPA date). ERF table keys must match those uploaded in Aurora.

**Table 7.3A: ERF Application**
- [ ] Insert Category or Categories: ______________________

| Deferred Element Key | ERF App Date Male | ERF App Date Female | ERF Table Male | ERF Table Female | Notes |
|---------------------|-------------------|---------------------|----------------|------------------|-------|
| | | | | | |
| | | | | | |
| | | | | | |

**Table 7.3B: ERF Factor Tables to Upload**

| Table Key (per Aurora) | Uploaded? (Y/N/TBC) |
|------------------------|---------------------|
| | |
| | |

**Note to Analysts:** Attach or reference the ERF table .csv. Format: Years, Months (can be 0), Value.

*Note to Coders:*
*TODO: Set up ERF Application Date and ERF Table in `ReusableFunctionsSchemeModule` (called via `ReusableFunctionsSchemeModule.ERFApplicationDate` and `ReusableFunctionsSchemeModule.ERFTable`)*
*TODO: Upload ERF .csv files to Factors*
*TODO: Set the ERF table name in `ValidateMemberForCalculation()` Validation 2 (`ERFfactorTable` string)*

### 7.4 LRF Application Detail

**Note to Analysts:** "Application Date" is the date from which the late retirement factor period is measured FROM (e.g. NPA date or date member reaches age 65).

**Table 7.4A: LRF Application**
- [ ] Insert Category or Categories: ______________________

| Deferred Element Key | LRF App Date Male | LRF App Date Female | LRF Table Male | LRF Table Female | Notes |
|---------------------|-------------------|---------------------|----------------|------------------|-------|
| | | | | | |
| | | | | | |
| | | | | | |

**Table 7.4B: LRF Factor Tables to Upload**

| Table Key (per Aurora) | Uploaded? (Y/N/TBC) |
|------------------------|---------------------|
| | |
| | |

**Note to Analysts:** Attach or reference the LRF table .csv. Format: Years, Months (can be 0), Value.

*Note to Coders:*
*TODO: Set up LRF Application Date and LRF Table in `ReusableFunctionsSchemeModule`*
*TODO: Upload LRF .csv files to Factors*
*TODO: Set the LRF table name in `ValidateMemberForCalculation()` Validation 2 (`LRFfactorTable` string)*

### 7.5 Additional Revaluation Notes

Note anything non-standard — underpins, alternative revaluation mechanisms, scheme-specific GMP treatment.

>

---

## 8. Pension Element Roll-up `[AtoR_template.cs: Step 3b — PreCommutationPensionElements]`

How do deferred elements group into pension elements for the purposes of commutation?

**Note to Analysts:** Please use Aurora Keys. The PRE97 excess element is handled automatically by the `PreCommutationPensionElements` call and will deduct GMP from the nominated Pre97 element.

**Table 8A**
- [ ] Insert Category or Categories: ______________________

| Pension Element Key (per Aurora) | Built from (Deferred Element Key(s)) |
|----------------------------------|--------------------------------------|
| e.g. PRE97 | e.g. PRE97, PRE88GMP, POST88GMP |
| e.g. POST97 | e.g. POST97, POST05 |
| | |
| | |

**Note to Analysts:** Copy and paste the above table for additional categories if required.

*Note to Coders:*
*TODO: Populate `PCLSandCommutationSchemeModule > SetElements` with the above groupings*

---

## 9. Commutation / PCLS `[AtoR_template.cs: Step 3b — PensionOptionBuilder]`

### 9.1 Commutation Factors

Please allocate commutation factor (CF) tables against all relevant pension element keys.

Single CF table for all elements?
- [ ] Yes
    - If yes, specify the single CF table key: ______________________
- [ ] No — multiple CF tables
    - If no, complete the table below:

**Table 9.1A**
- [ ] Insert Category or Categories: ______________________
- [ ] Applicable Male
- [ ] Applicable Female

| CF Table Key | Pension Elements Applicable | Notes |
|--------------|----------------------------|-------|
| | | |
| | | |
| | | |

**Table 9.1B: CF Tables to Upload**

| Table Key (per Aurora) | Uploaded? (Y/N/TBC) |
|------------------------|---------------------|
| | |
| | |

**Note to Analysts:** Attach or reference the CF table .csv. Format: Age (Years or Years and Months), Value.

*Note to Coders:*
*TODO: Populate `PCLSandCommutationSchemeModule > CommutationFactorsForElementsList` per the above*
*TODO: Upload CF .csv files to Factors*

### 9.2 GMP Restriction

**GMP restriction based on:**

- [ ] Total GMP at **Retirement Date** (default — `ElementValuesSchemeModule.TotalGMPAt.RetirementDate(GmpValues)`)
- [ ] Total GMP at **GMP Date**
- [ ] Other
    - If other, please specify: ______________________

*Note to Coders:*
*TODO: Set up and publish `ElementValuesSchemeModule` and assign as a dependency*
*TODO: Update `gmpRestriction` assignment and the `"PRE97"` parameter in `.WithGMPRestriction(...)` to match the PRE97 excess element key from Section 5.2*

### 9.3 PCLS Options Builder Parameters

Select all parameters required for the `PensionOptionBuilder`:

- [X] `.WithCommutationFactors` — Required for all schemes
- [X] `.WithGMPRestriction` — Required for all schemes
- [ ] `.Ordered` — Select if commutation is ordered (not proportional)
    - If Ordered, complete Section 9.3.1 below
- [X] `.WithDCFund` — Select if members may have DC fund values. De-select only if definitely no DC/AVC funds.
- [ ] `.WithSpouses` — Select only if spouse % is NOT 50%
    - If selected, specify percentage: ______
- [ ] `.WithSpousesGMPMinimumOption` — Select if a non-default GMP spouse treatment applies
    - [ ] `NilPre88GMP` — All GMP has 50% spouses except Female Pre-88 GMP which has 0% (default when selected)
    - [ ] `FiftyPercent` — All GMP has 50% spouses
    - [ ] Other
        - If other, specify: ______________________
- [ ] `.WithPIE` — Select if PIE (Pension Increase Exchange) is applicable
    - If selected, complete Section 9.3.2 below
- [ ] `.WithDecimalPlaces` — Select only if decimal places is NOT 2
    - If selected, specify number of decimal places: ______

*Note to Coders:*
*TODO: Add "PensionOption" global dependency*
*TODO: Uncomment optional `.Ordered`, `.WithSpouses`, `.WithSpousesGMPMinimumOption`, `.WithPIE`, `.WithDecimalPlaces` lines in `PensionOptionBuilder` as required*

#### 9.3.1 Order for Commutation (if `.Ordered` selected)

Please insert pension element keys in priority order for commutation.

**Table 9.3.1A**
- [ ] Category or Categories: ______________________

| Priority | Pension Element Key |
|----------|---------------------|
| 1 | |
| 2 | |
| 3 | |
| 4 | |
| 5 | |

**Note to Analysts:** Copy and paste the above table for additional categories if required.

*Note to Coders:*
*TODO: Uncomment and populate `OrderForCommutation` list, then uncomment `.Ordered(OrderForCommutation)` in the builder*

#### 9.3.2 For PIE Only (if `.WithPIE` selected)

PIE Factor Interpolation:
- [ ] YearMonth
- [ ] YearDay
- [ ] Other — specify: ______________________

PIE Application:
- [ ] Apply After Commutation
- [ ] Other — specify: ______________________

*Note to Coders:*
*TODO: Populate `.WithPIE(pieElements, PIEFactorInterpolation.[X], PIEApplication.[X])` per the above. Seek further guidance from V2 team if required.*

### 9.4 Additional Commutation Notes

Note anything non-standard about commutation for this scheme.

>

---

## 10. Validations `[AtoR_template.cs: ValidateMemberForCalculation(), PostCalculationValidation()]`

### 10.1 Standard pre-calculation checks (Errors — stop the calc)

**Table 10.1**

| # | Check | Applies? | Notes |
|---|-------|----------|-------|
| 1 | Member's category is valid for this calculation | Yes / No | See Section 1 |
| 2 | Retirement date is within the range of available ERF/LRF tables | Yes / No | Requires ERF and LRF table names per Sections 7.3 and 7.4 |

### 10.2 Standard post-calculation checks (Warnings — calc still runs)

**Table 10.2**

| # | Check | Applies? | Notes |
|---|-------|----------|-------|
| 3 | Capital value ≤ £30,000 (trivial commutation) | Yes / No | |
| 4 | Maximum cash is negative (GMP not covered) | Yes / No | |
| 5 | External fund values found (AVC check) | Yes / No | |

### 10.3 Additional scheme-specific validations

**Note to Analysts:**
- Error: Stops the calculation at the point it is hit.
- Warning: Flags in the log but does not prevent the calculation running.

**Table 10.3**

| # | Check | Error or Warning? | Pre or Post Calc? | Message to display |
|---|-------|-------------------|-------------------|--------------------|
| | | | | |
| | | | | |
| | | | | |

*Note to Coders:*
*TODO: Populate `ValidateMemberForCalculation()` with any additional pre-calc checks*
*TODO: Populate `PostCalculationValidation()` with any post-calc checks*

---

## 11. Dependencies & Setup Checklist `[AtoR_template.cs: using statements, dependencies]`

### 11.1 Coder Checklist

*Note to Coders: Perform this checklist*

**Table 11.1**

| # | Location | TODO | Status | Notes |
|---|-----------|------|--------|-------|
| 1 | Main A>R Scheme Calc | Replace `TEMPLATEActiveToRetiredCalculation` with `[SchemeName]ActiveToRetiredCalculation` | | |
| 2 | Main A>R Scheme Calc | Update `pre97ExcessPensionElement` if not "PRE97", per Section 5.2 | | |
| 3 | SalarySchemeModule | Complete and publish per Salary Scheme Module Spec (`SalarySchemeModule.md`) | | |
| 4 | ActiveToDeferredElementsSchemeModule | Complete and publish per Elements Module Spec (`AtoD-ElementsModule.md`) | | |
| 5 | Main A>R Scheme Calc | Set `ActiveToDeferredElementsSchemeModule` dependency | | |
| 6 | Main A>R Scheme Calc | Set service builder configuration per Section 2 | | |
| 7 | Aurora | Set up service tranches per Section 3 | | |
| 8 | Main A>R Scheme Calc | Set `CalculationEndDate` and `FPSCalcDate` per Sections 5.1 and 4.1 | | |
| 9 | Main A>R Scheme Calc | Set `LifetimeAllowance` global dependency | | |
| 10 | Main A>R Scheme Calc | Set GMP global dependency and configure GMP values per Section 6 | | |
| 11 | ReusableFunctionsSchemeModule | Set up ERF/LRF application dates and tables per Sections 7.3 and 7.4 and publish | | |
| 12 | Main A>R Scheme Calc | Set `ReusableFunctionsSchemeModule` dependency | | |
| 13 | Main A>R Scheme Calc | Update element groupings in `CalculateRetirementElementsA2R()` per Section 7.2 | | |
| 14 | Main A>R Scheme Calc | Set retirement factor basis (Age Attained vs Term) per Section 7.1 | | |
| 15 | Factors | Upload ERF and LRF .csv files per Sections 7.3 and 7.4 | | |
| 16 | Main A>R Scheme Calc | Set Validation 2 ERF/LRF table name strings per Sections 7.3 and 7.4 | | |
| 17 | PCLSandCommutationSchemeModule | Populate `SetElements` per Section 8 and `CommutationFactorsForElementsList` per Section 9.1 and publish | | |
| 18 | Main A>R Scheme Calc | Set `PCLSandCommutationSchemeModule` dependency | | |
| 19 | ElementValuesSchemeModule | Set up GMP restriction per Section 9.2 and publish | | |
| 20 | Main A>R Scheme Calc | Set `ElementValuesSchemeModule` dependency | | |
| 21 | Main A>R Scheme Calc | Add "PensionOption" global dependency | | |
| 22 | Main A>R Scheme Calc | Configure `PensionOptionBuilder` per Section 9.3 (ordered, spouses, PIE, decimal places) | | |
| 23 | Factors | Upload CF .csv files per Section 9.1 | | |
| 24 | Main A>R Scheme Calc | Populate `ValidateMemberForCalculation()` per Sections 1 and 10 | | |
| 25 | Main A>R Scheme Calc | Populate `PostCalculationValidation()` per Section 10 | | |
| 26 | Main A>R Scheme Calc | Remove or update `MySchemeServiceUnitCalculation` class per Section 2.1 | | |

---

## 12. Additional Misc Notes

Capture anything that doesn't fit the sections above — scheme quirks, known issues, or things the coder should watch out for.

>

---

## 13. Sign-Off

**Table 13**

| Role | Name | Date |
|------|------|------|
| Analyst | | |
| Coder | | |
| Reviewer | | |
