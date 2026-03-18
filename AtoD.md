# Active to Deferred (A>D) Calculation Spec

| Field | Value |
|-------|-------|
| Scheme | |
| Author | |
| Date | |
| Version | 1.0 |
| Status | Draft / Review / Approved |

---

## How to use this template

This is a structured checklist for analysts to complete before coding begins. Each section maps directly to a coding decision in the `AtoD.cs` template.

**For analysts:** Fill in every table. If you do not know the answer, please insert "TBC" to confirm this has been read and will be updated.
**For coders:** Each section header shows the matching code reference in brackets. Ctrl+F the TODO tag in `AtoD.cs` to find where each decision is used. TODOs also included here.

---

## 1. Valid Categories `[AtoD.cs: ValidateMemberForCalculation — Validation 1]`

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

## 2. Service Builder Configuration `[AtoD.cs: Section D — 2a. SERVICE BUILDER]`

### 2.1 Service Units

What unit of service measurement does this scheme use?

**Note to Analysts:** "Years and Complete Months" is the most common. "Years and Days 365" is a bespoke calculation and requires additional code. Confirm with the scheme spec.

- [ ] Years and Complete Months (`ServiceUnits.YearsAndCompleteMonths`) — standard, no bespoke code needed
- [ ] Years and Months Rounded Up (`ServiceUnits.YearsAndMonthsRoundedUp`) — standard, no bespoke code needed
- [ ] Years and Months 15 Rounded Up (`ServiceUnits.YearsAndMonths15RoundedUp`) — standard, no bespoke code needed
- [ ] Years and Days 365 (`MySchemeServiceUnitCalculation`) — bespoke, code already included in template
- [ ] Other
    - If other, describe the required service unit calculation: ______________________

*Note to Coders:*
*TODO: Set the correct `.ServiceUnitCalculation(PARAMETER)` in the `ServiceBuilder` call*
*TODO: If a standard `ServiceUnits` option is selected, remove the `MySchemeServiceUnitCalculation` class from the bottom of the file and remove the `IServiceUnitCalculation mySchemeServiceUnitCalculation = new MySchemeServiceUnitCalculation();` instantiation line*
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

The service builder needs to know when to stop counting service. For an A>D this is typically the Date of Leaving.

- [ ] Date of Leaving (default — `CalculationEndDate` as set in `SetupCalculationProperties`)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Confirm the `CalculationEndDate` in `SetupCalculationProperties()` is set correctly per Section 5.1 below*
*TODO: Check the `ServiceBuilder` constructor parameter `CalculationEndDate` is correct*

---

## 3. Service Tranches `[AtoD.cs: Section D — TODO: Set up tranche keys in Aurora]`

List all service tranches for this scheme. These need to be configured in Aurora **and** are referenced by key throughout the code.

**Note to Analysts:** Provide the tranche key, accrual fraction (e.g. 1/60), and the start and end dates of the tranche. Leave "End Date" blank if the tranche is open-ended (still accruing). Keys must exactly match what is entered in Aurora.

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
*TODO: Set up service tranches in Aurora under Client > Scheme > Admin > Categories > Service Tranches using the keys and details above*
*TODO: Confirm tranche keys match those used in `ActiveToDeferredElementsSchemeModule` (Section 1 of the Elements Module spec)*

---

## 4. Final Pensionable Salary `[AtoD.cs: Section D — 2b, and Calculate() return block]`

### 4.1 FPS Calc Date

What date is used as the reference point for calculating the Final Pensionable Salary?

**Note to Analysts:** The FPS Calc Date is the date from which the salary look-back period is measured. For an A>D it is commonly the Date of Leaving or the Normal Retirement Date (whichever is earlier), depending on the scheme rules.

- [ ] Date of Leaving
- [ ] Normal Retirement Date (where earlier than Date of Leaving)
- [ ] Normal Retirement Date (always)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Set `FPSCalcDate` in `SetupCalculationProperties()` per the above. e.g. `FPSCalcDate = Parameters.DateOfLeaving < Member.NormalRetirementDate ? Parameters.DateOfLeaving : Member.NormalRetirementDate;`*

### 4.2 FPS Values Included in Results

Which FPS values should be returned in the `FinalPensionableSalary` result list? (These must match the methods defined in `SalarySchemeModule`.)

**Note to Analysts:** See the Salary Scheme Module Spec for definitions of each FPS. List every FPS type that should appear in the A>D result output.

**Table 4.2: FPS Results**

| FPS Method | Include in A>D Result? (Y/N) |
|------------|------------------------------|
| GetFPS1 | |
| GetFPS2 | |
| GetFPS3 | |

*Note to Coders:*
*TODO: Add or remove `SalarySchemeModule.GetFPS[x](Member, FPSCalcDate)` entries in the `FinalPensionableSalary` list within the `Calculate()` return block*

---

## 5. Calculation Properties `[AtoD.cs: SetupCalculationProperties()]`

### 5.1 Calculation End Date

For an A>D, service and deferred element values are calculated up to the Date of Leaving.

- [ ] Date of Leaving (default — `Parameters.DateOfLeaving`)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Set `CalculationEndDate` in `SetupCalculationProperties()` accordingly*

### 5.2 Date Pension Due

The date pension is due is typically the later of Date of Leaving and Normal Retirement Date (i.e. NRD, unless the member has left after NRD in which case it is DOL).

- [ ] NRD, or DOL if DOL is after NRD (default)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Confirm `DatePensionDue` expression in the `Calculate()` return block is correct: `Parameters.DateOfLeaving > Member.NormalRetirementDate ? Parameters.DateOfLeaving : Member.NormalRetirementDate`*

### 5.3 Pre-1997 Excess Pension Element Key

What is the key for the Pre-1997 excess deferred element (used to align with GMP restriction logic)?

- [ ] PRE97 (default)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Update `protected const string pre97ExcessPensionElement = "PRE97"` if not "PRE97"*

### 5.4 DC Fund

The DC fund is calculated by summing the most recent fund value per provider from `Member.ExternalFundValues`. The template assumes all values held there are DC fund values.

Are there any non-DC values held in `Member.ExternalFundValues` that should be excluded?

- [ ] No — all values in ExternalFundValues are DC fund values (default)
- [ ] Yes — some values should be excluded
    - If yes, describe what should be excluded and how to identify them: ______________________

*Note to Coders:*
*TODO: Update the `DcFund` assignment in `SetupCalculationProperties()` if non-DC values need to be filtered out*

---

## 6. GMP Configuration `[AtoD.cs: SetupCalculationProperties() — GMP sections]`

### 6.1 GMP Type

Which GMP revaluation type applies?

- [ ] Fixed (`GmpType.Fixed`) — fixed rate revaluation
- [ ] Limited (`GmpType.Limited`) — limited revaluation
- [ ] S148 (`GmpType.S148`) — section 148 revaluation

*Note to Coders:*
*TODO: Update the `GmpType` parameter in `GmpGlobalModules.CalculateActiveGuaranteedMinimumPensionValues(...)` accordingly*

### 6.2 Future S148 Factor

If GMP type is S148, what future revaluation factor should be used for years where the S148 order has not yet been published?

- [ ] Not applicable (GMP type is Fixed or Limited — set factor to `1.0m`)
- [ ] Applicable
    - If applicable, specify the factor (e.g. `1.0325` for 3.25%): ______________________

*Note to Coders:*
*TODO: Update the final decimal parameter in `CalculateActiveGuaranteedMinimumPensionValues(...)` to the factor above (use `1.0m` if not S148)*

### 6.3 GMP Missed Increases

GMP missed increases reflect the pension increases a member would have received had they been in payment.

**Date missed increases are calculated up to:**

- [ ] Date of Leaving (default — `Parameters.DateOfLeaving`)
- [ ] Other
    - If other, specify: ______________________

**Date missed increases are applied from:**

- [ ] 1 April (default — parameters `4, 1`)
- [ ] Other
    - If other, specify day and month: ______________________

**Assumed future missed increase rate (where not yet in the GMPINC table):**

- [ ] 1.0 (no increase)
- [ ] 1.03 (3%)
- [ ] Other
    - If other, specify factor: ______________________

*Note to Coders:*
*TODO: Update `GmpGlobalModules.MissedIncreaseDateOfLeaving(...)` parameters:*
*- Parameter 1: Date to calculate up to*
*- Parameters 2 and 3: Month and day increases applied from (e.g. 4, 1 for 1 April)*
*- Parameter 4: Assumed future missed increase factor (e.g. 1.03)*
*- Parameter 5: GMP date (Date of Birth + 65 male / 60 female)*

### 6.4 GMP Standard to Actual Factor

The GmpStandardToActualFactor adjusts GMP from the standard (weekly) basis to the actual payment basis.

- [ ] Confirmed standard call applies (`GmpGlobalModules.GmpStandardToActualDateOfLeavingFactor(Parameters.DateOfLeaving, ...)`)
- [ ] Other
    - If other, describe: ______________________

*Note to Coders:*
*TODO: Confirm `GmpStandardToActualFactor` in `SetupCalculationProperties()` is correct — first parameter should typically be DOL*

---

## 7. Validations `[AtoD.cs: ValidateMemberForCalculation(), PostCalculationValidation()]`

### 7.1 Standard pre-calculation checks (Errors — stop the calc)

**Table 7.1**

| # | Check | Applies? | Notes |
|---|-------|----------|-------|
| 1 | Member's category is valid for this calculation | Yes / No | Covered in Section 1 |
| 2 | Member has a Date of Leaving recorded | Yes / No | |

### 7.2 Additional scheme-specific validations

**Note to Analysts:**
- Error: Stops the calculation at the point it is hit.
- Warning: Flags in the information log but does not prevent the calculation running.
- `ValidateMemberForCalculation`: Runs before calculation starts.
- `PostCalculationValidation`: Runs on outputs once the calculation has completed.

**Table 7.2**

| # | Check | Error or Warning? | Pre or Post Calc? | Message to display |
|---|-------|-------------------|-------------------|--------------------|
| | | | | |
| | | | | |
| | | | | |

*Note to Coders:*
*TODO: Populate `ValidateMemberForCalculation()` with any additional pre-calc checks*
*TODO: Populate `PostCalculationValidation()` with any post-calc checks, ensuring required parameters are defined*

---

## 8. Dependencies & Setup Checklist `[AtoD.cs: using statements, dependencies]`

### 8.1 Coder Checklist

*Note to Coders: Perform this checklist*

**Table 8.1**

| # | Location | TODO | Status | Notes |
|---|-----------|------|--------|-------|
| 1 | Main A>D Scheme Calc | Replace `TEMPLATEActiveToDeferredCalculation` with `[SchemeName]ActiveToDeferredCalculation` | | |
| 2 | Main A>D Scheme Calc | Update `pre97ExcessPensionElement` if not "PRE97", per Section 5.3 | | |
| 3 | ActiveToDeferredElementsSchemeModule | Complete and publish per Elements Module Spec (`AtoD-ElementsModule.md`) | | |
| 4 | Main A>D Scheme Calc | Set `ActiveToDeferredElementsSchemeModule` dependency | | |
| 5 | SalarySchemeModule | Complete and publish per Salary Scheme Module Spec (`SalarySchemeModule.md`) | | |
| 6 | Main A>D Scheme Calc | Set `SalarySchemeModule` dependency (flows through ActiveToDeferredElementsSchemeModule) | | |
| 7 | Main A>D Scheme Calc | Set service builder configuration per Section 2 (service units, method, fill type) | | |
| 8 | Aurora | Set up service tranches per Section 3 | | |
| 9 | Main A>D Scheme Calc | Set `CalculationEndDate` and `FPSCalcDate` per Sections 5.1 and 4.1 | | |
| 10 | Main A>D Scheme Calc | Set `LifetimeAllowance` global dependency | | |
| 11 | Main A>D Scheme Calc | Set GMP global dependency and configure `CalculateActiveGuaranteedMinimumPensionValues` per Section 6 | | |
| 12 | Main A>D Scheme Calc | Configure `MissedIncreaseFactor` and `GmpStandardToActualFactor` per Section 6 | | |
| 13 | Main A>D Scheme Calc | Populate `ValidateMemberForCalculation()` per Sections 1 and 7 | | |
| 14 | Main A>D Scheme Calc | Populate `PostCalculationValidation()` per Section 7 | | |
| 15 | Main A>D Scheme Calc | Set `FinalPensionableSalary` return list per Section 4.2 | | |
| 16 | Main A>D Scheme Calc | Remove or update `MySchemeServiceUnitCalculation` class per Section 2.1 | | |

---

## 9. Additional Misc Notes

Capture anything that doesn't fit the sections above — scheme quirks, known issues, or things the coder should watch out for.

>

---

## 10. Sign-Off

**Table 10**

| Role | Name | Date |
|------|------|------|
| Analyst | | |
| Coder | | |
| Reviewer | | |
