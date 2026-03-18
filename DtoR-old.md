# Deferred to Retired (D>R) Calculation Spec

| Field | Value |
|-------|-------|
| Scheme | |
| Author | |
| Date | |
| Version | 1.0 |
| Status | Draft / Review / Approved |

---

## How to use this template

This is a structured checklist for analysts to complete before coding begins. Each section maps directly to a coding decision in the `dtor.cs` template.

**For analysts:** Fill in every table. If you do not know the answer, please insert "TBC" to confirm this has been read and will be updated.
**For coders:** Each section header shows the matching code reference in brackets. Ctrl+F the TODO tag in `dtor.cs` to find where each decision is used. TODOs also included here.

---

## 1. Valid Categories `[dtor.cs: Validation 1]`

Which scheme categories can this calculation run for?

**Note to Analysts:** Keys must match those available and presented within the Aurora Client

| Category Key (Per Aurora) | Description |
|-------------|-------------|
| | |
| | |
| | |
| | |
| | |

*Note to Coders:*
*TODO: Populate Validation 1 with Category Keys*

---

## 2. Deferred Elements `[dtor.cs: Section G — Revaluation Classes]` 

List every deferred element key and its revaluation type. Where relevant, include assumed future revaluation (AFR) factor.

**Revaluation types:**
- **s52** — Statutory revaluation, compound. Uses assumed future reval (AFR) for years beyond latest published factors. Please do not leave ARF blank.
- **s101** — Statutory revaluation capped at CPI, compound. Uses AFR for future years. Please do not leave ARF blank.
- **Fixed** — No revaluation. Factor is always 1.
- **GMP** — GMP-specific revaluation. See section 3.

### 2.1 Revaluation Tables

**Table 2.1A**
- [ ] Insert Category or Categories: ______________________

| Deferred Element Key | NPA Male | NPA Female | NRA Male | NRA Female | Reval Type (s52 / s101 / Fixed / GMP) | AFR Rate | Revaluation End Date | Notes |
|---------------------|-------| -------| -------| -------| ---------------------------------------|---------|-------|-------|
| | | | | | | | | |
| | | | | | | | | |
| | | | | | | | | |
| | | | | | | | | |


**Table 2.1B**
- [ ] Insert Category or Categories: ______________________

| Deferred Element Key | NPA Male | NPA Female | NRA Male | NRA Female | Reval Type (s52 / s101 / Fixed / GMP) | AFR Rate | Revaluation End Date | Notes |
|---------------------|-------| -------| -------| -------| ---------------------------------------|---------|-------|-------|
| | | | | | | | | |
| | | | | | | | | |
| | | | | | | | | |
| | | | | | | | | |

**Note to Analysts:** Copy and paste the above table for the number of categories and elements required.

*Note to coders:*
*TODO: Populate Reusable Functions Scheme Module > NormalPensionAge and NormalRetirementAge*
*TODO: Populate Reusable Functions Scheme Module > RevaluationEndDate*
*TODO: Within Reval() set the RevalCalculatorFactory to the correct RevalTypes, e.g. RevalType.s52a, for each collection of application elements*

### 2.2  AFR reference values to set in Calculate

Create a table with reference key for each assumed revaluation factor listed above.

**Table 2.2A**
| Reference Key | Value | Used for |
|--------------|-------|---------|
| AFR1 | e.g. 1.03 | s52 elements |
| AFR2 | e.g. 1.025 | s101 elements |
| | | |

*Note to coders:*
*TODO: Within Factors > Decimal Reference Values set up the above ARF tables*
*TODO: Within Reval() set the decimal revaluationFactor parameter for AFR table, e.g. "AFR1"*

---

## 3. GMP `[dtor.cs: SetupCalculationProperties, GMPREVAL class]`

### 3.1 Statutory GMP Revaluation (Global Code): Statutory revaluation x Missed Increases X GMP Increments

#### 3.1.1 General Set Up

**GMP Restriction based on:**

- [ ] Total GMP at **Retirement Date** 
- [ ] Total GMP at **GMP Date** 
- [ ] Other 
    - If other, please specify: ______________________

**PRE97 Excess Element:**

Confirm the element GMP is restricted against, usually "PRE97"

- [ ] "PRE97"
- [ ] Other
    - If other, please specify: ______________________

**GMP Revaluation Basis:**

Where DOR is the same as or in the same tax year as the GMP entitlement date then complete tax years is used.
Please indicate what basis is used when DOR is in a tax year prior to the tax year of GMP entitlement.

- [ ] 6 Aprils
- [ ] Complete Tax Years

**Date GMP is calculated up to:**

- [ ] Date of Retirement
- [ ] Other 
    - If other, please specify: ______________________

*Note to Coders:*
*TODO: Populate SetUpCalculationProperties > GmpValues = GmpGlobalModules.CalculateGuaranteedMinimumPensionValues*
    *Parameter 1: Calculation End Date*
        *- 6 Aprils + Date of Retirement =  Parameters.DateOfRetirement*
        *- Complete Tax Years + Date of Retirement = Parameters.DateOfRetirement.PreviousInstanceOfDayOfYear(4,5)*
    *Parameter 10: CTY bool*
        *- 6 Aprils = false*
        *- CTY = true*
*TODO: See section 6 and PCLS Options Builder for populating gmpRestriction and .WithGMPRestriction*

#### 3.1.2 GMP revaluation formula

These are the standard GMP formulas. Confirm they apply or note differences.

> **POST88 GMP:** ((Weekly value x Reval Factor x Missed Increases x Increments) rounded to nearest penny) x 52 x Retirement Factor
> **PRE88 GMP:** ((Weekly value x Reval Factor x Increments) rounded to nearest penny) x 52 x Retirement Factor

- [ ] Confirmed
- [ ] Other Formula
    - If other, please give detail: 

#### 3.1.3 GMP Missed Increase Rules

**Date missed increases are calculated up to:**

- [ ] Date of Retirement
- [ ] Other 
    - If other, please specify: ______________________

**Date missed increases are applied from:**

- [ ] 1st April
- [ ] Other 
    - If other, please specify: ______________________

**Assummed future increases if not provided in GMPINC table yet:**

- [ ] No increase, i.e. 1.0
- [ ] 3%
- [ ] Other
    - If other, please specify: ______________________

*Note to Coders:*
*TODO: Populate Reusable Functions Scheme Module > GMPMissedIncrease*
*TODO: Populate SetUpCalculationProperties > GmpValues = GmpGlobalModules.CalculateGuaranteedMinimumPensionValues*
    *Parameters 3 and 4: Date Missed Increases Applied From*
       *- 1 April = 1, 4*
    *Parameter 5: Assumed Future Missed Increase Rate*
        *- 3% = 1.03*

#### 3.1.4 GMP Increments Rules (s148)

**Date increments are calculated up to:**

- [ ] Date of Retirement
- [ ] Other 
    - If other, please specify: ______________________

**Are increments capped at 260 weeks?**

- [ ] Yes
- [ ] No

*Note to Coders:*
*TODO: Populate Reusable Functions Scheme Module > GmpIncrements*
*TODO: Populate SetUpCalculationProperties > GmpValues = GmpGlobalModules.CalculateGuaranteedMinimumPensionValues*
    *Parameter 2: Capped At 260 Weeks Bool*
        *- Capped = true*
        *- Uncapped = false*

#### 3.1.5 GMP Increases Table

Please provide a copy of GMPINC.csv which is set out with the Date and Value of increase for each applicable year.

**Table 3.1.5**
| Setting | Value |
|---------|-------|
| Factor table Key name in Calculate | GMPINC |
| Uploaded? | Yes / No / TBC |

*Note to Coders:*
*TODO: Upload GMPINC to Factors > Pension Increases*

### 3.2 GMP Deferred Element Scheme Revaluation

> Some schemes will calculate statutory GMP for Pre and Post 88 periods, as well as revalue individual Deferred GMP Elements using Scheme rules.
> The excess, when comparing to the GMP rules above, goes to the Pre97 Excess element.

#### 3.2.1 Alternative Late Retirement Factors
Does the Scheme calculate GMP using any other basis, such as application of Scheme LRF factors or underpin check?

- [ ] Yes 
    - If yes, please fill out Deferred GMP Element table below
- [ ] No
    - If no, move on to Section 4.

**Example Table**
| Deferred GMP Element Key (per Aurora) | Revaluation Rate Type | RevalEnd Date Males | Reval End Date Females | Late Retirement Rules | LRF Table Male | LRF Table Female |
|-----------|-----------|-----------|-----------|---------------------|-----------|-----------|
| e.g.Pre88 GMP | e.g. Fixed | e.g. Date reaches age 65 | e.g. Date reaches age  60 | e.g. Higher of Scheme LRF from Reval End to DoR and GMP Missed Increases and Increments | e.g. LRF 1 | e.g. LRF 1
| e.g. Post88 GMP (88 to 90) | e.g. 
| e.g. Post88 GMP (90 to 94) |
| e.g. Post88 GMP (94 to 97) |


**Table 3.2.1A: GMP LRFs**
| Deferred GMP Element Key (per Aurora) | Revaluation Rate Type | RevalEnd Date Males | Reval End Date Females | Late Retirement Rules | LRF Table Male | LRF Table Female |
|-----------|-----------|-----------|-----------|---------------------|-----------|-----------|
| | | | | | | |

*Note to Coders:*
*TODO: Within Reval() set up Scheme specific, GMP deferred element revaluation logic*

### 3.3 Additional Notes

Any additional notes, regarding non-standard GMP?

>

---

## 4. Retirement Factors `[dtor.cs: Validation 2, CallReusableFunctions]`

### 4.1 Basis
Are retirement factor periods calculated on an Age Attained or Term Basis?

**Note To Analysts:**
> Age Attained: If someone's birthday was 19th March 2025 and we wanted the period to 14th May 2025, that would be 1 month (as they don't attain 2 full months until 19th May)
> Term Basis: If someone's birthday was 19th March 2025 and we wanted the period to 14th May 2025, that would be 2 months (comparing March (03) to May (05))

- [ ] Age Attained Basis
- [ ] Term Basis

*Note to Coders:*
*TODO: Populate Reusable Functions Scheme Module > GetSchemeFactor > RetirementFactorBuilder*
    *- Can be set up for just ERFs or just LRFs or using both. Template assumes both and will use the Application Dates and Tables below.*
    *- Add .WithAgeAttainedBasis if applicable*

### 4.2 Early Retirement Factors (ERF)

#### 4.2.1 ERF Application Detail

Please populate the table below with ERFs applicable for deferred elements.

**Note To Analysts:**
> "Application Date" is the date from which factors are applied FROM.
    - e.g. If a member gets ERFs prior to NPA of 65 and at DOR they are 60, they are 5 years early. ERF Application Date = NPD (Date reaches NPA)

**Table 4.2.1A: Scheme ERFs**
- [ ] Insert Category or Categories: ______________________

| Deferred Element Key | ERF App Date Male | ERF App Date Female | ERF Table Male | ERF Table Female | Notes |
|---------------------|-------|-------|-------|-------|------------------|
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 

**Table 4.2.1B: Scheme ERFs**
- [ ] Insert Category or Categories: ______________________

| Deferred Element Key | ERF App Date Male | ERF App Date Female | ERF Table Male | ERF Table Female | Notes |
|---------------------|-------|-------|-------|-------|------------------|
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 

**Note To Analysts:** Copy and paste the above tables for the number of categories and elements required.

*Note to Coders:*
*Populate Reusable Functions Scheme Module > ERF Application Date and ERF Table*

#### 4.2.2 ERF Application Table Upload

Confirm all tables Keys listed above have been provided to coders or uploaded

**Table 4.2.2: Scheme ERFs**
| Table Key (per Aurora) | Uploaded? (Y/N/TBC) |
|---------|-------|
| e.g. ERF1 | |
| e.g. ERF2F | |
| e.g. ERF2M | |

**Note To Analysts:** Attach or reference the ERF table .csv. Format needs to be Years, Months (can be 0), Value

*Note to Coders:*
*TODO: Upload table .csv to Factors*

### 4.3 Late Retirement Factors (LRF)

#### 4.3.1 LRF Application Detail

Please populate the table below with ERFs applicable for deferred elements.

**Note To Analysts:**
> "Application Date" is the date from which factors are applied FROM.
    - e.g. If a member gets LRFs after age 65 and at DOR they are 67, they are 2 years late. LRF Application Date = Date reaches age 65

**Table 4.3.1A**
- [ ] Insert Category or Categories: ______________________

| Deferred Element Key | LRF App Date Male | LRF App Date Female | LRF Table Male | LRF Table Female | Notes |
|---------------------|-------|-------|-------|-------|------------------|
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 

**Table 4.3.1B**
- [ ] Insert Category or Categories: ______________________

| Deferred Element Key | LRF App Date Male | LRF App Date Female | LRF Table Male | LRF Table Female | Notes |
|---------------------|-------|-------|-------|-------|------------------|
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 
| | | | | | | 

**Note To Analysts:** Copy and paste the above tables for the number of categories and elements required.

*Note to Coders:*
*Populate Reusable Functions Scheme Module > LRF Application Date and LRF Table*

#### 4.3.2 LRF Application Table Upload

Confirm all tables Keys listed above have been provided to coders or uploaded

**Table 4.3.2: Scheme LRFs**
| Table Key (per Aurora) | Uploaded? (Y/N/TBC) |
|---------|-------|
| e.g. LRF1 | |
| e.g. LRF2F | |
| e.g. LRF2M | |

**Note To Analysts**: Attach or reference the LRF table .csv. Format needs to be Years, Months (can be 0), Value

*Note to Coders:*
*TODO: Upload table .csv to Factors*

### 4.4 Additional Notes

If all elements follow the standard formula, write "Standard". Otherwise describe differences (e.g. underpin, PIA, alternative revaluation mechanisms, underpins).

>

---

## 5. Pension Element Roll-up `[dtor.cs: Step 3b — PreCommutationPensionElements]`

### 5.1 Pension Element Dictionary

How do deferred elements group into pension elements for commutation?

***Note To Analysts**: Please use Aurora Keys

**Table 5.1A**
- [ ] Insert Category or Categories: ______________________

| Pension Element Key (per Aurora) | Built from (Deferred Element Key(s), per Aurora) |
|----------------|-----------------------------------|
| e.g. PRE97| e.g. PRE97INCXS, NONINCXS, PRE88GMP, POST88GMP |
| e.g. POST97| e.g. POST97, POST00 |
| | |
| | |
| | |
| | |

**Note To Analysts:** Copy and paste the above tables for the number of categories and elements required.

*Note to Coders:*
*TODO: Populate PCLSandCommutationSchemeModule > SetElements*

### 5.2 Additional Notes

Note anything non-standard about the roll-up from deferred to pension elements?

>

---

## 6. Commutation / PCLS `[dtor.cs: Step 4 — PensionOptionBuilder]`

### 6.1 Commutation factors

Please allocate CFs against all relevant PENSION Element Keys. 

Single CF?
- [ ] Yes
    - If yes, please specify the single CF table used: ______________________
- [ ] No, multiple CF tables used
    - If No, please fill out table below:

**Table 6.1A**
- [ ] Insert Category or Categories: ______________________
- [ ] Applicable Male
- [ ] Applicable Female

| Commutation Factor Table | Deferred Elements Applicable | Notes |
|----------------|-------------------|-------|
| e.g. CF1 | PRE97, POST97, PRE88GMP, POST88GMP| e.g. Male and Female|
| | | |
| | | |
| | | |
| | | |

**Note To Analysts:** Copy and paste the above tables for the number of categories and elements required.

*Note to Coders:*
*TODO: Populate PCLSandCommutationSchemeModule > CommutationFactorsForElementsList*

### 6.2 Commutation Factor Table Upload

**Table 6.2**
| Table Key (per Aurora) | Uploaded? (Y/N/TBC) |
|---------|-------|
| e.g. CF1 | |
| e.g. CF2F | |
| e.g. CF2M | |

**Note To Analysts:** Attach or reference the CF table .csv. Format needs to be Age (Years or Years and Months), Value

*Note to Coders:*
*TODO: Upload table .csv to Factors*

### 6.3 PCLS Options Builder

Please select from the required parameters to build the PCLS pensionOptionCalculator:

- [X] .WithCommucationFactors (Required for all schemes)
- [X] .WithGMPRestriction (Required for all schemes)
- [ ] .Ordered (Optional: Do not select if commutation is proportional)
    - If Ordered, fill out "Order for Commutation" below
- [X] .WithDCFund (Optional: Defaults to assuming scheme has members with potential DCFund. De-select if not applicable.)
- [ ] .WithSpouses (Optional: Defaults to 50%, so only select here if another percentage is used)
    - If .WithSpouses and not 50%, specify percentage: ______
- [ ] .WithSpousesGMPMinimumOption (Optional: Defaults to GMP have 50% spouses except Female Pre88GMP which has 0. Select this if another method applied)
    - [ ] NilPre88GMP (all GMP nil)
    - [ ] Fifty Percent (all GMP 50%)
    - [ ] Other
        - If other, please specify how to treat GMP for spouses: ______________________
- [ ] .WithPIE (Optional: Select if PIE is applicable and complete "For PIE only" below)
- [ ] . With DecimalPlaces (Optional: Defaults to 2 d.p. so only select this is a different number of d.p required)
    - Indicate decimal places to use, if not 2, here: 

*Note to Coders: Populate pensionOptionCalculator*
*TODO: .WithGMPRestriction: Populate gmpRestriction and apply PRE97 Excess Element, per section 3 of this spec.*
*TODO: If .Ordered: Populate OrderForCommutation (see below)*
*TODO: If .WithSpouses or .WithSpousesGMPMinimumOption apply appropriate parameters*
*TODO: If .WithPIE, see below and populate Interpolation and Application parameters*
*TODO: If .WithDecimalPlaces insert the number of decimal places (not required if 2)*

#### 6.3.1 Order for Commutation (if selected above)

Please insert Pension ELements Keys in priority order.

**Table 6.3.1.A**
- [ ] Category or Categories: ______________________

| Priority | Pension Element |
|----------|----------------|
| 1 | |
| 2 | |
| 3 | |
| 4 | |
| 5 | |

**Note To Analysts:** Copy and paste the above tables for the number of categories and elements required.

#### 6.3.2 For PIE only (if selected above)

PIE Factor Interpolation
- [ ] YearMonth
- [ ] YearDay
- [ ] Other
    - If other, please specify: ______________________

PIE Application
- [ ] Apply After Commutation
- [ ] Other
    - If other, please specify instructions: ______________________

### 6.4 Addiional Notes

Please note anything non-standard about commutation.

>

---

## 7. Validations `[dtor.cs: Sections D.1 and E]`

### 7.1 Standard pre-calculation checks (Errors — stop the calc)

These are built into the template. Confirm they apply.

**Table 7.1**
| # | Check | Applies? | Notes |
|---|-------|----------|-------|
| 1 | Check that the Member's category is a valid category for the calculation | Yes / No | |
| 2 | Check that the retirement date is not earlier than the earliest ERF or later than the latest LRF | Yes / No | |
| 3 | Member has either a date of leaving or date terminated employment | Yes / No | |

### 7.2 Standard post-calculation checks (Warnings — calc still runs)

**Table 7.2**
| # | Check | Applies? | Notes |
|---|-------|----------|-------|
| 4 | Capital value <= £30,000 (trivial commutation) | Yes / No | |
| 5 | Maximum cash is negative (GMP not covered) | Yes / No | |
| 6 | External fund values found (AVC check) | Yes / No | |

### 7.3 Additional scheme-specific validations

**Notes to Analysts:** 
    - Error: Will stop the calculation at the point it meets the error. 
    - Warning: Will flag in the information log on a calculation but will not prevent it running.
    - ValidateMemberForCalculation: Checks performed before calculation is started
    - PostCalculationValidation: Checks performed on outputs, once calculation has run

**Table 7.2**
| # | Check | Error or Warning? | Pre or Post Calc Validation? | Message to display | 
|---|-------|-------------------|-------------------|----------------------------|
| | | | |
| | | | |
| | | | |

*Note to Coders:*
*TODO: Populate ValidateMemberForCalculation*
*TODO: Populate PostCalculationValidation*

---

## 8. Dependencies & Setup Checklist `[dtor.cs: using statements, dependencies]`

### 8.1 Coder Checklist

*Note to Coders: Perform this Checklist*

**Table 8.1**
| # | Location | TODO | Status | Notes |
|---|-----------|--------|-------|---------------------|
| 1 | Main D>R Scheme Calc | Set "RevaluationBase" and "Revaluation" dependencies | | |
| 2 | Main D>R Scheme Calc | Update "TEMPLATEDeferredToRetiredCalculation" with [SchemeName]DeferredToRetiredCalculation" | | |
| 3 | ElementValuesSchemeModule | Set up module per instructions in Template (generic) and publish | | |
| 4 | Main D>R Scheme Calc | Set "ElementValuesSchemeModule" dependency | | |
| 5 | PCLSandCommutationSchemeModule | Populate SetElements per section 5 and CommutationFactorsForElementsList per section 6 and publish | | |
| 6 | Main D>R Scheme Calc | Set "PCLSandCommutationSchemeModule" dependency | | |
| 7 | Main D>R Scheme Calc | gmpRestriction: Make sure correct restriction called, per section 3 | | |
| 8 | Main D>R Scheme Calc | Set "PensionOption" dependency | | |
| 9 | Main D>R Scheme Calc | Populate pensionOptionCalculator | | Per section 6 instructions
| 10 | Factors | Upload provided GMPINC file | | |
| 11 | Main D>R Scheme Calc | GMPValues: Set parameters per section 3 | | |
| 12 | Main D>R Scheme Calc | Set Validation 1 per section section 1 and remaining ValidateMemberForCalculation/ PostCalculationValidation per secion 7| | |
| 13 | Factors | Upload ERF and LRF files | | |
| 14 | Main D>R Scheme Calc | Replace "TemplateElementBase" with [SchemeName]ElementBase | | |
| 15 | ReusableFunctionsSchemeModule | Set up module per instructions in Template (generic), plus bespoke instructions per sections 2, 3 and 4 and publish | | |
| 17 | Main D>R Scheme Calc | Set "ReusableFunctionsSchemeModule" dependency | | |
| 18 | Factors > Decimal Reference Values | Set AFR Tables, per section 2 | | |
| 19 | Main D>R Scheme Calc | Follow through TODOs in Reval() section of template, using information is section 2 and section 3 - GMP Deferred Element Scheme Revaluation | | |

---

## 9. Additional Misc Notes

Capture anything that doesn't fit the sections above — scheme quirks, known issues, things the coder should watch out for.
Please check Additional Notes sections available above and insert comments there, should they relate to any of those sections.

>

---

## 10. Sign-Off

**Table 10**
| Role | Name | Date |
|------|------|------|
| Analyst | | |
| Coder | | |
| Reviewer | | |