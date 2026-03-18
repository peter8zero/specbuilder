

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // TEMPLATE GUIDE
                #region TEMPLATE GUIDE - EXPLANATION
                // Green comments are explanatory notes.
                    // Some comments explain basic principals of coding
                    // Other comments relate to the specific active to deferred calculation code set up

                // Generally, comments are explanatory and do not require coders to make updates.
                    // Where a comment starts with "TODO", a coding decision needs to be made, and the code potentially updated. 
                        // Use "CTRL-F" to search for TODO to highlight all coding updates required and jump between instances.

                // Once this template has been imported into a Scheme specific coding environment, green comments can be removed, as the Scheme code is progressed.

                // Green comments can also be expanded and reduced, using the arrows to the left of each section starting "////////" or at the start of each "#region".

                #endregion
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // A: PRE-CALC SET UP
                #region PRE-CALC SET UP - EXPLANATION

                // "using" imports Namespaces into the file. The code can then use various types (classes, interfaces, etc) defined within that Namespace.
                // It prevents the need to define a path within the code, making it more readable.
                // e.g. The Namespace "System.Text" contains the type "StringBuilder". Once it is defined here, we can use it to create strings of text for Log.Information.
                    // With "using" assigned: StringBuilder InformationLog = new StringBuilder();
                    // Without "using" assigned: System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    // Example error if Namespace is missing: "The type or namespace name 'StringBuilder' could not be found (are you missing a using directive or an assembly reference?)."

                // KEY WORDS: 
                    // Namespace: A way to logically organise code, acting as a container for related objects and helping to maintain naming convention across different Schemes' code.

                #endregion
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using NodaTime.Extensions; 
using System.Text;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // B: SET CLASS
                #region SET CLASS - EXPLANATION

                // OUTPUT: Retirement Benefit Object
                    // > A class acts as a blueprint for creating an object. It will encapsulate all the data required as well as the methods to manipulate that data into a result.
                    // > Within a class you can define the data structure and the functions which use that data.
                    // > Classes allow for the creation of modular code, which can be de-bugged, reused (e.g. in A>D and A>R calcs) and maintained easily.

                // INPUT: All necessary steps to calculate Retirement Benefits Object (The method ("Calculate()"), Validations for members, Calculation Properties, etc)
                    // > The "TEMPLATEActiveToDeferredCalculation" class below contains the logic and data necessary to calculate retirement benefits, following the calculation of service and creation of deferred elements.
                    // > The syntax ": ActiveToDeferredCalculationBase" means the TEMPLATEActiveToDefererdCalculation class inherits from a Base class.
                        // > This Base class contains definitions of Data Types specific to a A>D calc. Our class can use, extend and override Base methods and properties.
                        // > Below, we use "Calculate()" which is an abstract method from the "ActiveToDeferredCalculationBase" Base class. The syntax "public override" is used to allow the code to override certain features.
                        // > Scheme specific properties and methods can then be added to the class.

                // KEY WORDS:
                    // Base Class: Parent or Top Class
                    // Derived Class: Sub Class
                    // Method: A function (e.g. "ValidateMemberForCalculation())" associated with an object (see below) and is defined within a class. 
                        // Methods perform actions (a calculation, a check of a record, etc) a return values (output figure, warning message, etc).
                    // Abstract Method: Does not contain its own executable code but serves as a placeholder for methods to be implemented elsewhere (e.g. "Calculate()").
                    // Object: 
                        // 1. Has states, represented by the values of its properties. (e.g. Member.Person object contains DateOfBirth property.)
                        // 2. Exhibits behaviors, defined by methods, which can use or edit the state. (e.g. Adding 65 years to DateOfBirth to get GMP Age.)

                #endregion
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class TEMPLATEActiveToDeferredCalculation : ActiveToDeferredCalculationBase  // TODO: Replace "TEMPLATEActiveToDeferredCalculation" with [SchemeName]ActiveToDeferredCalculation
{  

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // C: SET PROTECTED DATA TYPES (string, decimal, LocalDate, etc)
                #region SET PROTECTED DATA TYPES - EXPLANATION

                // OUTPUT: Protected Data Types 
                    // > These Data Types are protected. As such, use of the name (e.g. "GmpValues") can only return the protected value.
                    // > You will be unable to accidently name a new variable, within the code, by the same name. This prevents accidently over-riding values later in the code.
                    // > Protected Data Types are accesible in the class where they have been defined and their sub-classes (derived classes).

                // INPUT: Values assigned to Protected Data Types
                    // > The value can be set directly here: e.g. pre97ExcessPensionElement = "PRE97". This sets the string "PRE97" as the pre97ExcessPensionElement Value.
                    // > Values can also be defined under "SetupCalculationProperties", in instances where calculation data (e.g. the member object) needs to be accessed. 
                        // > See "SetupCalculationProperties" for definitions of each Data Type used in this code.
                #endregion
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    protected LocalDate CalculationEndDate;
    protected LocalDate FPSCalcDate;
    protected LocalDate normalRetirementDate;
    protected decimal LifetimeAllowance;
    protected decimal DcFund;
    protected LocalDate GmpDate;
    protected GmpValues GmpValues;
    protected decimal MissedIncreaseFactor;
    protected decimal GmpStandardToActualFactor;
    protected const string pre97ExcessPensionElement = "PRE97"; // TODO: If the Pre97 excess penion element is not called "PRE97", update here. It should match the pension element defined in PCLSandCommutationSchemeModule.

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // D: A>D CALCULATION
                #region A>D Calculate() - EXPLANATION

                // OUTPUT:
                    // > A>D Results List, as set up in the Base Class, returning:
                        // > DateOfLeaving
                        // > Date ElementsEffectiveFrom
                        // > DatePensionDue (usually NRD or DOL if earlier)
                        // > Final Pensionable Salary or Salaries applicable a a list "FinalPensionableSalary"
                        // > Deferred ElementValues 
                        // > Spouse Values as "SpousePensionBeforeRetirement"
                        // > GmpValues
            
                // INPUTS:
                    // > 1. Call "ValidateMemberForCalculation();" and "SetupCalculationProperties();", as two additional methods to be set later in the class, for storing validation checks and protected values.

                    // > 2. Set up the Active To Deferred (A>D) calculation

                        /***************************************************************
                        2a. SERVICE BUILDER
                        
                        This section calls the global service builder module which is designed to provide service history for the member.

                        Output: Once tranches are set up in Aurora, the service builder is designed to output service duration for each tranche key, which can be set to account for service breaks and part time service.

                        Inputs: The service builder requires inputs of:
                            > Service tranches applicable to member - held in Aurora member record under call CategoryReferenceData.ServiceTranches
                            > Service periods and breaks applicable to member - held in Aurora member record under calls Member.ServicePeriods, Member.ServiceBreaks
                            > Date member commenced pensionable service - held in Aurora member record under call Member.DateCommencedPensionableService.Value
                            > Scheme service end date - This has to be set in the code for each scheme. Below it is currently set to CalculationEndDate.

                            > .ServiceUnitCalculation(PARAMETER) - PARAMETER has to be set to the correct service units required in the output (e.g. Years and Months)
                                > ServiceUnits.YearsAndCompleteMonths, ServiceUnits.YearsAndMonthsRoundedUp or ServiceUnits.YearsAndMonths15RoundedUp can be called from the global Service Builder.
                                > If a different service unit calc is required it required bespoke coding within this code, calling "MySchemeServiceUnitCalculation" via "MySchemeServiceUnitCalculation mySchemeServiceUnitCalculation = new MySchemeServiceUnitCalculation();". 
                                    > An example is given below, for Scheme Specific "Years and Days 365".

                            > .WithMethod - Service method. Usually default, but can be made bespoke in instances such as altering the preservation method (default adds to latest tranche)

                            > .WithFillType - Service fill type. Usually "Gaps to be filled".
                        ***************************************************************/

                        /***************************************************************
                        2b. CALCULATING DEFERRED PENSION ELEMENT VALUES 
                        
                        This section uses the service builder outputs to create deferred elements at Date of Leaving.

                        Output: Once the service builder has provided service duration per tranche key the next section is designed to:
                            > deferredElementValues: Take the service history and feed it into specific scheme code set up in the ActiveToDeferredElementsSchemeModule.
                                > This produces a deferred element value for each element key.
                            > orderedPensionElementValues: Put the deferred element values into the order we have asked it to.
                                > This is per the display order set up within the ActiveToDeferredElementsSchemeModule.

                        Inputs:
                        > ActiveToDeferredElementsSchemeModule
                        > SalarySchemeModule
                        > The only changes required to the below code are the ensure the parameters in brackets are set correctly. TODOs have been added to indicate checks required.
                        ***************************************************************/  

                    // > 3. Set Log.Debugs and Log.Information
                        // > A standard set of Log.Debugs and Log.Information are set within the code, which will appear in the "Log" of a test case.
                        // > Log.Debugs are designed to show coders how the calculation is progressing. Therefore, by putting them after their associated value, we can see where the code errors and problem solve.
                        // > Log.Information is set up to show information to admin in a reader-friendly way and can therefore be placed anywhere convienient in the code. 
                            // > They are compiled using string builders, to create a message embedded with values.
                        // > These act differently to "InterimValues.Add("Name", Value);" as they do not impact test cases if updated.

                    // > 4. Assign results values to the pre-set "Calculate()" Results List (see "OUTPUT" at the top of this section).
                        // > This must go at the end of the "Calculate()" method. After results have been assigned (using "return"), no further code can be accessed, within that method.

                    // KEY WORDS:
                        // Variable: A storage location in memory that can hold a value. This can be a simple specific data type (integer, decimal, string of text) or a complex data structure, like an object.
                            // Variables can be set by typing "var" or the specific name (e.g. "decimal"). Hovering over "var" with a cursor will show the variable type.
                            // In the template code below, simple variables are inititated using their name, for ease of understanding. Complex variables use "var" for readability purposes.

                        // Parameters: When a method is written, it may require data to be passed into it. This data is set as parameters within brackets, before the method is then written.
                            // Each parameter has a "type" (see 'Variable' above) and a name (any name we assign to it).
                            // When a method is called, data is passed into the call by assigning it to the matching parameter.
                                // NOTE: A walkthrough example of setting and calling parameters can be found at the end of this code.

                        // String Builder: A modyfiable string of characters.
                            // Part of the "System.Text" namespace defined under "using" at the top of the code.
                            // Modifyable (can be cleared and repopulated).
                            // Low memory usage compared to other strings as they are modified in place rather than creating an object in memory.
                            
                #endregion        
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public override ActiveToDeferredResult Calculate()
    {   
        //// 1. Validations and Property Set Up
        ValidateMemberForCalculation();      
        SetupCalculationProperties();

        //// 2. A>D SECTION OF CODE

        // TODO: Set up the tranche keys in Aurora
            // > Tranche keys are provided within the A>D (or A>R) tab of the Scheme specification, which need to be input into Aurora under Client > Scheme (select scheme) > Admin > Categories > Service Tranches. 
            // > The spec provides information for the Tranche Key, Service Accrual (nominator = 1, denominator = 60/80/etc), start and end date.

            /***************************************************************
            2a. SERVICE BUILDER 
            ***************************************************************/
        IServiceUnitCalculation mySchemeServiceUnitCalculation = new MySchemeServiceUnitCalculation(); // TODO: This line is only required where the service method is bespoke in .ServiceUnitCalculation (see explanation above)

        // TODO: Set up the "ActiveToDeferredElementsSchemeModule" following the template instructions. Publish and set as a dependency here.
            // TODO: Set up for SalarySchemeModules. Add dependency within the ActiveToDeferredElementsSchemeModule. Instructions on how to make this bespoke are within the module template.
            // EXTRA INFO: That module also contains a dependency for the global ServiceBuilder which will flow through and allow the ServiceCalculator below to work.
            
        IServiceCalculator ServiceCalculator = new ServiceBuilder(
            CategoryReferenceData.ServiceTranches, Member.ServicePeriods, Member.ServiceBreaks, Member.DateCommencedPensionableService.Value, CalculationEndDate) // TODO: Check schemeServiceEndDate is correct (currently CalculationEndDate)
            .ServiceUnitCalculation(mySchemeServiceUnitCalculation) // TODO: This is bespoke in this Scheme to allow for Years and Days 365. Add correct parameter for service units, available in global builder or written as bespoke (e.g. ServiceUnits.YearsAndCompleteMonths).
            .WithMethod(ServiceMethod.Default) 
            .WithFillType(ServicePeriodFillType.GapsToBeFilled) 
            .Build();

        InterimValues.Add("CategoryReferenceData.ServiceTranches", CategoryReferenceData.ServiceTranches); // EXTRA INFO: Adds the full Aurora service data as interim value. Purposely not a debug as a large list.
        
        List<ServiceHistoryOverTrancheWithDuration> serviceHistoryOverTrancheWithDurations = ServiceCalculator.Calculate();
        InterimValues.Add("serviceHistoryOverTrancheWithDurations", serviceHistoryOverTrancheWithDurations); // EXTRA INFO: Adds the service builder outputs as interim value. Purposely not a debug as a large list.
       
        var serviceHistoryFigures = serviceHistoryOverTrancheWithDurations.Select(s => new { Key = s.Tranche.Key, PartTimeAdjustedDurationAmount= s.PartTimeAdjustedDurationAmount, PartTimeAdjustedDuration = s.PartTimeAdjustedPeriod }).ToList(); // This is not used in the code but is in here to give an interim values list of the Total Durations and Keys. Can be changed or removed or the code utilised elsewhere for subsets of interim values lists.
        InterimValues.Add("serviceHistoryFigures", serviceHistoryFigures);
 
            /***************************************************************
            2b. CALCULATING DEFERRED PENSION ELEMENT VALUES 
            ***************************************************************/
        var deferredElementValues = ActiveToDeferredElementsSchemeModule.CalculateDeferredElementValuesIncGMP(Member, 
                                                                                FPSCalcDate, // EXTRA INFO: See Salary Scheme Module for explanation of this parameter. Make sure protected value is set correctly in this calc.
                                                                                GmpValues,
                                                                                CategoryReferenceData,
                                                                                serviceHistoryOverTrancheWithDurations); 

        var orderedDeferredElementValues = deferredElementValues.OrderBy(x=> ActiveToDeferredElementsSchemeModule.elementDisplayOrder().IndexOf(x.Key));

        //// > 3. Set Log.Debugs and Log.Information
        Log.Debug ($"Total Deferred Elements Calculated From Service History: {orderedDeferredElementValues.Sum(element => element.Value).Round()}.", "Deferred Element Valuation");
        
        string orderedDeferredElementValuesLog = 
        $"Deferred Elements Calculated From Service History: {string.Join(Environment.NewLine, orderedDeferredElementValues.Select(ev => $"Element: {ev.Key}, Value: {ev.Value.Round()}"))} ";
        Log.Information(orderedPensionElementValuesLog);

        return new ActiveToDeferredResult
        {
            DateOfLeaving = CalculationEndDate,
            ElementsEffectiveFrom = CalculationEndDate, // TODO: Check this is DOL within SetupCalculationProperties();
            DatePensionDue = Parameters.DateOfLeaving > Member.NormalRetirementDate ? Parameters.DateOfLeaving : Member.NormalRetirementDate, // EXTRA INFO: Set here as NRD or DOL if over NRD  

            FinalPensionableSalary = 
            new List<FinalPensionableSalary>
            {
                SalarySchemeModule.GetFPS1(Member, FPSCalcDate),
                SalarySchemeModule.GetFPS2(Member, FPSCalcDate) // TODO: Add or remove FPS[x] per SalarySchemeModule
            },

            ElementValues = orderedDeferredElementValues.Where(x =>x.Value > 0).ToList(),
            SpousePensionBeforeRetirement = orderedDeferredElementValues.Sum(x => x.SpouseValue).Round(),
            
           GmpValues = GmpValues,
            
        };
    }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // E: SET UP CALCULATION PROPERTIES
                #region SET UP CALCULATION PROPERTIES - EXPLANATION

                // This section sets protected values. These variable values cannot be overrriden elsewhere in the code, and can be called for use anywhere in the code.
                // This code gives protected values for:
                    // > CalculationEndDate - TODO: To be updated for when the code should calculate service up to. DOR for A>R and DOL for A>D. Any addions to totalPension uplited from DOR.
                    // > FPSCalcDate - TODO: TO be updated for what date is used to take as FPS starting point
                    // > normalRetirementDate - The single Normal Retirement Date as recorded within Aurora for the member, rather than element specific NRDs.
                    // > Lifetimeallowance
                    // > DCFund - Sums up external fund values in the member record by provider.
                    // > Gmp date and values, including Missed Increase and Increment decimals for late GMP - Calls methods per the GMP global module

                #endregion
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

protected virtual void SetupCalculationProperties()
    {
        CalculationEndDate = Parameters.DateOfLeaving; // TODO: Set when the calculation ends. It will be DOR for A>R and DOL for A>D.
        FPSCalcDate = Parameters.DateOfLeaving; // TODO: Set the FPSCalcDate as required in the SalarySchemeModule for the salary effective date. See that module for further information.

        normalRetirementDate = Member.NormalRetirementDate;

        // TODO: Set LifetimeAllowance global dependency 
        LifetimeAllowance = LifetimeAllowanceGlobalModules.GetLifetimeAllowanceLimit(Parameters.DateOfLeaving, GlobalReferenceData); // TODO: Ensure the first parameter ("EndDate" per global module) is set correctly.

        DcFund = Member.ExternalFundValues
                .GroupBy(x => x.Provider)
                .SelectMany(group =>
                group.OrderByDescending(g => g.EffectiveFrom)
                    .Take(1)
                    .Select(item => item.FundValue)).Sum(); // TODO: Consider the information in Member External Fund Values. This assumes the only information held there is DC fund values.

        // TODO: Set GMP global dependency 
        GmpDate = GmpGlobalModules.GetGmpDate(Member.Person.DateOfBirth, Member.Person.Gender);

        var gmpelements = GmpGlobalModules.CalculateActiveGuaranteedMinimumPensionValues(Parameters.DateOfLeaving,
                                                                                                        Member.Person.DateOfBirth,
                                                                                                        Member.Person.Gender,
                                                                                                        Member.ContractedOutEarnings,
                                                                                                        GlobalReferenceData,
                                                                                                        GmpType.Fixed, // TODO: Set GMP Type as .Fixed, .Limited or .S148 **Update to GMP coming to allow for multiple**
                                                                                                        1.0325m // TODO: Set future s148 factor or 1 if n/a 
                                                                                                        ).ToList(); 

        // EXTRA INFO: The below calls the global GMP modules for Missed Increases and Increments. You may notice we also set these factors within the ReusableFunctionsSchemeModule
            // > A. The below factors are set for the purposes of total pre and post GMP revaluation which would be matched against a HMRC calc by admin, to set the required GMP in payment
            // > B. The factors within the ReusableFunctionsSchemeModule should reflect how admin use GMP increases for the purposes of individual deferred element revaluation
                // > These methods may be the same, but sometimes, individual elements do not have uplift or uplift differently. In these cases the difference between A and B creates a PRE97 excess

        MissedIncreaseFactor = GmpGlobalModules.MissedIncreaseDateOfLeaving(Parameters.DateOfLeaving, 4, 1, 1, SchemeReferenceData, Member.Person.DateOfBirth.PlusYears(Member.Person.IsMale ? 65 : 60)); // TODO: Check first parameter should be DOL. Check 4th parameter "1" (to be set to assumed future missed increase uplift)
        GmpStandardToActualFactor = GmpGlobalModules.GmpStandardToActualDateOfLeavingFactor(Parameters.DateOfLeaving, Member.Person.DateOfBirth, Member.Person.IsMale,true); // TODO: Check first parameter should be DOL

        GmpValues = new () // Will value to DOL so uplift by GMP Uplift to then (if leaving after GMP Age - rare scenario).
            {
            EffectiveFrom = gmpelements.Single().EffectiveDate,
            Pre88GmpAtDateOfRetirement = ((gmpelements.Single().Pre88AtEffectiveDate) * GmpStandardToActualFactor).Round(), 
            Pre88GmpAtGmpPaymentDate = gmpelements.Single().Pre88AtGmpDate,
            Post88GmpAtDateOfRetirement = ((gmpelements.Single().Post88AtEffectiveDate) * GmpStandardToActualFactor * MissedIncreaseFactor).Round(),
            Post88GmpAtGmpPaymentDate = gmpelements.Single().Post88AtGmpDate,
            };
    }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // F: MEMBER VALIDATIONS
                #region MEMBER VALIDATIONS - EXPLANATION

                // OUTPUT: 
                    // > ValidateMemberForCalculation() - A set of checks are performed to ensure the calculation can be run.
                        // > The parameters required to run the calculation are: Member number and Date Of Retirement. 
                        // > Validations are run to check the member is within a valid scheme category and that their date of retirement will produce a result.
                        // > Error messages will appear if the member parameters are not valid, preventing the calculation from running.

                    // PostCalculationValidation()
                        // > These checks are run as the calcultion proceeds, once it is determined the member is valid.
                        // > Various warning or error messages will be output, based on certain checks. A warning will not stop the calculation. An error will stop the calculation at the point the validation is run.

                // INPUTS:
                    // > See descriptions of each validation below.

                #endregion
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////    

    protected virtual void ValidateMemberForCalculation()  
    {
        // VALIDATION 1:  Check member is in a valid category
        HashSet<string> categoriesCoded = new HashSet<string> { "BRK", "UNK", "YAM1", "YAM2", "YAM3" }; // TODO: Set up the list of the names of the Scheme Categories this code can be run for.
        var memberMostRecentSchemeCategoryInformation = Member.CategoryHistory.OrderByDescending(x => x.EffectiveFrom).FirstOrDefault(); // EXTRA INFO: This returns a complex data structure, being the member's most recent category information set.
        bool memberCategoryIsCoded = categoriesCoded.Contains(memberMostRecentSchemeCategoryInformation.Category.Key); // EXTRA INFO: This checks if the most recent category key in the set is one of those listed above. Returns "true" or "false".

        if (memberCategoryIsCoded == false) // EXTRA INFO: Log an error if the member's category is not listed.
        {
            Log.Error($"This member is in category {memberMostRecentSchemeCategoryInformation.Category.Key}. This category is not valid for the automated calculation being attempted.");
        }
    }

    protected virtual void PostCalculationValidation(decimal zeroCashCommutedLifetimeAllowancePension, decimal CommutedPensionTotalCash) // EXTRA INFO: Indicate parameters (type and name) required for validations. These are called in section 
    {
        // FURTHER VALIDATIONS: 
            // TODO: Add specific validations for the Scheme here. No template validations for A>D.
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
                // ADDITIONAL METHODS SECTION

                // ADDITIONAL METHOD 1: EXAMPLE OF BESPOKE SERVICE BUILDER CODE
                    
                    // OUTPUT: Service Units for options builder in "Years and Days 365".
                    // INPUTS: Bespoke code has been written to convert a decimal value for years into years and days. 
                        // If another service unit calculation is required and unavailable in the global service builder module, the code below will need over-writing for that scenario.
                        // Further bespoke code can also be added for other aspects of the service builder, such as service method.

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

        // ADDITIONAL METHOD 1: EXAMPLE OF BESPOKE SERVICE BUILDER CODE
public class MySchemeServiceUnitCalculation : IServiceUnitCalculation
    {
        public Period PeriodBetween(LocalDate startDate, LocalDate endDate)
        {    
            Period initialPeriod = Period.Between(startDate, endDate.PlusDays(1), PeriodUnits.Days); // EXTRA INFO: Calculates the initial period

            // EXTRA INFO: Count leap years between the two dates, but only after the specified date:
            int leapYears = 0;
            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                if (DateTime.IsLeapYear(year)) // EXTRA INFO: // Check if the year is a leap year
                {
                    // EXTRA INFO: Check if the leap day (February 29) is within the range and after the specified date
                    LocalDate leapDay = new LocalDate(year, 2, 29);
                    if (startDate <= leapDay && leapDay <= endDate)
                    {
                        leapYears++;
                    }
                }
            }

            int adjustedDays = initialPeriod.Days - leapYears; // EXTRA INFO: Deduct the number of leap years from the total days

            int fullYears = adjustedDays / 365;
            int remainingDays = adjustedDays % 365;

            // EXTRA INFO: Create a new period with the adjusted number of days
            Period adjustedPeriod = new PeriodBuilder
            {
                Years = fullYears,
                Days = remainingDays 
            }.Build();

            return adjustedPeriod;
        }

        public decimal Duration(Period period) 
        {
            decimal years = period.Years;
            decimal dayFraction = (decimal)period.Days / 365; 
            // EXTRA INFO: Based on spec asking  for "Years and Days 365". If specific leap years require calculating, this will need updating to look specifically at the start and end date.
            return years + dayFraction; 
        }

        public Period GetPeriod(decimal amount)
        {
            int years = (int)amount; // EXTRA INFO: Whole years
            decimal fractionalYears = amount - years; // EXTRA INFO: Fraction of a year
            int days = (int)(fractionalYears * 365m); // EXTRA INFO: Convert fractional year to days

            return new PeriodBuilder { Years = years, Days = days }.Build(); // EXTRA INFO: Shows period in xYXD.
        }

 	    public decimal DurationInYears(decimal amount)
        {
            return amount / 12;
        }

        public Period GetPeriodFromYears(decimal amount)
        {
            int years = (int)amount;
            int months = (int)((amount - years) * 12m).Round(0);
            return Period.FromYears(years) + Period.FromMonths(months); 
        }       
    }
}
