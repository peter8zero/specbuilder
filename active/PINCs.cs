public enum ProRateRoundingBasis
{
    CompleteMonths,NearestMonths 
};
 
public enum PincRoundingBasis
{
    IndividualElementRoundingRoundUp, IndividualElementRoundingRoundNearest, IndividualElementRoundingRoundDown
};

public class PincElementValue
{
    public int MemberPensionHistoryID { get; set; }

    public string Key { get; set; }

    public decimal ExistingValue { get; set; }
		
    public decimal ExistingSpousesValue { get; set; }
    
    public decimal Value { get; set; }

    public decimal SpousesValue { get; set; }

    public decimal IncreaseRate {  get; set; }

    public PincElementValue(string key)
    {
        Key = key;
    }
};

public static class ProRateFactory
{
    public static IPincProRate CreatePincProRate(ProRateRoundingBasis proRateRoundingBasis)
    {
        switch (proRateRoundingBasis) { 
            case ProRateRoundingBasis.CompleteMonths:
                return new ProRateCompleteMonths();
            default:
                throw new ArgumentException("Invalid proRatingBasis");
        }
    }
}

public class ProRateCompleteMonths : IPincProRate
{
    public decimal Calculate(LocalDate StartDate, LocalDate EndDate)
    {
        Period period = Period.Between(StartDate, EndDate);

        // Extract the number of complete months
        int completeMonths = period.Years * 12 + period.Months;

        int denominator = ((period.Years * 12) + 12);

        return (decimal)completeMonths / denominator;
    }
}

/*public class ProRateNearestMonths : IPincProRate
{
    public decimal Calculate(LocalDate StartDate, LocalDate EndDate)
    {
        //TODO:
    }
}*/

/*
    Pro rate section - deals with whether to apply a pro rate or not
*/

public class PincProRateParameters
{
    public bool ProRateGMPInFirstYear;
    public bool ProRateExcessInFirstYear;
    public bool ProRateMemberFromActive;
    public bool ProRateMemberFromDeferred;
    public bool ProRateSpouseFromActive;
    public bool ProRateSpouseFromDeferred;
    public bool ProRateSpouseFromRetirement;
}


public class ProRate : IProRate<PincProRateParameters>
{
    public bool IsProRate(bool IsSpouse, FullMemberStatus MemberStatus, LocalDate MemberRetirement, LocalDate ReviewDate, string ElementKey, PincProRateParameters pincProRateParameters)
    {
        if (IsSpouse)
        {
            return Period.Between(MemberRetirement, ReviewDate).Years < 1 && ProRateFirstYear(ElementKey, pincProRateParameters) && SpouseProRate(MemberStatus, MemberRetirement, ReviewDate, pincProRateParameters);
        }
        else
        {
            return Period.Between(MemberRetirement, ReviewDate).Years < 1 && ProRateFirstYear(ElementKey, pincProRateParameters) && MemberProRate(MemberStatus, pincProRateParameters);
        }
    }

    public bool ProRateFirstYear(string ElementKey, PincProRateParameters pincProRateParameters)
    {
        if (PincHelper.IsGMP(ElementKey))
        {
            return pincProRateParameters.ProRateGMPInFirstYear;
        }
        else
        {
            return pincProRateParameters.ProRateExcessInFirstYear;
        }
    }

    public bool SpouseProRate(FullMemberStatus MemberStatus, LocalDate MemberRetirement, LocalDate ReviewDate, PincProRateParameters pincProRateParameters)
    {
        if (FullMemberStatusExtensions.InferStatus(MemberStatus).IsActive())
        {
            return pincProRateParameters.ProRateSpouseFromActive;
        }
        else if (FullMemberStatusExtensions.InferStatus(MemberStatus).IsDeferredOrActive())
        {
            return pincProRateParameters.ProRateSpouseFromDeferred;
        }
        else if (FullMemberStatusExtensions.InferStatus(MemberStatus).IsDeferredOrActiveOrPensioner())
        {
            Period period = Period.Between(MemberRetirement, ReviewDate);
            if (period.Years < 1)
            {
                return pincProRateParameters.ProRateSpouseFromRetirement;
            }
            else
                return false;
        }
        else
        {
            return false; //default to no pro rate if no details for original member
        }
    }

    public bool MemberProRate(FullMemberStatus MemberStatus, PincProRateParameters pincProRateParameters)
    {
        if (FullMemberStatusExtensions.InferStatus(MemberStatus).IsActive())
        {
            return pincProRateParameters.ProRateMemberFromActive;
        }
        else if (FullMemberStatusExtensions.InferStatus(MemberStatus).IsDeferredOrActive())
        {
            return pincProRateParameters.ProRateMemberFromDeferred;
        }
        else
        {
            return false; //default to no pro rate if no details for original member
        }
    }
}

/*
    Rounding section. Deals with rounding the individual elements.
*/

public class PincRoundingParameters
{
    public int GMPRoundingPence;
    public int ExcessRoundingPence;
}

public static class PincRoundingFactory
{
    public static IPincRounding<PincRoundingParameters> CreatePincRounding(PincRoundingBasis pincRoundingBasis)
    {
        switch (pincRoundingBasis)
        {
            case PincRoundingBasis.IndividualElementRoundingRoundUp:
                return new PincRoundingUpToPence();
            case PincRoundingBasis.IndividualElementRoundingRoundNearest:
                return new PincRoundingToNearestPence();
            case PincRoundingBasis.IndividualElementRoundingRoundDown:
                return new PincRoundingDownToPence();
            default:
                throw new ArgumentException("Invalid pincRoundingBasis");
        }
    }
}

public class PincRoundingUpToPence : IPincRounding<PincRoundingParameters>
{
    public decimal RoundElement(decimal Amount, string ElementKey, PincRoundingParameters pincRoundingParameters)
    {
        if (PincHelper.IsGMP(ElementKey))
        {
            return Amount.RoundToBeDivisibleBy(pincRoundingParameters.GMPRoundingPence);
        }
        else
        {
            return Amount.RoundToBeDivisibleBy(pincRoundingParameters.ExcessRoundingPence);
        }
    }
}

public class PincRoundingToNearestPence : IPincRounding<PincRoundingParameters>
{
    public decimal RoundElement(decimal Amount, string ElementKey, PincRoundingParameters pincRoundingParameters)
    {
        if (PincHelper.IsGMP(ElementKey))
        {
            return Amount.RoundNearestToBeDivisibleBy(pincRoundingParameters.GMPRoundingPence);
        }
        else
        {
            return Amount.RoundNearestToBeDivisibleBy(pincRoundingParameters.ExcessRoundingPence);
        }
    }
}

public class PincRoundingDownToPence : IPincRounding<PincRoundingParameters>
{
    public decimal RoundElement(decimal Amount, string ElementKey, PincRoundingParameters pincRoundingParameters)
    {
        if (PincHelper.IsGMP(ElementKey))
        {
            return Amount.RoundDownToBeDivisibleBy(pincRoundingParameters.GMPRoundingPence);
        }
        else
        {
            return Amount.RoundDownToBeDivisibleBy(pincRoundingParameters.ExcessRoundingPence);
        }
    }
}


/*
A generic section. Not sure this is even pinc specific. Could be centralised more.
*/


public static class PincHelper
{
    public static PincMemberParameters SetMemberParameters(Member member, Member? originalMember , LocalDate reviewDate)
    {
        PincMemberParameters pincMemberParameters = new PincMemberParameters();

        pincMemberParameters.IsSpouse = IsSpouse(member);
        pincMemberParameters.ReviewDate = reviewDate;

		if (member.PensionDetails.Any())
		{
			pincMemberParameters.MemberRetirementDate = member.PensionDetails.First().EffectiveFrom;
		}
        else
        {
            Log.Warning("No PensionDetails for the member. Please check data tab settings in the first instance, and then check member data.");
        }

        if (pincMemberParameters.IsSpouse)
        {
            if (originalMember != null)
            {
                if (originalMember.PensionDetails.Any())
                {
                    pincMemberParameters.MemberRetirementDate = originalMember.PensionDetails.First().EffectiveFrom;
                }
                pincMemberParameters.MemberPreviousStatus = MembersPreviousStatus(originalMember);
            }
            else
            {
                pincMemberParameters.MemberPreviousStatus = 0; //unknown
            }
        }
        else 
        {
            pincMemberParameters.MemberPreviousStatus = PincHelper.MembersPreviousStatus(member);
        }

        return pincMemberParameters;
    }

    public static bool IsGMP(string ElementKey)
    {
        // possibly a better way to tell this? may be better to pass in an object so can change this more easily?
        return ElementKey.Contains("GMP");
    }

    public static FullMemberStatus MembersPreviousStatus(Member member)
    {
        if (member.StatusHistory.Count > 1)
        {
            return member.StatusHistory.OrderByDescending(x => x.EffectiveFrom).Skip(1).First().Status;
        }
        else
        {
            return 0; //unknown
        }
    }

    public static bool IsSpouse(Member member)
    {
        bool isSpouse = false;

        InferredSubStatus[] inferredSubStatuses = { InferredSubStatus.Dependant, InferredSubStatus.DependantChild, InferredSubStatus.DependantSpouse };

        //FullMemberStatus[] fullMemberStatuses = { FullMemberStatus.DB_Pensioner_Dependant, FullMemberStatus.DB_Pensioner_DependantChild, FullMemberStatus.DB_Pensioner_DependantSpouse, FullMemberStatus.Hybrid_Pensioner_Dependant, FullMemberStatus.Hybrid_Pensioner_DependantChild, FullMemberStatus.Hybrid_Pensioner_DependantSpouse };

        if (inferredSubStatuses.Contains(member.CurrentSubStatus))
        {
            isSpouse = true;
        }

        return isSpouse;
    }

    public static List<PincElementValue> RoundTotal(this List<PincElementValue> elements, int roundToPence, List<string> KeyOrder)
    {
        decimal total = elements.Sum(x => x.Value);
        decimal roundedTotal = total.RoundToBeDivisibleBy(roundToPence);
        decimal adjustment = roundedTotal - total;

        decimal spousesTotal = elements.Sum(x => x.SpousesValue);
        decimal roundedSpousesTotal = spousesTotal.RoundToBeDivisibleBy(roundToPence);
        decimal spousesAdjustment = roundedSpousesTotal - spousesTotal;

        if (adjustment != 0 || spousesAdjustment != 0)
        {
            string? pincElementValueToAdjust = KeyOrder.FirstOrDefault(key => elements.Any(x => x.Key == key));

            if (pincElementValueToAdjust != null)
            {
                elements.Where(x => x.Key == pincElementValueToAdjust).First().Value += adjustment;
                elements.Where(x => x.Key == pincElementValueToAdjust).First().SpousesValue += spousesAdjustment;
            }
            else
            {
                Log.Warning($"Unable to adjust total to be rounded to {roundToPence} pence");
            }
        }

        return elements;
    }
}

/*
    The main increase section. Takes an element amount and all required params.
    Returns an increased amount

    Need to decide what is being input and returned.
    Do we want just an amount, or should this be changed to an element?

    Could move the calculation of pro rate outside to improve performance as no need to repeat for every element.
*/

public class IncreaseElement<TProRateParams, TRoundingParams>
{

    private IProRate<TProRateParams> _proRate;
    private IPincRounding<TRoundingParams> _pincRounding;

    public IncreaseElement(IProRate<TProRateParams> proRate, IPincRounding<TRoundingParams> pincRounding)
    {
        _proRate = proRate;
        _pincRounding = pincRounding;
    }

    public List<PincElementValue> CalculateIncreases(
            List<MemberPensionHistory> memberPensionHistories, 
            PensionIncreaseInputs pensionIncreaseInputs, 
            PincMemberParameters pincMemberParameters, 
            decimal proRateFraction,
            TProRateParams pincProRateParameters,
            TRoundingParams pincRoundingParameters)
    {
        decimal IncreaseToApply = 0;

        List<PincElementValue> pincElementValues = new List<PincElementValue>();    

        foreach (var memberPensionHistory in memberPensionHistories)
        {
            if (pensionIncreaseInputs.PensionElementIncreaseRates.Count(x => x.ElementId == memberPensionHistory.ElementId) > 0)
            {
                IncreaseToApply = pensionIncreaseInputs.PensionElementIncreaseRates.Where(x => x.ElementId == memberPensionHistory.ElementId).First().IncreaseRate / 100;

                PincPensionParameters pincPensionParameters = new PincPensionParameters
                {
                    Amount = memberPensionHistory.MemberAmount.HasValue ? (decimal)memberPensionHistory.MemberAmount : 0,
                    SpousesAmount = memberPensionHistory.SpouseAmount.HasValue ? (decimal)memberPensionHistory.SpouseAmount : 0,
                    Increase = IncreaseToApply,
                    ElementKey = memberPensionHistory.Element.Key,
                    ProRate = proRateFraction,
                    IncreaseRateRounding = 4
                };

                PincElementValue pincElementValue = CalculateMemberAndSpouses(
                        pincPensionParameters,
                        pincMemberParameters,
                        pincProRateParameters,
                        pincRoundingParameters,
                        memberPensionHistory.Id
                    );

                pincElementValues.Add( pincElementValue );
            }
        }

        return pincElementValues;
    }

    public PincElementValue CalculateMemberAndSpouses(
            PincPensionParameters pincPensionParameters,
            PincMemberParameters pincMemberParameters,
            TProRateParams PincProRateParameters,
            TRoundingParams PincRoundingParameters,
            int originalID)
    {
        PincElementValue pincElementValue = new PincElementValue(pincPensionParameters.ElementKey);

        pincElementValue.ExistingValue = pincPensionParameters.Amount;
    	pincElementValue.ExistingSpousesValue = pincPensionParameters.SpousesAmount;

        decimal proRatedIncrease = pincPensionParameters.Increase;

        if (_proRate.IsProRate(pincMemberParameters.IsSpouse, pincMemberParameters.MemberPreviousStatus, pincMemberParameters.MemberRetirementDate, pincMemberParameters.ReviewDate, pincPensionParameters.ElementKey, PincProRateParameters))
        {
            proRatedIncrease = pincPensionParameters.Increase * pincPensionParameters.ProRate;
            proRatedIncrease = proRatedIncrease.Round(pincPensionParameters.IncreaseRateRounding);
        }

        decimal increasedAmount = pincPensionParameters.Amount + (pincPensionParameters.Amount * proRatedIncrease);
        decimal increasedSpousesAmount = pincPensionParameters.SpousesAmount + (pincPensionParameters.SpousesAmount * proRatedIncrease);

        pincElementValue.Value = _pincRounding.RoundElement(increasedAmount, pincPensionParameters.ElementKey, PincRoundingParameters);
        pincElementValue.SpousesValue = _pincRounding.RoundElement(increasedSpousesAmount, pincPensionParameters.ElementKey, PincRoundingParameters);
        pincElementValue.IncreaseRate = proRatedIncrease;
        pincElementValue.MemberPensionHistoryID = originalID;

        return pincElementValue;
    }

    [Obsolete("This is now obsolete, use the version that returns PincElementValue.")]
    public (decimal ProRatedIncrease, decimal IncreasedAmount) Calculate(
            PincPensionParameters pincPensionParameters,
            PincMemberParameters pincMemberParameters,
            TProRateParams PincProRateParameters,
            TRoundingParams PincRoundingParameters)
    {
        decimal proRatedIncrease = pincPensionParameters.Increase;

        if (_proRate.IsProRate(pincMemberParameters.IsSpouse, pincMemberParameters.MemberPreviousStatus, pincMemberParameters.MemberRetirementDate, pincMemberParameters.ReviewDate,  pincPensionParameters.ElementKey, PincProRateParameters))
        {
            proRatedIncrease = pincPensionParameters.Increase * pincPensionParameters.ProRate;
            proRatedIncrease = proRatedIncrease.Round(pincPensionParameters.IncreaseRateRounding);
        }

        decimal increasedAmount = pincPensionParameters.Amount + (pincPensionParameters.Amount * proRatedIncrease);

        return (proRatedIncrease, _pincRounding.RoundElement(increasedAmount, pincPensionParameters.ElementKey, PincRoundingParameters));
    }

}

public class PincPensionParameters{
    public decimal Amount;
    public decimal SpousesAmount;
    public decimal Increase;
    public string ElementKey;
    public decimal ProRate;
    public int IncreaseRateRounding;
}

public class PincMemberParameters{
    public bool IsSpouse;
    public FullMemberStatus MemberPreviousStatus;
    public LocalDate MemberRetirementDate;
    public LocalDate ReviewDate;
}


/***************************************************************************************/
/***********************Untested code - work in progress********************************/
/***************************************************************************************/

public abstract class PensionIncreaseElementBase<TPincMemberParameters> //, TParameters
{
    public string ElementKey { get; set; }

    public string Mechanism { get; set; }

	public Member Member { get; set; }

    public TPincMemberParameters PincMemberParameters { get; set; }

    public LocalDate CalculationDate { get; set; }

    public ReferenceData GlobalReferenceData { get; set; }

    public SchemeReferenceData SchemeReferenceData { get; set; }

    public PensionIncreaseInputs PensionIncreaseInputs { get; set; }

    public int ElementID { get; set; }

    public abstract decimal GetPensionIncrease();
}



public abstract class PensionIncreasesBase<TPincMemberParameters, TElementCalculation>
    where TElementCalculation : PensionIncreaseElementBase<TPincMemberParameters>
{

    public List<MemberPensionHistory> MemberPensionHistories { get; set; } = new();

	public LocalDate CalculationDate { get; set; }

	public Member Member { get; set; }

    public TPincMemberParameters PincMemberParameters { get; set; }

    public ReferenceData GlobalReferenceData { get; set; }

    public SchemeReferenceData SchemeReferenceData { get; set; }

    public PensionIncreaseInputs pensionIncreaseInputs { get; set; }

    public Dictionary<string, List<PensionElementIncreaseRate>> PensionIncreasesWithMechanismValue(string mechanism)
    {
        var elementCalculationTypes = GetType().Assembly.GetTypes()
            .Where(t => !t.IsAbstract)
            .Where(t => t.IsAssignableTo(typeof(TElementCalculation)))
            .Select(Activator.CreateInstance)
            .OfType<TElementCalculation>()
            .ToList();

        List<string> pensionHistoryKeys = Member.PensionHistory.Select(x=> x.Element.Key).Distinct().ToList();

        var categoryElements = elementCalculationTypes
            .Select(SetBasicProperties)
            .SelectMany(c =>
            {
                var keyAttributes = c.GetType()
                    .GetCustomAttributes(false)
                    .OfType<ElementKeyAttribute>()
                    .ToList();

                return keyAttributes
                    .Where(x => (x.Mechanism == mechanism || mechanism=="All") && pensionHistoryKeys.Contains(x.Key))
                    .Select(k =>
                    {
                        var calculation = SetElementProperties(c, k.Key);

                        decimal value = 0;

                        if (pensionIncreaseInputs != null && pensionIncreaseInputs.PensionElementIncreaseRates.Any(y => y.ElementId== calculation.ElementID))
                        {
                            value = pensionIncreaseInputs.PensionElementIncreaseRates.First().IncreaseRate;
                        }
                        else
                        {
                            value = calculation.GetPensionIncrease();
                        }

                        return new
                            {
                                Key = k.Key,
                                ElementID = c.ElementID,
                                Mechanism = k.Mechanism,
                                Calculation = c,
                                Value = value
                            };
                    }).Where(z=> z.ElementID !=0);
            }).ToList();

        var duplicatesByElementIDAndMechanism = categoryElements
            .GroupBy(x => new { x.ElementID, x.Mechanism })
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicatesByElementIDAndMechanism.Any())
        {
            foreach (var duplicate in duplicatesByElementIDAndMechanism)
            {
                Log.Error($"Multiple calculations found for Element {duplicate.Key.ElementID} Mechanism {duplicate.Key.Mechanism}");
            }

            throw new InvalidOperationException("Multiple calculations found for some Element and Mechanisms");
        }

        return categoryElements
            .GroupBy(x => x.Mechanism)
            .ToDictionary(g => g.Key, g => g
                .Select(x => new PensionElementIncreaseRate{ElementId = x.ElementID, IncreaseRate = x.Value})
                .ToList());
    }

    public List<PensionElementIncreaseRate> GetPensionIncreases()
    {
        var allMechanisms = PensionIncreasesWithMechanismValue("All");

        if (allMechanisms.Count > 1)
        {
            throw new InvalidOperationException("Multiple element calculation mechanisms found");
        }

        return allMechanisms
            .Take(1)
            .SelectMany(x => x.Value)
            .ToList();
    }

    public List<PensionElementIncreaseRate> GetPensionIncreasesWithMechanism(string mechanism)
    {
        var allMechanisms = PensionIncreasesWithMechanismValue(mechanism);

        if (allMechanisms.Count > 1)
        {
            throw new InvalidOperationException($"Multiple element calculation for mechanism {mechanism} found");
        }

        return allMechanisms
            .Take(1)
            .SelectMany(x => x.Value)
            .ToList();
    }


    private TElementCalculation SetBasicProperties(
        TElementCalculation elementCalculations)
    {
		elementCalculations.Member = Member;
        elementCalculations.CalculationDate = CalculationDate;
        elementCalculations.PincMemberParameters = PincMemberParameters;
        elementCalculations.GlobalReferenceData = GlobalReferenceData;
        elementCalculations.SchemeReferenceData = SchemeReferenceData;

        return elementCalculations;
    }

    private TElementCalculation SetElementProperties(
        TElementCalculation elementCalculations,
        string key)
    {
        if (MemberPensionHistories
            .Any(v => v.Element.Key == key))
        {
            elementCalculations.ElementID = MemberPensionHistories
            .First(v => v.Element.Key == key).ElementId.HasValue? MemberPensionHistories
            .First(v => v.Element.Key == key).ElementId.Value : 0;
        }

        return elementCalculations;
    }

}


public class SchemePINCs : PensionIncreaseCalculationBase 
{
    public override PensionIncreaseResult Calculate(){
        
        PensionIncreaseResult pensionIncreaseResult = new PensionIncreaseResult();

        pensionIncreaseResult.PensionIncreaseViewModels = CalculatePinc(Member, OriginalMember, Parameters );

        return pensionIncreaseResult;
    }
    
    private static PincProRateParameters SetPincProRateParameters()
    {
        PincProRateParameters pincProRateParameters = new PincProRateParameters();
        pincProRateParameters.ProRateGMPInFirstYear = false;
        pincProRateParameters.ProRateExcessInFirstYear = true;
        pincProRateParameters.ProRateMemberFromDeferred = true;
        pincProRateParameters.ProRateMemberFromActive = true;
        pincProRateParameters.ProRateSpouseFromActive = true;
        pincProRateParameters.ProRateSpouseFromDeferred = false;
        pincProRateParameters.ProRateSpouseFromRetirement = false;

        return pincProRateParameters;
    }

    /// <summary>
    /// Values will be rounding to 2 dp. So set these values to 1, 12 or 51 accordingly.
    /// </summary>
    /// <returns></returns>
    private static PincRoundingParameters SetPincRoundingParameters()
    {
        PincRoundingParameters pincRoundingParameters = new PincRoundingParameters();
        pincRoundingParameters.GMPRoundingPence = 52;
        pincRoundingParameters.ExcessRoundingPence = 1;

        return pincRoundingParameters;
    }

    public static List<PensionIncreaseViewModel> CalculatePinc(Member member, Member originalMember, PensionIncreaseInputs pensionIncreaseInputs)
    {

        // Set the list of element keys here in the order of preference for the rounding difference to be added
        // List<string> keyOrder = new List<string>{"POST_2005","97_TO_2005","PRE_97_EXCESS","POST_88_GMP","PRE_88_GMP","AVC"}; 
        List<string> keyOrder = new List<string>{"POST_2005","97_TO_2005","PRE_97_EXCESS","POST_88_GMP","PRE_88_GMP", "AVC"}; //needs to be an input to the process.

        LocalDate reviewDate = pensionIncreaseInputs.ReviewDate; 
        LocalDate retirementDate = member.PensionDetails.First().EffectiveFrom;

        PincMemberParameters pincMemberParameters = PincHelper.SetMemberParameters(member, originalMember, reviewDate);

        List<PensionIncreaseViewModel> pensionIncreaseViewModels = new List<PensionIncreaseViewModel>();

        //we are only given the open elements (those that have a null effectiveto date)
        List<MemberPensionHistory> memberPensionHistories = member.PensionHistory.Where(x => x.ClosedInd == false).ToList();

        if (memberPensionHistories.Count() > 0)
        {
            IPincProRate pincProRate = ProRateFactory.CreatePincProRate(ProRateRoundingBasis.CompleteMonths);
            decimal proRateFraction = pincProRate.Calculate(member.PensionDetails.First().EffectiveFrom, reviewDate);

            IProRate<PincProRateParameters> proRate = new ProRate();
            PincProRateParameters pincProRateParameters = SetPincProRateParameters();

            // Change the paramater here to choose either total or element rounding.
            // If you need to code at a scheme level. Create a scheme class that adheres to the IPincRounding interface and set pincRounding directly to that class rather than
            // using the factory.
            IPincRounding<PincRoundingParameters> pincRounding = PincRoundingFactory.CreatePincRounding(PincRoundingBasis.IndividualElementRoundingRoundNearest);
            PincRoundingParameters pincRoundingParameters = SetPincRoundingParameters();


            IncreaseElement<PincProRateParameters, PincRoundingParameters> increaseElement = new IncreaseElement<PincProRateParameters, PincRoundingParameters>(proRate, pincRounding);

            List<PincElementValue> pincElementValues = increaseElement.CalculateIncreases(memberPensionHistories, pensionIncreaseInputs, pincMemberParameters, proRateFraction, pincProRateParameters, pincRoundingParameters);
            InterimValues.Add("pincElementValues", pincElementValues);

            pincElementValues.RoundTotal(12, keyOrder);

            foreach (var memberPensionHistory in memberPensionHistories)
            {
                PensionIncreaseViewModel pensionIncreaseViewModel = new PensionIncreaseViewModel();
                pensionIncreaseViewModel.MemberId = member.Id;
                pensionIncreaseViewModel.MemberName = $"{member.Person.Forename} {member.Person.Surname}";
                pensionIncreaseViewModel.AuroraId = member.AuroraId; // update on deployment - don't have latest nugets
                pensionIncreaseViewModel.PensionElementId = memberPensionHistory.ElementId.HasValue ? memberPensionHistory.ElementId.Value : 0;
                pensionIncreaseViewModel.ExistingElementValue = memberPensionHistory.MemberAmount.HasValue ? (decimal)memberPensionHistory.MemberAmount : 0;
                pensionIncreaseViewModel.ElementName = memberPensionHistory.Element.Name;
                //(pensionIncreaseViewModel.IncreaseRate, pensionIncreaseViewModel.NewElementValue) = pincElementValues.Where(x => x.Key == memberPensionHistory.Element.Key).Select(x => (x.IncreaseRate, x.Value)).First();
                // When deployed use this line instead to set the spouses amount.
                (pensionIncreaseViewModel.IncreaseRate, pensionIncreaseViewModel.NewElementValue, pensionIncreaseViewModel.NewSpouseElementValue) = pincElementValues.Where(x => x.Key == memberPensionHistory.Element.Key).Select(x => (x.IncreaseRate, x.Value, x.SpousesValue)).First();

                pensionIncreaseViewModels.Add(pensionIncreaseViewModel);
            }

        }
        else
        {
            Log.Error("Member has no elements to increase");
        }
        InterimValues.Add("reviewDate", reviewDate);
        return pensionIncreaseViewModels;

    }
}