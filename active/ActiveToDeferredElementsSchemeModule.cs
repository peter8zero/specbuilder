////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
                // TEMPLATE GUIDANCE

                // All notes are helpful, but use TODO to determine main code changes required on set up.
                    // Note: This code has no "TODO" sections and can be published as it is.

                // Set Class: Make sure this is the same as the name of the module. 
                    // > When you assign a dependency in the main code, you select the module name (as seen in the Module contents page). 
                    // > However, When you call a method from the module in your code, you use the class name (below).
                        // > As such, it is easy to remember which class to call if it has the same name as the module/ dependency.

//////////////////////////////////////// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

// TODO: Assign dependency: ServiceBuilder to allow use of parameter type: List<ServiceHistoryOverTrancheWithDuration>

using NodaTime.Extensions; 
using System; 
using System.Text;
public class ActiveToDeferredElementsSchemeModule 
{

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
                // SET FPS BASIS FOR DEFERRED ELEMENTS
                    // > The dictionary below assigns deferred element keys to the relevant FPS basis.
                    // > Deferred elements and their FPS are defined in the calc spec.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

public static Dictionary<string, string[]> FPSBasisForDeferredElements(Member member)
    {    
        return new Dictionary<string, string[]>
            {
                { "FPS1", new string[] {"PRE97", "POST97", "POST05", "POST09", "POST14", "POST20"}}, // TODO: Assign service tranche keys to FPS basis. Note: "PRE88GMP","POST88GMP" roll into PRE97 within the service history, so are not required here.
                //{ "FPS2", new string[] {"POST06","POST09"} }, // EXTRA INFO: Examples of additional lines
                //{ "FPS3", new string[] {"PR97INCXS","POST97"}} 
            };
    }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
                // HASHSET
                    // >  Values can be stored in a HASHSET. It is changable, so values can be added and removed.
                        // > loggedElements is a HASHSET which stores strings of text (i.e. Element Keys) and is set up below to start empty.
                        // > A deferred element key is added everytime "loggedElements.Add(elementIdentifier);" is called below.
                        // > This Set is checked each time we ask the code to log information. If they key is not in the set, information is logged and the Key added to the Set.
                        // > If the Key is in the Set, information is not logged; this helps prevent duplicate logs where code is interated through more than once.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

private static HashSet<string> loggedElements = new HashSet<string>(); 

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
                // CONVERT SERVICE TRANCHE DURATIONS INTO DEFERRED ELEMENT KEYS AND VALUES
                    // > The calculation below will use the service builder outputs from the main Scheme A>R code (serviceHistoryOverTranchesWithDuration).
                    // > It will convert the service period into a deferred element value, as at service end date (usually DOL for an A>D and DOR if and A>R calc).

                    // INPUTS:
                        // > FPS values as calculated in SalarySchemeModule
                        // > Transfer in values held on the member record in Aurora
                        // > Accrual nominators and denominators as set in Aurora (see main Scheme A>R code (Section D.1.2))
                        // > Service durations from each tranche as input via the serviceHistoryOverTrancheWithDurations parameter: We assign the service builder outputs to this parameter

                    // OUTPUT: 
                        // > Initial deferred element value associated with each service tranche
                        // > Later we can adjust these values to separate out GMP or any other, bespoke requirements
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

public static ActiveToDeferredElementValue CalculateServiceTrancheDeferredElement (Member member, 
                                                                                string trancheKey, 
                                                                                List<ServiceHistoryOverTrancheWithDuration> serviceHistoryOverTrancheWithDurations, 
                                                                                CategoryReferenceData categoryReferenceData,
                                                                                LocalDate FPSCalcDate) 
    {
        // TODO: Create the SalarySchemeModule following the instructions within. Publish and assign as a dependency here. 

        //// PART 1: Pull through FPS definitions from the Salary Scheme module, assign decimal values to them and allocate the correct FPS to the relevant service tranches.

            // 1a: Pull FPS values from SalarySchemeModule
        decimal FPS1Value = SalarySchemeModule.GetFPS1(member, FPSCalcDate).Amount; // TODO: Call correct FPS[x] code from Salary Scheme Module for all potential FPSValues needed. 
        decimal FPS2Value = SalarySchemeModule.GetFPS2(member, FPSCalcDate).Amount; // TODO: Add FPS2Value, FPS3Value, etc if needed.

            // 1b: Call the "FPSBasisForDeferredElements" dictionary which assigns which FPS, e.g "FPS1", a service tranche (later the deferred element) uses.
        var fpsToUse = FPSBasisForDeferredElements(member).Where(x=>x.Value.Contains(trancheKey));

            Log.Debug(fpsToUse, "FPSToUse"); // EXTRA INFO: Debug to log what FPS each element key has used.

            // 1c: Assign FPSValues to FPSKeys, e.g. Assign the "FPS1Value" decimal in instruction 1a. to "FPS1" name in instruction 1b.
        decimal finalPensionableSalaryToUse = 0; // EXTRA INFO: Sets an initial salary of 0. We can then cycle through a "foreach" statement, which updates the value from 0, to the correct "FPS[x]Value".
  
        foreach (var kvp in fpsToUse) // EXTRA INFO: Reviews each kvp (key-value pair) in "FPSBasisForDeferredElements", e.g. Key String: "POST97", Value String: "FPS1".
            {
                switch (kvp.Key) // EXTRA INFO: Cycles through each Key, checks the "FPS[x]" string value assigned to it, and assigns the relevant decimal value.
                    {
                        case "FPS1":
                            finalPensionableSalaryToUse = FPS1Value;
                            break;
                        case "FPS2":
                            finalPensionableSalaryToUse = FPS2Value;
                            break;
                        
                        // TODO: Assign relevant FPS values. i.e. Add "FPS3" and "FPS3Value", etc. Remove FPS2 if only FPS1.

                        default: // EXTRA INFO: Handles the case where the key does not match any known keys - currently set to do nothing as all service tranches must be assigned an FPS key.
                            break;
                    }
            }

        //// PART 2: Pull through Service Tranche Durations from the service builder, via the "List<ServiceHistoryOverTrancheWithDuration>" parameter.

        var tranche = categoryReferenceData.ServiceTranches.Single(x => x.Key == trancheKey); // EXTRA INFO: Identify an individual tranche as each tranche set up in Aurora with a unique key.
        decimal trancheServiceDuration = serviceHistoryOverTrancheWithDurations.Where(x => x.Tranche.Key == trancheKey).Sum(x=>x.PartTimeAdjustedDurationAmount); // EXTRA INFO: Use that same key, to extract each tranche's service duration from the Service Builder output (serviceHistoryOverTrancheWithDurations).

        //// PART 3: Look up whether there is any TVIN service (Membership -> External Benefits -> HistoricBenefitElements) and add TVINs to relevant tranche.
            // TODO: This section may not be applicable if the scheme has no TVIN service. If left as is, it won't have any impact on the calculation.

        if (trancheKey == "PRE97") // TODO: Set pre97 deferred element key if not "PRE97".
        {
        var TVINServiceInYears = TVINService(member, "PRE97ADYRS"); // TODO: Update "PRE97ADYRS" to correct Aurora reference for PRE97 TVIN years.
        var TVINServiceInDays = TVINService(member, "PRE97ADDAY"); // TODO: Update "PRE97ADDAY" to correct Aurora reference for PRE97 TVIN days.

        decimal totalTvinService = TVINServiceInYears + TVINServiceInDays/365m; // TODO: This calculates total TVIN Service in years, as a decimal. Update to /12 if TVINService is returned with months.
        trancheServiceDuration += totalTvinService; // EXTRA INFO: adds the TVIN service to the total tranche duration.
        }

        if (trancheKey == "POST97") // TODO: Set post97 deferred element key if not "POST97".
        {
        var TVINServiceInYears = TVINService(member, "PST97ADYRS"); // TODO: Update "PRE97ADYRS" to correct Aurora reference for POST97 TVIN years.
        var TVINServiceInDays = TVINService(member, "PST97ADDAY"); // TODO: Update "PRE97ADYRS" to correct Aurora reference for POST97 TVIN days.
        
        decimal totalTvinService = TVINServiceInYears + TVINServiceInDays/365m; // TODO: This calculates total TVIN Service in years, as a decimal. Update to /12 if TVINService is returned with months.
        trancheServiceDuration += totalTvinService;
        }

        //// PART 4: Convert final service tranche durations, inclusive of TVIN service periods, into a deferred element value, as at DOL (DOR if A>R).
            
            // 4a: Service tranche value = Service tranche duration x final pensionable salary x accrual (e.g. 1/60). Sets to 0, if no positive pension value calculated.
        var serviceTrancheValue = Max(((trancheServiceDuration * 
                            finalPensionableSalaryToUse * 
                            tranche.AccrualNominator / 
                            tranche.AccrualDenominator)), 0);

            // 4b: TODO: If the serviceTrancheValue needs to be futher adjusted (e.g. uplifted to NRD, or some other bespoke rule, logic can be added here.)

            // 4c: Set deferred element values for member and spouse.
        var deferredElementValue = serviceTrancheValue;  

        var spouseDeferredElementValue = deferredElementValue / 2; // TODO: Assumes spouse values are 50% of standard. Adjust as appropriate.

        //// PART 5: INFO LOGS: All of the below is just taking information from parts 1-4 and creating an information log for Aurora calc outputs. No new calculations performed.
   
                        var TVINForMessagePRE97 = 
                            TVINService(member, "PRE97ADYRS") + (TVINService(member, "PRE97ADDAY") / 365m); // change to / 12 if months

                        var TVINForMessagePOST97 = 
                            TVINService(member, "PST97ADYRS") + (TVINService(member, "PST97ADDAY") / 365m); // change to / 12 if months

                        var TotalTVINService = TVINService(member, "PST97ADYRS") + TVINService(member, "PST97ADDAY") + TVINService(member, "PRE97ADYRS") + TVINService(member, "PRE97ADDAY");
                        
                        string trancheCalcMessage = 

                            trancheKey == "PRE97" ?

                            $"Tranche Key: {trancheKey}; " +
                            $"TVIN Service (Years and Days): { GetTVINPeriod(TVINForMessagePRE97)}; " +
                            $"TVIN Service (decimal): {TVINForMessagePRE97}; " +

                            $"Accrued Service (Years and Days): {GetPeriod(trancheServiceDuration - TVINForMessagePRE97)}; " +
                            $"Accrued Service (decimal): {(trancheServiceDuration - TVINForMessagePRE97)}; " +

                            $"Service Duration After Breaks and Part Time Adjustments (Period): {GetPeriod(trancheServiceDuration)}; " +
                            $"Service Duration After Breaks and Part Time Adjustments (Decimal): {trancheServiceDuration}; " +

                            $"Final Pensionable Salary To Use: {finalPensionableSalaryToUse}; " +
                            $"Accrual: {tranche.AccrualNominator / tranche.AccrualDenominator}; " +
                            $"Tranche Value: {deferredElementValue}; " :

                            trancheKey == "POST97"?

                            $"Tranche Key: {trancheKey}; " +
                            $"TVIN Service (Years and Days): { GetTVINPeriod(TVINForMessagePOST97)}; " +
                            $"TVIN Service (decimal): {TVINForMessagePOST97}; " +

                            $"Accrued Service (Years and Days): {GetPeriod(trancheServiceDuration - TVINForMessagePOST97)}; " +
                            $"Accrued Service (decimal): {(trancheServiceDuration - TVINForMessagePOST97)}; " +

                            $"Service Duration After Breaks and Part Time Adjustments (Period): {GetPeriod(trancheServiceDuration)}; " +
                            $"Service Duration After Breaks and Part Time Adjustments (Decimal): {trancheServiceDuration}; " +

                            $"Final Pensionable Salary To Use: {finalPensionableSalaryToUse}; " +
                            $"Accrual: {tranche.AccrualNominator / tranche.AccrualDenominator}; " +
                            $"Tranche Value: {deferredElementValue}; " :

                            $"Tranche Key: {trancheKey}; " +
                            $"Service Duration After Breaks and Part Time Adjustments (Period): {GetPeriod(trancheServiceDuration)}; " +
                            $"Service Duration After Breaks and Part Time Adjustments (Decimal): {trancheServiceDuration}; " +

                            $"Final Pensionable Salary To Use: {finalPensionableSalaryToUse}; " +
                            $"Accrual: {tranche.AccrualNominator / tranche.AccrualDenominator}; " +
                            $"Tranche Value: {deferredElementValue}; " ;

                        string elementIdentifier = trancheKey;
                        if (!loggedElements.Contains(elementIdentifier) && deferredElementValue > 0.0m)
                        {
                            Log.Information(trancheCalcMessage);
                            loggedElements.Add(elementIdentifier);
                        } 

        //// PART 6: Produce Final Deferred Element Values in the "ActiveToDeferredElementValue" format, which requires a Key, Value, SpouseValue and whether it is notional.
            // EXTRA INFO: Values marked as notional can be shown in Aurora for information purposes, without impacting final PCLS calcs. Generally, elements are not notional and are marked as "false".
        return new ActiveToDeferredElementValue 
        {
            Key = trancheKey,
            Value = deferredElementValue.Round(),
            SpouseValue = spouseDeferredElementValue.Round(2), // TODO: Set appropriate rounding for Scheme. This is to the penny.
            Notional = false
        };
    }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
                // SPLIT OUT GMP VALUES INTO THEIR OWN DEFERRED ELEMENT OR PERFORM OTHER AMENDMENTS TO DEFERRED ELEMENT VALUES
                    // > Purpose: The pre97 service tranche will roll up into a PRE97 Excess deferred element (often named PRE97, or PR97INCXS).
                        // > The service tranche will initially contain PRE88 and POST88 GMP values, as those service dates fall within the PRE97 service tranche.
                        // > Ultimately, these need to be separated out into their own deferred elements, as they'll have their own statutory revaluation method and retirement factors applied.

                        // STEPS:
                            // Create a method "CalculateDeferredElementValuesIncGMP" to return final deferred elements and put them into a list.
                            // Within "CalculateDeferredElementValuesIncGMP" call method "AdjustDeferredElementsForGMP".
                            // Within "AdjustDeferredElementsForGMP", identify the Pre97 deferred element key (e.g. "PRE97"). 
                            // Set the total GMP value to be deducted from the Pre97 deferred element and assigned to separate element keys.
                            // Create new elements with the correct GMP deferred element codes and applicable GMP values.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

public static List<ActiveToDeferredElementValue> CalculateDeferredElementValuesIncGMP(Member member, 
                                                                                LocalDate FPSCalcDate, // EXTRA INFO: See Salary Scheme Module for explanation of this parameter
                                                                                GmpValues GmpValues,
                                                                                CategoryReferenceData categoryReferenceData,
                                                                                List<ServiceHistoryOverTrancheWithDuration> serviceHistoryOverTrancheWithDurations) 
    {
        var elementValues = new List <ActiveToDeferredElementValue>();
            elementValues.AddRange(AdjustDeferredElementsForGMP(member, FPSCalcDate, GmpValues, categoryReferenceData, serviceHistoryOverTrancheWithDurations)); 

        return elementValues; // EXTRA INFO: This is the final list of deferred element Keys and Values that your main active scheme calc will return from this module.
    }    
  
public static IEnumerable<ActiveToDeferredElementValue> AdjustDeferredElementsForGMP(Member member, 
                                                                                LocalDate FPSCalcDate,
                                                                                GmpValues GmpValues,
                                                                                CategoryReferenceData categoryReferenceData,
                                                                                List<ServiceHistoryOverTrancheWithDuration> serviceHistoryOverTrancheWithDurations)                                                                                                                                                   
    {
    // PRE-CALC: Creates an instance of the ActiveToDeferred Element Values list, which will be added to with the key and value of each service tranche
    var elementValues = new List<ActiveToDeferredElementValue>();

    // TRANCHE ADJUSTMENTS
    foreach(var tranche in categoryReferenceData.ServiceTranches) // EXTRA INFO: Identifies service tranches per Aurora and looks at each individual tranche
        {
            // BRING IN INITIAL DEFERRED ELEMENT VALUES
            var serviceTrancheDeferredElement = CalculateServiceTrancheDeferredElement(member, tranche.Key, serviceHistoryOverTrancheWithDurations, categoryReferenceData, FPSCalcDate);
        
            // SET GMP VALUES - to deduct from Pre97 element and add to separate GMP elements.

            var gmpDeduction = 0m; // EXTRA INFO: This initiates an instance of gmpDeduction with a default value of 0. 
                                    // The "if" statement below, then allows for "gmpDeduction" to be overriden in a specific circumstance.
                                    // In this case, in the circumstance that the service tranche key is "PRE97", gmpDeduction is overriden to be the total pre and post88 deduction.

            var gmpSpouseDeduction = 0m; // EXTRA INFO: Initiates an instance of the spouse GMP deduction, since each deferred element has a Key, Value and Spouse Value to adjust correctly.

            if (tranche.Key == "PRE97") // TODO: Identify the Pre97 deferred element key (e.g. "PRE97"). 
                {
                    var pre88deduction = GmpValues.Pre88GmpAtDateOfRetirement; // EXTRA INFO: Looks at your GmpValues (as set in the main Scheme code > SetupCalculationProperties(); > GmpValues), specifically the Pre88 value within the results
                    var post88deduction = GmpValues.Post88GmpAtDateOfRetirement; // EXTRA INFO: As above, for Post88

                    gmpDeduction = pre88deduction + post88deduction; // EXTRA INFO: Overrides "gmpDeduction = 0m;" where "if" statement is true. Sets gmpDeduction to value of Gmp at retirement
                    gmpSpouseDeduction = gmpDeduction / 2; // EXTRA INFO: Overrides "gmpSpouseDeduction = 0m;" where "if" statement is true. - Sets gmpSpouseDeduction to value of Gmp at retirement / 2
                    
                    elementValues.Add(new ActiveToDeferredElementValue("PRE88GMP", pre88deduction, pre88deduction / 2, false)); // EXTRA INFO: Creates a new deferred element called "PRE88GMP" set at the correct GmpValue at retirement
                    elementValues.Add(new ActiveToDeferredElementValue("POST88GMP", post88deduction, post88deduction / 2, false)); // EXTRA INFO: As above for "POST88GMP". // TODO: Set GMP deferred element codes.
                }

        // PERFORM CALC ADJUSTMENT: The below takes the assigned GMP deductions above and applies them to the deferred element with key "PRE97", removing GMP from the element and spouse value.
            if (tranche.Key == "PRE97") // TODO: Identify the Pre97 deferred element key (e.g. "PRE97"). 
            {
                serviceTrancheDeferredElement.Value -= gmpDeduction;
                serviceTrancheDeferredElement.SpouseValue -= gmpSpouseDeduction;
            }
        
        // INFO LOGS - Returns strings of information for each deferred element to show final deferred element values for use in the Active calcs (prior to retirement factors).
        var sb = new StringBuilder();
        sb.AppendLine($"Key: {tranche.Key}. Deferred Element Value Before Retirement: {serviceTrancheDeferredElement.Value}. Spouse Value Before Retirement: {serviceTrancheDeferredElement.SpouseValue}");

        if (serviceTrancheDeferredElement.Value > 0.0m)
        {
            Log.Information(sb.ToString());
            sb.Clear();
        }

        // CREATE DEFERRED ELEMENT LIST - Adds each calculated element value to the outputs for deferred elements at date of leaving
            elementValues.Add(serviceTrancheDeferredElement);
        }

        // RETURN COMPLETED LIST
        return elementValues;
    
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
///// ADDITIONAL FUNCTIONS FOR PRODUCING ACTIVE TO DEFERRED ELEMENT RESULTS
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

//// DISPLAY 1: Element Order
public static List <string> elementDisplayOrder() // TODO: Write the order you want deferred element results displayed in, below
    {
        return new List <string> {  
                                    "PRE97",
                                    "POST97",
                                    "POST05",
                                    "POST09",
                                    "POST14",
                                    "POST20"};
    }

//// DISPLAY 2: Service Periods: Used for Log.Information to put decimal period into a readable Years and Days. 
    // TODO: For months, can edit to *12 not 365
public static Period GetPeriod(decimal amount) 
    {
        int years = (int)amount; // Takes the whole number for years
        decimal fractionalYear = amount - years; // Gets the remainder as a fraction
        int days = (int)(fractionalYear * 365); // Convert the fractional year into days by multiplying by the number of days in a year (does not account for leap years)

        // Create a period from the years and days
        return new PeriodBuilder { Years = years, Days = days }.Build();
    }

//// TVIN 1: Builds a period for TVIN service using Aurora record
    // TODO: For months, can edit to *12 not 365
public static Period GetTVINPeriod(decimal amount)
    {
        int years = (int)amount; // Takes the whole number for years
        decimal fractionalYear = amount - years; // Gets the remainder as a fraction
        int days = (int)(fractionalYear * 365); // Convert the fractional year into days

        // Create a period from the years and days
        return new PeriodBuilder { Years = years, Days = days }.Build();
    }   

public static decimal TVINService(Member member, string TVINKey)
        {
            return member.MemberHistoricBenefitElementValues
                            .Where(x => x.ElementKey == TVINKey) 
                            .Select(x => (decimal?)x.Amount)
                            .FirstOrDefault()
                        ?? member.MemberTransferInElements
                            .Where(x => x.ElementType == TVINKey)
                            .Select(x => (decimal?)x.Amount)
                            .FirstOrDefault()
                        ?? 0m;
        }
}
