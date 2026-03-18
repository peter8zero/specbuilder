IFPPCalculator fppCalculator = new FPPCalculator(calculationDate, salaryType);
                fppCalculator.CalculationDate = calculationDate;
                fppCalculator.Best = Best;   //integer value
                fppCalculator.OutOf = OutOf;    //integer value
                fppCalculator.FPPMethod = new FPPSalaryOnSpecificDateMethod();
                fppCalculator.salaryType = //salary type as per in the salary history record
                FPPMethod.MinimumTotalPeriod = 
                FPPMethod.CandidateCreator = candidateCreator  //See below
                FPPMethod.ProRate = fppProRate //See below
                FPPMethod.FppDates = fppDates //See below
            
decimal fpp = fppCalculator.Calculate(salaryHistories);


// Note - if you start by using the builder, it will tell you  each of the classes it actually uses. You can then copy the code from them for the basis of any overrides.

// When doing any overrides, if you can make the code generic and reusable, we can easily port it back into global for future schemes to make use of, 
// and ensure there is a sensible option on the builder to select it.


/************************************************************************/
/****************************** FPP Method ******************************/
/************************************************************************/

public class MySchemeFPPMethod : IFPPMethod
{
    public IProRate ProRate { get; set; }
    public Period MinimumTotalPeriod { get; set; }
    public ICandidateCreator CandidateCreator { get; set; }
    public IFPPDates FPPDates { get; set; }

    public decimal Calculate(int Best, int OutOf, List<SalaryHistory> salaryHistories)
    {
       decimal fpp = 0;

        // Code whatever you need to here, as long as you return the final fpp at the end.

        //The below is taken from FPPBestMethod - and may contain some useful building blocks as a starting place.
        List<SalaryHistory> salariesToConsider = FPPStatic.SalariesInPeriod(salaryHistories, FPPDates.EarliestDate, FPPDates.EndDate);

        List<FPPCandidate> fppCandidates = CandidateCreator.PopulateCandidateDates(FPPDates.EarliestDate, FPPDates.EndDate, Best, MinimumTotalPeriod?.Years ?? 1, salariesToConsider);

        foreach (FPPCandidate candidate in fppCandidates)
        {
            CandidateCreator.PopulateFPPCandidateSalaryAndPeriod(candidate, FPPDates.EarliestDate, FPPDates.EndDate, salariesToConsider);
            ProRate.ProRateFPPCandidate(candidate);
            candidate.CalculateFPP();
        }

        decimal maxFPP = fppCandidates.Max(x => x.FPPValue);

        return maxFPP.Round(2);
    }
}

/******************************************************************************/
/****************************** CandidateCreator ******************************/
/******************************************************************************/

// This line goes before your set up of FPPCalculator
MySchemeCandidateCreator candidateCreator = new MySchemeCandidateCreator();

// An FPP candidate is any period that could be considered for an FPP. This class/function should return a list of FPPCandidates with just the start and end dates populated.
// You can replace the logic here with whatever you wish. The logic here is from YearlyCandidateCreator and just provides a set of yearly candidates
public class MySchemeCandidateCreator : CandidateCreator
{
    public override List<FPPCandidate> PopulateCandidateDates(LocalDate earliestDate, LocalDate endDate, int years, int minimumYears, List<SalaryHistory> salaryHistories)
    {
        List<FPPCandidate> yearlyCandidateDates = new List<FPPCandidate>();

        if (Period.Between(earliestDate, endDate.PlusDays(1)).Years < years && Period.Between(earliestDate, endDate.PlusDays(1)).Years > minimumYears)
        {
            years = Period.Between(earliestDate, endDate).Years;
        }

        yearlyCandidateDates.Add(new FPPCandidate { EffectiveFrom = earliestDate, EffectiveTo = earliestDate.PlusYears(years).PlusDays(-1), FPPAverageDivisor = years });

        foreach (SalaryHistory salaryHistory in salaryHistories.Where(x => x.EffectiveFrom > earliestDate && x.EffectiveFrom.PlusYears(years).PlusDays(-1) < endDate))
        {
            yearlyCandidateDates.Add(new FPPCandidate { EffectiveFrom = salaryHistory.EffectiveFrom, EffectiveTo = salaryHistory.EffectiveFrom.PlusYears(years).PlusDays(-1), FPPAverageDivisor = years });
        }

        yearlyCandidateDates.Add(new FPPCandidate { EffectiveFrom = endDate.PlusYears(-years).PlusDays(1), EffectiveTo = endDate, FPPAverageDivisor = years });

        return yearlyCandidateDates;
    }

}

/*****************************************************************************/
/********************************** ProRate **********************************/
/*****************************************************************************/

// This line goes before your set up of FPPCalculator
MySchemeProRate fppProRate = new MySchemeProRate();

public class MySchemeProRate : IProRate
{

    // The following function will need to take a candidate. It is assumed the start and end dates are populated.
    // This function need to populate ProRateNumerator and ProRateDenominator for each salary in the candidate.
    // The following logic has been taken from ProRateDays
    public FPPCandidate ProRateFPPCandidate(FPPCandidate candidate)
    {
        foreach (SalaryPeriod salaryPeriod in candidate.SalaryPeriods)
        {
            InterimValues.Add("alaryPeriod.EffectiveFrom.PlusYears(1)", salaryPeriod.EffectiveFrom.PlusYears(1));
            InterimValues.Add(" salaryPeriod.EffectiveTo", salaryPeriod.EffectiveTo);

            if (salaryPeriod.EffectiveFrom.PlusYears(1).PlusDays(-1) != salaryPeriod.EffectiveTo)
            {
                salaryPeriod.ProRateNumerator = Period.DaysBetween(salaryPeriod.EffectiveFrom, salaryPeriod.EffectiveTo.PlusDays(1));

                LocalDate salaryStartDate = salaryPeriod.Salary.EffectiveFrom;
                LocalDate salaryEndDate = salaryPeriod.Salary.EffectiveTo.Value;

                if (candidate.EffectiveTo == salaryEndDate || (candidate.EffectiveFrom < salaryStartDate && candidate.EffectiveTo > salaryEndDate))
                {
                    salaryEndDate = salaryPeriod.Salary.EffectiveFrom.PlusYears(1).PlusDays(-1);
                }

                if (candidate.EffectiveFrom == salaryStartDate)
                {
                    salaryStartDate = salaryPeriod.Salary.EffectiveTo.Value.PlusYears(-1).PlusDays(1);
                }

                salaryPeriod.ProRateDenominator = Period.DaysBetween(salaryStartDate, salaryEndDate.PlusDays(1));
            }
            else
            {
                salaryPeriod.ProRateNumerator = 1;
                salaryPeriod.ProRateDenominator = 1;
            }
        }

        return candidate;
    }

}

/******************************************************************************/
/********************************** FPPDates **********************************/
/******************************************************************************/

// For FPP dates, you can change the inputs to the constructor (the function named after the class - in this case: MySchemeFPPDates)
// The logic can then be whatever you need it to be.
// EarliestDate needs to be the earliest possible cut off date for any FPP calculation
// EndDate needs to be the actual calculation date for the FPP

// Ad in this example, if you are deducting a number of years to get the earliest date, remember to add a day back on, or pro rating could be out by a day.

// This line goes before your set up of FPPCalculator. Note, inputs can be whatever you want so this line can change
MySchemeFPPDates fppDates = new MySchemeFPPDates(calculationDate, Member);

 public class MySchemeFPPDates : IFPPDates
 {
     private readonly LocalDate _earliestDate;
     private readonly LocalDate _endDate;

     public MySchemeFPPDates(LocalDate calculationDate, Member member)
     {
         _earliestDate = calculationDate.PlusYears(-OutOf).PlusDays(1);
         _endDate = calculationDate;
     }

     public LocalDate Date => _earliestDate;

     public LocalDate EarliestDate { get => _earliestDate; }

     public LocalDate EndDate { get => _endDate; }

 }
