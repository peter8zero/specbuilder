////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
                // TEMPLATE GUIDANCE

                // All notes are helpful, but use TODO to determine main code changes required on set up.
                    // Note: This code has no "TODO" sections and can be published as it is.

                // Set Class: Make sure this is the same as the name of the module. 
                    // > When you assign a dependency in the main code, you select the module name (as seen in the Module contents page). 
                    // > However, When you call a method from the module in your code, you use the class name (below).
                        // > As such, it is easy to remember which class to call if it has the same name as the module/ dependency.

////////////////////////////////////////////// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

public static class SalarySchemeModule 

{

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
                // FPS: Create a variable type "FinalPensionableSalary" names "GetFPS[x]" for each salary type
                    // > There are some pre-created FPS types in a global Salary module: https://calculate.calcuat.xpsplc.com/global-modules/1
                    // > You can create bespoke FPS calculations below: Should you do so, these can be added to the global module for others to use by sending them to the V2 team.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
                // FPS1: This is an example of calculating a final pensionable salary based on a definition provided in the calculation spec.   
                    // e.g. "Highest salary in any 12 month or 52 week period during the 3 years immidiately preceding NRD (or DOL/ DOR if earlier).
                        // Considerations: 
                            // > An A>D will compare NRD and DOL, but an A>R will not have a DO. The calculation will need to handle both of these.
                            // > Use of "FPSCalcDate" as a parameter here, allows the coder to set NRD vs DOL in the A>D scheme calc and NRD vs DOR in the A>R scheme calc.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

    public static FinalPensionableSalary GetFPS1 (Member member, LocalDate FPSCalcDate) 
    {
        var threeYearsBeforeCalculationDate = FPSCalcDate.PlusYears(-3).PlusDays(1); // TODO: Set bespoke code based on what period is being considered for the FPS.

        var highestPSLastThreeYears = // TODO: Update this example to work as required.
            member.SalaryHistory.Where(x => x.SalaryType.Type == "FTSAL" // EXTRA INFO: Member's Salary History in Aurora, looking at all salary types with key "FTSAL". TODO: Update salary type key code.
            && (x.EffectiveFrom >= threeYearsBeforeCalculationDate // EXTRA INFO: Selects member's salaries that started on or after 3 years prior to the FPS Calc date.
            || (x.EffectiveFrom < threeYearsBeforeCalculationDate && (x.EffectiveTo > threeYearsBeforeCalculationDate || !x.EffectiveTo.HasValue)))) // EXTRA INFO: Also selects salaries that started more than 3 years ago but remined (or remains) effective since the date 3 years ago passed.

            .OrderByDescending(x => x.Amount) // EXTRA INFO: Salaries are ordered by value.
            .FirstOrDefault()?.Amount ?? 0; // EXTRA INFO: The salary first in the list (highest) is selected, or 0 is given if the list is empty.

        Log.Debug($"HighestPSLastThreeYears: {highestPSLastThreeYears}"); // EXTRA INFO: Add a debug which returns FPS1 for information.  

        var finalPensionableSalary1 = highestPSLastThreeYears.Round(); // TODO: Always set "finalPensionableSalary[x]" with the amount rounded as required (here to 2 d.p.).

        return new FinalPensionableSalary // EXTRA INFO: The variable "FinalPensionableSalary" requires the "Type", "Amount" and "EffectiveFrom" date to be assigned. Do so below:
        {
            Type = "FPS1",
            Amount = finalPensionableSalary1,
            EffectiveFrom = FPSCalcDate            
        };
    }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////    
                // FPS2: This is an example of returning a salary held in a field in Aurora. 
                    // > The field needs to be identified (as a string) and returned (as a decimal) and then converted to a "FinalPensionableSalary" by also assigning a "type" and "EffectiveFrom" date.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

    public static FinalPensionableSalary GetFPS2 (Member member, LocalDate FPSCalcDate)
    {
 
        object fpsfieldcheck = member.ExtraDataStringField("Quote FPS1"); // EXTRA INFO: Checks for a string. 

        // EXTRA INFO: If string is not empty (null) return value as a decimal. Otherwise return nil.
        var finalPensionableSalary1 = (fpsfieldcheck != null) ? member.ExtraDataDecimalField("Quote FPS1"): 0m; 

        return new FinalPensionableSalary 
        {
            Type = "FPS1",
            Amount = finalPensionableSalary1,
            EffectiveFrom = FPSCalcDate          
        };        
    }
    
}    
