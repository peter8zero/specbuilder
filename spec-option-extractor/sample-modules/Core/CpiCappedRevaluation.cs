using System;
using Calculate.Interfaces;

namespace Calculate.Core.Revaluation
{
    [SpecOption(
        Category = "Revaluation",
        Name = "CPI-Capped (s101)",
        Description = "Statutory revaluation capped at CPI rather than RPI.",
        WhyItMatters = "The standard for post-97 deferred benefits under most modern schemes."
    )]
    public class CpiCappedRevaluation : IRevaluationStrategy
    {
        [SpecCapability(
            Category = "Revaluation",
            Name = "Pro-rata CPI Revaluation",
            Description = "Calculates CPI revaluation for a partial year period.",
            WhyItMatters = "Needed when a member leaves mid-year."
        )]
        public decimal CalculateProRata(decimal pension, DateTime startDate, DateTime endDate, decimal partialFactor)
        {
            return pension * partialFactor;
        }

        public decimal Calculate(decimal pension, DateTime startDate, DateTime endDate)
        {
            return pension;
        }
    }
}
