# Pension Increases (PINCs) Calculation Spec

| Field | Value |
|-------|-------|
| Scheme | |
| Author | |
| Date | |
| Version | 1.0 |
| Status | Draft / Review / Approved |

---

## How to use this template

This spec covers the `SchemePINCs` class in `PINCs.cs`, which applies annual pension increases to in-payment pension elements for pensioners and their dependants/spouses.

The key decisions are: which elements are in scope, whether a first-year pro-rate applies and under what conditions, how individual elements are rounded after increasing, and how any rounding difference on the total is distributed.

**For analysts:** Fill in every table. If you do not know the answer, insert "TBC". Sections 2 and 3 (pro-rate) are the most complex — read the guidance notes carefully.
**For coders:** Each section header shows the matching code location in `PINCs.cs`. The configurable values all live in `SetPincProRateParameters()`, `SetPincRoundingParameters()`, and the `keyOrder` list in `CalculatePinc()`.

---

## 1. Pension Elements `[PINCs.cs: CalculatePinc() — keyOrder, memberPensionHistories]`

### 1.1 Elements in Scope

List all pension element keys that will receive increases. The calculation works on all open (current) pension history elements for the member — confirm which elements are expected.

**Note to Analysts:** Element keys must match those held in Aurora pension history. Elements containing "GMP" in their key are automatically treated as GMP elements by the code for pro-rate and rounding purposes. Confirm GMP element keys follow this convention, or flag if they do not.

**Table 1.1: Pension Elements**

| Pension Element Key (per Aurora) | GMP or Excess? | Notes |
|----------------------------------|----------------|-------|
| e.g. PRE_97_EXCESS | Excess | |
| e.g. 97_TO_2005 | Excess | |
| e.g. POST_2005 | Excess | |
| e.g. PRE_88_GMP | GMP | Key must contain "GMP" |
| e.g. POST_88_GMP | GMP | Key must contain "GMP" |
| | | |
| | | |

**Note to Analysts:** If any GMP element key does not contain the string "GMP", the code will treat it as an excess element for pro-rate and rounding purposes. Flag any exceptions in the Notes column above.

*Note to Coders:*
*TODO: Confirm all elements listed above are held in Aurora pension history with the correct keys*
*TODO: If any GMP element key does not contain "GMP", override the `PincHelper.IsGMP()` method to handle the scheme-specific key convention*

### 1.2 Rounding Adjustment Priority Order

When the total of all increased elements needs to be rounded to a target precision (see Section 5), the rounding adjustment is added to the first element in this list that the member actually has in payment. Set the priority order below.

**Note to Analysts:** Place the element most appropriate to absorb small rounding differences first. Typically this is a main excess element. GMP elements should usually be lower priority as they have statutory precision requirements.

**Table 1.2: Key Order for Rounding Adjustment**

| Priority | Pension Element Key |
|----------|---------------------|
| 1 | e.g. POST_2005 |
| 2 | e.g. 97_TO_2005 |
| 3 | e.g. PRE_97_EXCESS |
| 4 | e.g. POST_88_GMP |
| 5 | e.g. PRE_88_GMP |
| 6 | |

*Note to Coders:*
*TODO: Update `keyOrder` list in `CalculatePinc()` with the keys above in priority order*

---

## 2. Pro-Rate — First Year `[PINCs.cs: SetPincProRateParameters(), ProRate.ProRateFirstYear()]`

A first-year pro-rate reduces the increase for members who have been in receipt of their pension for less than a full year at the review date. The pro-rate fraction is: **complete months from retirement date to review date ÷ 12**.

**Note to Analysts:** "First year" means the period from the member's retirement date to the increase review date is less than 12 months. After the first year, the full increase always applies — these settings only affect that first year.

### 2.1 Pro-Rate Basis

How are the months in the pro-rate fraction counted?

- [ ] Complete months only (default — `ProRateRoundingBasis.CompleteMonths`)
    - e.g. Retired 14 March, review 1 April = 0 complete months → fraction = 0/12
- [ ] Nearest months (not yet implemented in template — requires bespoke code)
    - If nearest months, describe the rounding rule: ______________________

*Note to Coders:*
*TODO: Confirm `ProRateFactory.CreatePincProRate(ProRateRoundingBasis.CompleteMonths)` is correct, or implement `ProRateNearestMonths` if required*

### 2.2 GMP Elements — First Year Pro-Rate

Should GMP elements (element keys containing "GMP") be pro-rated in the first year?

- [ ] Yes (`ProRateGMPInFirstYear = true`)
- [ ] No (`ProRateGMPInFirstYear = false`)

**Note to Analysts:** GMP statutory increases are often not pro-rated in the first year — check the scheme rules.

*Note to Coders:*
*TODO: Set `pincProRateParameters.ProRateGMPInFirstYear` accordingly in `SetPincProRateParameters()`*

### 2.3 Excess Elements — First Year Pro-Rate

Should excess (non-GMP) elements be pro-rated in the first year?

- [ ] Yes (`ProRateExcessInFirstYear = true`)
- [ ] No (`ProRateExcessInFirstYear = false`)

*Note to Coders:*
*TODO: Set `pincProRateParameters.ProRateExcessInFirstYear` accordingly in `SetPincProRateParameters()`*

---

## 3. Pro-Rate — Member Status Rules `[PINCs.cs: SetPincProRateParameters(), ProRate.MemberProRate(), ProRate.SpouseProRate()]`

Even when pro-rating applies in the first year (Section 2), the code also checks the member's previous status before applying it. These flags control whether the pro-rate applies based on where the member came from.

**Note to Analysts:** "Previous status" is the status the member held before becoming a pensioner. Sections 3.1 and 3.2 apply to the member's own pension. Section 3.3 applies to spouse/dependant pensions.

### 3.1 Member Pro-Rate — Came from Active

If the member was previously in **active** status before retiring, should the first-year pro-rate apply to their pension?

- [ ] Yes (`ProRateMemberFromActive = true`)
- [ ] No (`ProRateMemberFromActive = false`)

*Note to Coders:*
*TODO: Set `pincProRateParameters.ProRateMemberFromActive` accordingly*

### 3.2 Member Pro-Rate — Came from Deferred

If the member was previously in **deferred** status before retiring, should the first-year pro-rate apply to their pension?

- [ ] Yes (`ProRateMemberFromDeferred = true`)
- [ ] No (`ProRateMemberFromDeferred = false`)

*Note to Coders:*
*TODO: Set `pincProRateParameters.ProRateMemberFromDeferred` accordingly*

### 3.3 Spouse / Dependant Pro-Rate

The spouse pro-rate has three separate flags depending on the **original member's** previous status, and is only triggered in the first year after the member's retirement date (not the spouse's bereavement date).

**Note to Analysts:** These flags apply when running the PINC for a dependant/spouse record. The "original member" is the pensioner to whom the spouse belongs.

**Table 3.3: Spouse Pro-Rate Flags**

| Scenario | Pro-Rate Apply? | Setting |
|----------|----------------|---------|
| Original member was previously **Active** | Yes / No | `ProRateSpouseFromActive` = true / false |
| Original member was previously **Deferred** | Yes / No | `ProRateSpouseFromDeferred` = true / false |
| Spouse is within 1 year of the member's **retirement date** (member status is Pensioner) | Yes / No | `ProRateSpouseFromRetirement` = true / false |

*Note to Coders:*
*TODO: Set `pincProRateParameters.ProRateSpouseFromActive`, `ProRateSpouseFromDeferred`, and `ProRateSpouseFromRetirement` accordingly in `SetPincProRateParameters()`*

---

## 4. Element Rounding `[PINCs.cs: SetPincRoundingParameters(), PincRoundingFactory]`

After the increase is applied, each element value is rounded individually before being output.

### 4.1 Rounding Direction

How should individual element values be rounded after applying the increase?

- [ ] Round up to nearest pence (`PincRoundingBasis.IndividualElementRoundingRoundUp`)
- [ ] Round to nearest pence (`PincRoundingBasis.IndividualElementRoundingRoundNearest`)
- [ ] Round down to nearest pence (`PincRoundingBasis.IndividualElementRoundingRoundDown`)

*Note to Coders:*
*TODO: Update `PincRoundingFactory.CreatePincRounding(PincRoundingBasis.[X])` in `CalculatePinc()` to the correct basis*

### 4.2 GMP Rounding Precision

To how many pence (as a divisor) should GMP element values be rounded?

**Note to Analysts:** GMP is an annual figure and often needs to be divisible by 52 (weekly equivalent) or 12 (monthly equivalent). Common values:
- `1` = nearest penny
- `12` = divisible by 12p (nearest 12p, for monthly payment)
- `52` = divisible by 52p (nearest 52p, for weekly payment)

- [ ] 1 (nearest penny)
- [ ] 12 (divisible by 12p — monthly)
- [ ] 52 (divisible by 52p — weekly)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Set `pincRoundingParameters.GMPRoundingPence` in `SetPincRoundingParameters()`*

### 4.3 Excess Element Rounding Precision

To how many pence should excess (non-GMP) element values be rounded?

- [ ] 1 (nearest penny — default)
- [ ] 12 (divisible by 12p)
- [ ] 52 (divisible by 52p)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Set `pincRoundingParameters.ExcessRoundingPence` in `SetPincRoundingParameters()`*

---

## 5. Total Rounding `[PINCs.cs: CalculatePinc() — pincElementValues.RoundTotal()]`

After individual elements are rounded, the total pension may not round cleanly to the required precision. A final adjustment is applied to one nominated element (the first in the `keyOrder` list from Section 1.2 that the member holds) to make the total divisible by the required amount.

### 5.1 Total Rounding Precision

What precision should the **total** of all increased pension elements be divisible by?

**Note to Analysts:** This is typically 12p (for monthly payment) or 52p (for weekly payment). It is separate from the individual element rounding in Section 4.

- [ ] 1 (nearest penny — effectively no total rounding)
- [ ] 12 (divisible by 12p — monthly)
- [ ] 52 (divisible by 52p — weekly)
- [ ] Other
    - If other, specify: ______________________

*Note to Coders:*
*TODO: Update the first parameter in `pincElementValues.RoundTotal([PENCE], keyOrder)` in `CalculatePinc()`*

---

## 6. Validations `[PINCs.cs: CalculatePinc(), PincHelper.SetMemberParameters()]`

### 6.1 Standard checks (built into template)

**Table 6.1**

| # | Check | Type | Message |
|---|-------|------|---------|
| 1 | Member has no PensionDetails (retirement date cannot be determined) | Warning | "No PensionDetails for the member. Please check data tab settings in the first instance, and then check member data." |
| 2 | Member has no open pension history elements to increase | Error | "Member has no elements to increase" |

Confirm these standard checks apply:

| # | Applies? | Notes |
|---|----------|-------|
| 1 | Yes / No | |
| 2 | Yes / No | |

### 6.2 Additional scheme-specific validations

**Note to Analysts:**
- Error: Stops the calculation.
- Warning: Flags in the log but allows the calculation to continue.

**Table 6.2**

| # | Check | Error or Warning? | Message to display |
|---|-------|-------------------|--------------------|
| | | | |
| | | | |
| | | | |

*Note to Coders:*
*TODO: Add any additional validations to `CalculatePinc()` or `SetMemberParameters()` as appropriate*

---

## 7. Dependencies & Setup Checklist `[PINCs.cs: SchemePINCs class]`

### 7.1 Coder Checklist

*Note to Coders: Perform this checklist*

**Table 7.1**

| # | Location | TODO | Status | Notes |
|---|-----------|------|--------|-------|
| 1 | SchemePINCs | Rename `SchemePINCs` class to `[SchemeName]PINCs` | | |
| 2 | SchemePINCs | Set `ProRateRoundingBasis` per Section 2.1 | | |
| 3 | SchemePINCs | Set `pincProRateParameters.ProRateGMPInFirstYear` per Section 2.2 | | |
| 4 | SchemePINCs | Set `pincProRateParameters.ProRateExcessInFirstYear` per Section 2.3 | | |
| 5 | SchemePINCs | Set `pincProRateParameters.ProRateMemberFromActive` per Section 3.1 | | |
| 6 | SchemePINCs | Set `pincProRateParameters.ProRateMemberFromDeferred` per Section 3.2 | | |
| 7 | SchemePINCs | Set `pincProRateParameters.ProRateSpouseFromActive`, `ProRateSpouseFromDeferred`, `ProRateSpouseFromRetirement` per Section 3.3 | | |
| 8 | SchemePINCs | Set `PincRoundingBasis` per Section 4.1 | | |
| 9 | SchemePINCs | Set `pincRoundingParameters.GMPRoundingPence` per Section 4.2 | | |
| 10 | SchemePINCs | Set `pincRoundingParameters.ExcessRoundingPence` per Section 4.3 | | |
| 11 | SchemePINCs | Update `keyOrder` list per Section 1.2 | | |
| 12 | SchemePINCs | Update `RoundTotal([PENCE], keyOrder)` parameter per Section 5.1 | | |
| 13 | SchemePINCs | Confirm GMP element keys contain "GMP", or override `PincHelper.IsGMP()` per Section 1.1 | | |
| 14 | SchemePINCs | Add any additional validations per Section 6.2 | | |
| 15 | Aurora | Confirm all pension element keys in Section 1.1 are configured in Aurora pension history | | |

---

## 8. Additional Misc Notes

Capture anything that doesn't fit the sections above — e.g. elements that receive a nil increase, non-standard increase mechanisms, or known data quality issues with pension history.

>

---

## 9. Sign-Off

**Table 9**

| Role | Name | Date |
|------|------|------|
| Analyst | | |
| Coder | | |
| Reviewer | | |
