//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // TEMPLATE GUIDE
                #region TEMPLATE GUIDE - EXPLANATION
                // Green comments are explanatory notes.
                    // Some comments explain basic principals of coding
                    // Other comments relate to the specific active to retired calculation code set up

                // Generally, comments are explanatory and do not require coders to make updates.
                    // Where a comment starts with "TODO", a coding decision needs to be made, and the code potentially updated. 
                        // Use "CTRL-F" to search for TODO to highlight all coding updates required and jump between instances.

                // Once this template has been imported into a Scheme specific coding environment, green comments can be removed, as the Scheme code is progressed.

                // Green comments can also be expanded and reduced, using the arrows to the left of each section starting "////////" or at the start of each "#region".

                #endregion
///////////////////////////////////////////////// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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
                    // > Classes allow for the creation of modular code, which can be de-bugged, reused (e.g. in D>R and A>R calcs) and maintained easily.

                // INPUT: All necessary steps to calculate Retirement Benefits Object (The method ("Calculate()"), Validations for members, Calculation Properties, etc)
                    // > The "TEMPLATEActiveToRetiredCalculation" class below contains the logic and data necessary to calculate retirement benefits, following the calculation of service and creation of deferred elements.
                    // > The syntax ": ActiveToRetiredCalculationBase" means the TEMPLATEActiveToRetiredCalculation class inherits from a Base class.
                        // > This Base class contains definitions of Data Types specific to a A>R calc. Our class can use, extend and override Base methods and properties.
                        // > Below, we use "Calculate()" which is an abstract method from the "ActiveToRetiredCalculationBase" Base class. The syntax "public override" is used to allow the code to override certain features.
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

public class TEMPLATEActiveToRetiredCalculation : ActiveToRetiredCalculationBase // TODO: Replace "TEMPLATEActiveToRetiredCalculation" with [SchemeName]ActiveToRetiredCalculation
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
                // D: A>R CALCULATION
                #region A>R Calculate() - EXPLANATION

                // OUTPUT:
                    // > A>R Results List, as set up in the Base Class, returning:
                        // > PensionElements
                        // > GmpValues
                        // > MinimumCashCommuted
                        // > MaximumCashCommuted 
                        // > TargetCashCommuted

                // INPUTS:
                    // > 1. Call "ValidateMemberForCalculation();" and "SetupCalculationProperties();", as two additional methods to be set later in the class, for storing validation checks and protected values.

                    // > 2. Set up the Active To Deferred (A>D) part of the calculation

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
                        
                        This section uses the service builder outputs to create deferred elements at Date of Leaving (or Date of Retirement if direct A>R).

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

                    // > 3. Set up the Deferred to Retired (D>R) part of the calculation

                        /***************************************************************
                        3a. REVALUING DEFERRED ELEMENTS FOR RETIREMENT

                        Output: 
                        > elementsAfterRetirement: Calls the Reval section at the end of this code to apply GMP Uplifts and scheme ERFs and LRFs to deferred element keys.

                        Inputs:
                        > CalculateRetirementElementsA2R which is set up as a method at the end of this code.
                        ***************************************************************/ 

                        /***************************************************************
                        3b. PCLS AND COMMUTATION OPTIONS BUILDER

                        This section calls the global options builder module which is designed to provide maximum, minimum and target pension options based on given inputs.

                        Output: 
                        - Maximum, Minimum and Target pension options: DB Cash, AVC Cash, Total Cash, Residual Pension and Spouse Pension.
                        - The Log.Information sections are taking the results of the builder and outputting an information block which summarises the maximum options available, to aid testing. Target information is also provided if a target amount is given as a parameter on running a case.

                        Inputs: The options builder requires inputs for:
                        - (Member, SchemeReferenceData, GlobalReferenceData, preCommutationPensionElements, Parameters.DateOfRetirement) - these calls remain the same for each scheme.
                            > TODO: "preCommutationPensionElements" are set up in the PCLSandCommutationSchemeModule. The below code does not need updating, but instructions need to be followed in that module.

                        - .WithCommutationFactors(PARAMETER) - PARAMETER is set to call "CommutationFactorsForElements". 
                            > TODO: "CommutationFactorsForElements" are set up in the PCLSandCommutationSchemeModule. The below code does not need updating, but instructions need to be followed in that module.

                        - .WithGMPRestriction(PARAMETER) - PARAMETER is set to call "gmpRestriction". 
                            > TODO: "gmpRestriction" is set up in the ElementValuesSchemeModule. The below code does not need updating, but instructions need to be followed in that module.

                        - .Ordered(PARAMETER) - Tells the code if it is ordered commutation.
                            > TODO: If the scheme has ordered commutation "OrderForCommutation" will be called and the order requires updating. If proportional, line not required in the builder.

                        - .WithDCFund(PARAMETER) - Sets the DcFund value, which is defined under "SetupCalculationProperties".

                        - .WithSpouses(spousePercentage) - Optional line that will default to 50%. If not 50%, set this to value and uncomment call below.

                        - .WithSpousesGMPMinimumOption(PARAMETER) - Spouses default with GMP
                            >TODO: PARAMETER set to "SpousesGMPMinimumOptions.NilPre88GMP", whereby all GMP is assumed to have 50% spouses except Female Pre88GMP which has 0. Change "NilPre88GMP" to "FiftyPercent" if all GMP has 50%s spouses.

                        - .WithPIE(PARAMETER) - Bespoke for PIE. Further instructions to be added once this has been utilised.

                        - .WithDecimalPlaces(PARAMETER) - Defaults to 2. Can be updated if result not wanted in pence. 
                        ***************************************************************/

                    // > 4. Set Log.Debugs and Log.Information
                        // > A standard set of Log.Debugs and Log.Information are set within the code, which will appear in the "Log" of a test case.
                        // > Log.Debugs are designed to show coders how the calculation is progressing. Therefore, by putting them after their associated value, we can see where the code errors and problem solve.
                        // > Log.Information is set up to show information to admin in a reader-friendly way and can therefore be placed anywhere convienient in the code. 
                            // > They are compiled using string builders, to create a message embedded with values.
                        // > These act differently to "InterimValues.Add("Name", Value);" as they do not impact test cases if updated.

                    // > 5. Assign results values to the pre-set "Calculate()" Results List (see "OUTPUT" at the top of this section).
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

public override ActiveToRetiredResult Calculate()
    {   
        //// 1. Validations and Property Set Up
        ValidateMemberForCalculation();      
        SetupCalculationProperties();

        //// 2. A>D SECTION OF CODE

        // TODO: Set up the tranche keys in Aurora
            // > Tranche keys are provided within the A>R tab of the Scheme specification, which need to be input into Aurora under Client > Scheme (select scheme) > Admin > Categories > Service Tranches. 
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

        Log.Debug ($"Total Deferred Elements Calculated From Service History: {orderedDeferredElementValues.Sum(element => element.Value).Round()}.", "Deferred Element Valuation");
        
        //// 3. D>R SECTION OF CODE

            /***************************************************************
            3a. REVALUING DEFERRED ELEMENTS FOR RETIREMENT
            ***************************************************************/
        // TODO: Go to CalculateRetirementElementsA2R at the end of the code and update as instructed.
        var deferredElementsAtRetirement = CalculateRetirementElementsA2R(deferredElementValues); // EXTRA INFO: Returns elements revalued by retirement factor, gmp uplift or other process.
    
        Log.Debug($"Deferred Element Values after Retirement Factors: {deferredElementsAtRetirement}", "Deferred Element Valuation");

            /***************************************************************
            3b. PCLS AND COMMUTATION OPTIONS BUILDER
            ***************************************************************/
        // TODO: Set up and publish PCLSandCommutationSchemeModule and set as a dependency.

        // EXTRA INFO: Convert Deferred Elements into Pension Elements:
        var preCommutationPensionElements = PCLSandCommutationSchemeModule.PreCommutationPensionElements(deferredElementsAtRetirement, pre97ExcessPensionElement, Parameters.DateOfRetirement, GmpDate, GmpValues);
        
        Log.Debug(string.Join(Environment.NewLine, preCommutationPensionElements.Select(ev => $"Key: {ev.Key}, Value: {ev.Value.Round()}")), "PreCommutationPensionElements");

        var commutationFactorsForElements = PCLSandCommutationSchemeModule.CommutationFactorsForElements(Member, preCommutationPensionElements, Parameters.DateOfRetirement, SchemeReferenceData);
        var gmpRestriction = ElementValuesSchemeModule.TotalGMPAt.RetirementDate(GmpValues); // TODO: Set up and publish the "ElementValuesSchemeModule" and add as a dependency
        // List<string> OrderForCommutation = new List<string> {"DCFund", "PRE97","POST97","POST16","POST20"}; // TODO: Uncomment if ordered and set order.
        //var spousePercentage = 0.5m; // TODO: Uncomment if you want to set spouse percentage

        // TODO: Add "PensionOption" global dependency

        IPensionOptionCalculator pensionOptionCalculator = new PensionOptionBuilder(Member, SchemeReferenceData, GlobalReferenceData, preCommutationPensionElements, Parameters.DateOfRetirement) 
        .WithCommutationFactors(commutationFactorsForElements) // EXTRA INFO: commutationFactorsForElements is assigned here.
        .WithGMPRestriction(gmpRestriction, GmpDate, "PRE97") // EXTRA INFO: gmpRestriction, GmpDate (a protected data type in SetupCalculationProperties()) and pre97ExcessPensionElement (protected data type set at top of code) are assigned here.
        //.Ordered(OrderForCommutation) // TODO: Optional line for ordered commutation - will default to proportional. Consider if needed.
        //.WithDCFund(DcFundOther) // TODO: Optional line - Optional Line which will will default to the DCFund protected data type. Uncomment and add a variable for DCFundOther, if not the default. Consider if needed.
        //.WithSpouses(spousePercentage) // TODO: Optional line - will default to 50%. TODO: Consider if needed.
        //.WithSpousesGMPMinimumOption(SpousesGMPMinimumOptions.NilPre88GMP) // TODO: All GMP assumed to have 50% spouses except Female Pre88GMP which has 0. Change "NilPre88GMP" to "FiftyPercent" if all GMP has 50%s spouses. Consider if needed.
        //.WithPIE(pieElements, PIEFactorInterpolation.YearMonth, PIEApplication.ApplyAfterCommutatio) - TODO: Optional bespoke section for PIE. Seek further guidance if required. Consider if needed.
        //.WithDecimalPlaces(int DecimalPlaces) - TODO: Optional line = defaults to 2. Consider if needed.
        .Build(); 

        PensionOption maximumPensionOption = pensionOptionCalculator.CalculateMaximum(); 
        PensionOption zeroLumpSumOption = pensionOptionCalculator.CalculateZeroLumpSum();   
        PensionOption targetPensionOption = pensionOptionCalculator.CalculateTarget(Parameters.TargetCashAmount, maximumPensionOption);
        PostCalculationValidation(zeroLumpSumOption.LifetimeAllowancePensionAmount, maximumPensionOption.TotalCash); // EXTRA INFO: This line is setting parameters for some of the PostCalculationValidations. See section E.

        //// 4. Set Log.Information 
            // > CODED BELOW: Standard set of Information Logs which can be added to.

        var sb = new StringBuilder();

            // > Log total pension elements
        sb.AppendLine($"Total Pension Elements: {preCommutationPensionElements.Sum(element => element.Value)}");
        
        Log.Information(sb.ToString());
        sb.Clear();

            // > Log commutation factors
        sb.AppendLine("Commutation Factors Used:");
        foreach (var pensionElement in commutationFactorsForElements)
        {
            sb.AppendLine($"Element: {pensionElement.Key}, Commutation Factor: {pensionElement.Value}");
        }
        
        Log.Information(sb.ToString());
        sb.Clear();

            // > Log maximum output summary
        sb.AppendLine("Maximum Output Summary:");
        sb.AppendLine($"Spouses Retirement Pension: {maximumPensionOption.SpousePensionValues.Sum(element => element.Value)}");
        sb.AppendLine($"DB Cash: {maximumPensionOption.TotalCash - DcFund}");
        sb.AppendLine($"AVC Cash: {DcFund}");
        sb.AppendLine($"Total Cash: {maximumPensionOption.TotalCash}");
        sb.AppendLine($"Residual: {maximumPensionOption.ResidualPensionValues.Sum(element => element.Value)}");
        
        Log.Information(sb.ToString());
        sb.Clear();

            // > Log target output summary (if applicable)
        if (Parameters.TargetCashAmount > 0.0m)
        {
            sb.AppendLine("Target Output Summary:");
            sb.AppendLine($"Spouses Retirement Pension: {targetPensionOption.SpousePensionValues.Sum(element => element.Value)}");
            sb.AppendLine($"DB Cash: {targetPensionOption.TotalCash - DcFund}");
            sb.AppendLine($"AVC Cash: {DcFund}");
            sb.AppendLine($"Total Cash: {targetPensionOption.TotalCash}");
            sb.AppendLine($"Residual: {targetPensionOption.ResidualPensionValues.Sum(element => element.Value)}");
    
            Log.Information(sb.ToString());
        }

        //// > 5. Assign results values to the pre-set "Calculate()" Results List (see "OUTPUT" above).
            // > CODED BELOW: Assignment of "preCommutationPensionElements", "GmpValues", "zeroLumpSumOption", "maximumPensionOption" and "targetPensionOption" as the values for the associated output definitions.

        return new() 
        { 
            PensionElements = preCommutationPensionElements, // EXTRA INFO FOR V1 CODERS: This has previously been set to show deferred element values, incorrectly. For deferred element values, refer to Log.Debug.
            GmpValues = GmpValues,
            MinimumCashCommuted = zeroLumpSumOption,
            MaximumCashCommuted = maximumPensionOption,
            TargetCashCommuted = targetPensionOption,
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
        CalculationEndDate = Parameters.DateOfRetirement; // TODO: Set when the calculation ends. It will be DOR for A>R and DOL for A>D.
        FPSCalcDate = Parameters.DateOfRetirement; // TODO: Set the FPSCalcDate as required in the SalarySchemeModule for the salary effective date. See that module for further information.

        normalRetirementDate = Member.NormalRetirementDate;

        // TODO: Set LifetimeAllowance global dependency
        LifetimeAllowance = LifetimeAllowanceGlobalModules.GetLifetimeAllowanceLimit(Parameters.DateOfRetirement, GlobalReferenceData); // TODO: Ensure the first parameter ("EndDate" per global module) is set correctly.

        DcFund = Member.ExternalFundValues
                .GroupBy(x => x.Provider)
                .SelectMany(group =>
                group.OrderByDescending(g => g.EffectiveFrom)
                    .Take(1)
                    .Select(item => item.FundValue)).Sum(); // TODO: Consider the information in Member External Fund Values. This assumes the only information held there is DC fund values.

        GmpDate = GmpGlobalModules.GetGmpDate(Member.Person.DateOfBirth, Member.Person.Gender);

        var gmpelements = GmpGlobalModules.CalculateActiveGuaranteedMinimumPensionValues(Parameters.DateOfRetirement,
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

        MissedIncreaseFactor = GmpGlobalModules.MissedIncreaseDateOfLeaving(Parameters.DateOfRetirement,4, 1, 1, SchemeReferenceData, Member.Person.DateOfBirth.PlusYears(Member.Person.IsMale ? 65 : 60));
        GmpStandardToActualFactor = GmpGlobalModules.GmpStandardToActualDateOfLeavingFactor(Parameters.DateOfRetirement, Member.Person.DateOfBirth, Member.Person.IsMale,true);

        GmpValues = new () // Will value to DOR so uplift by GMP Uplift but not add in any ERFs or LRFs.
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

        // VALIDATION 2: Check the date of retirement is one for which there are retirement factors available (not too early or too late)

            // > TODO: Upload ERF and LRF tables under Factors > Decimal Factors, in Calculate.
            // > CODED BELOW: This code checks the Retirement Factor tables for the highest available years early/ late. 
        
        string ERFfactorTable = "ERF1"; // TODO: Set the name of the ERF table.
        decimal maxERFFactorAvailableInMonths = SchemeReferenceData.DecimalLookups(ERFfactorTable).Max(entry => entry.Year * 12 + (entry.Month)); // EXTRA INFO: Check the lists for the highest entry of years and months.

        string LRFfactorTable = "LRF1"; // TODO: Set the name of the ERF table to use as reference.
        decimal maxLRFFactorAvailableInMonths = SchemeReferenceData.DecimalLookups(LRFfactorTable).Max(entry => entry.Year * 12 + (entry.Month)); // EXTRA INFO: Check the lists for the highest entry of years and months.

        Period periodFromDesiredRetirementDateToNRD = Period.Between(Parameters.DateOfRetirement, Member.NormalRetirementDate, PeriodUnits.Months); // EXTRA INFO: Calculate period between DoR and NRD.
        decimal monthsFromDesiredRetirementDateToNRD = Math.Abs(periodFromDesiredRetirementDateToNRD.Months); // EXTRA INFO: Return decimal months to/ from NRD. Change negative late retirement value to absolute.

        if (Parameters.DateOfRetirement < Member.NormalRetirementDate && maxERFFactorAvailableInMonths < monthsFromDesiredRetirementDateToNRD) // EXTRA INFO: Early retirement error warning, if no factors available.
            {
                Log.Error($"Member has selected a retirement date {Math.Abs(periodFromDesiredRetirementDateToNRD.Years)} years and {Math.Abs(periodFromDesiredRetirementDateToNRD.Months)} months early. Factors are not available.");
            }
        else if (Parameters.DateOfRetirement > Member.NormalRetirementDate && maxLRFFactorAvailableInMonths < monthsFromDesiredRetirementDateToNRD) // EXTRA INFO: Late retirement error warning, if no factors available.
            {
                Log.Error($"Member has selected a retirement date {Math.Abs(periodFromDesiredRetirementDateToNRD.Years)} years and {Math.Abs(periodFromDesiredRetirementDateToNRD.Months)} months late. Factors are not available.");
            }
    }

    protected virtual void PostCalculationValidation(decimal zeroCashCommutedLifetimeAllowancePension, decimal CommutedPensionTotalCash) // EXTRA INFO: Indicate parameters (type and name) required for validations. These are called in section 
    {
        // VALIDATION 3: Check for potential trivial commutation. Return a warning such that results are still produced, but admin are aware to check for triv comm.    

        if (zeroCashCommutedLifetimeAllowancePension <30000.01m) 
        {
            Log.Warning ("Capital value is £30,000 or less. Potential Trivial Commutation Case.");
        }

        // VALIDATION 4: Determine whether GMP at GMP Age is covered by checking if Commuted Cash is negative. Return a warning to indicate admin check required. Note: There are further GMP restriction checks held globally.

        if (CommutedPensionTotalCash.Round(2) < 0)
        {
            Log.Warning ("PCLS has been restricted to cover GMP. In this instance GMP at GMP Age is not covered. Consider a manual calculation to determine if and when member can retire."); 
        }

        // VALIDATION 5: Check for AVCs and highlight to admin these have been found. This code assumed the DCFund location is storing AVCs. Code may need to be updated if External Values contains other information.

        var AvcFund = Member.ExternalFundValues
                .GroupBy(x => x.Provider)
                .SelectMany(group => group.OrderByDescending(g => g.EffectiveFrom)
                .Take(1)
                .Select(item => item.FundValue))
                .Sum(); 

        if (AvcFund > 0)
        {
            Log.Warning ("Provider found under Membership > External Fund Values. Check if member has AVCs and whether calculation is working as expected."); 
        }

        // FURTHER VALIDATIONS: 
            // TODO: Add specific validations for the Scheme here.
    }

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
                // ADDITIONAL METHODS SECTION:
                
                // ADDITIONAL METHOD 1: REVALUATION CODE
                    // This code cycles through element keys and uplifts deferred element values prior to their conversion to pension elements, for example, by applying retirement factors.

                    // OUTPUT: Final defered element pension values with GMP Uplifts and scheme ERFs and LRFs to deferred element keys.

                    // INPUTS:
                        // > nonGMPElements - Lists out non-GMP elements
                        // > gmpUpliftsgmpUplift Lists out GMP elements which only get GMP uplift.
                        // > Values for uplift factors (Retirement factors, GMP uplifts)

                // ADDITIONAL METHOD 2: EXAMPLE OF BESPOKE SERVICE BUILDER CODE
                    
                    // OUTPUT: Service Units for options builder in "Years and Days 365".
                    // INPUTS: Bespoke code has been written to convert a decimal value for years into years and days. 
                        // If another service unit calculation is required and unavailable in the global service builder module, the code below will need over-writing for that scenario.
                        // Further bespoke code can also be added for other aspects of the service builder, such as service method.

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
private static HashSet<string> loggedElements = new HashSet<string>(); // EXTRA INFO: To prevent debugs duplicating

        // ADDITIONAL METHOD 1: REVALUATION CODE
public List<ElementValue> CalculateRetirementElementsA2R(List<ActiveToDeferredElementValue> deferredElementValues) 
    {
        // TODO: Consider the below groupings of elements. Organise the lists in such a way that those with the same Retirement Factor application are grouped together
            // > The below assumes non-GMP elements will receive the same ERF and LRF treatment and then GMP elements will have alternative treatment.
        var nonGMPElements = new[] {"PRE97", "POST97", "POST05", "POST09", "POST14", "POST20"}; // TODO: Set correct keys
        var gmpUpliftPRE88 = new[] {"PRE88GMP"}; // TODO: List Pre88 GMP which get GMP increments only.
        var gmpUpliftPOST88 = new[] {"POST88GMP"}; // TODO: List Post88 GMP which gets GMP increments and missed increases only.

        var finalDeferredElementValues = deferredElementValues
                    .Select(ev =>
                    {
                        var ERFLRFRetirementFactor = ReusableFunctionsSchemeModule.GetSchemeFactor(ev.Key, // EXTRA INFO: This calls the RetirementFactorBuilder as held in the ReusableFunctionsSchemeModule
                                    Member.Person.DateOfBirth, 
                                    Parameters.DateOfRetirement, // TODO: "Set to DateModule.AgeAttainedRetirementDate(Member.Person.DateOfBirth, Parameters.DateOfRetirement)" if Age Attained Basis
                                    ReusableFunctionsSchemeModule.ERFTable(true), // EXTRA INFO: "true" denotes it is an active calc
                                    ReusableFunctionsSchemeModule.LRFTable(true), // EXTRA INFO: "true" denotes it is an active calc
                                    ReusableFunctionsSchemeModule.ERFApplicationDate (Member, ev.Key, true), // EXTRA INFO: "true" denotes it is an active calc
                                    ReusableFunctionsSchemeModule.LRFApplicationDate (Member, ev.Key, true), // EXTRA INFO: "true" denotes it is an active calc
                                    SchemeReferenceData,
                                    2);

                        decimal factorToApply = 1m; // EXTRA INFO: Sets assumed Reval Factor of 1, which is overriden in circumstances below.
                        
                        if (nonGMPElements.Contains(ev.Key)) 
                            {
                                factorToApply = ERFLRFRetirementFactor; 
                                // TODO: This example sets ERFs and LRFs for non-GMP elements. Amend as appropriate.
                            }

                        if (gmpUpliftPRE88.Contains(ev.Key)) 
                            {
                                factorToApply = ReusableFunctionsSchemeModule.GMPMissedIncrease(Member, Parameters.DateOfRetirement, SchemeReferenceData); 
                                // TODO: This example only applies GMP increments to PRE88GMP elements. Amend if GMP elements get ERFs/ LRFs or the higher of several options.
                            }
                        
                        if (gmpUpliftPOST88.Contains(ev.Key))
                            {
                                factorToApply = ReusableFunctionsSchemeModule.GMPMissedIncrease(Member, Parameters.DateOfRetirement, SchemeReferenceData) *
                                                ReusableFunctionsSchemeModule.GmpIncrements(Member, Parameters.DateOfRetirement);
                                // TODO: This example applies GMP increments and missed increases to PRE88GMP elements. Amend if GMP elements get ERFs/ LRFs or the higher of several options.
                            }

        // INFORMATION LOGS: The below code is for the sake of logging information and does not further impact the calculation.
        string DeferredToRetiredFigures =
        $"Deferred Element Key: {ev.Key} " +
        $"Deferred Element Starting Value: {ev.Value} " +
        $"Retirement Factor or GMP Uplift Factor Applied: {factorToApply} " +
        $"Revalued Deferred Element: {(ev.Value * factorToApply).Round() } " ;

        string elementIdentifier = ev.Key;
        if (!loggedElements.Contains(elementIdentifier) && ev.Value > 0.0m)  
        {
        Log.Debug(DeferredToRetiredFigures, "Deferred Element Valuation");
        loggedElements.Add(elementIdentifier);
        } 

        // RETURN UPLIFTED ELEMENT VALUES
                    return new ElementValue(ev.Key, (ev.Value * factorToApply).Round());
                    })
                    .ToList();
         
        return finalDeferredElementValues;
    }

        // ADDITIONAL METHOD 1: REVALUATION CODE
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
            // EXTRA INFO: Based on spec asking for "Years and Days 365". If specific leap years require calculating, this will need updating to look specifically at the start and end date.
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
