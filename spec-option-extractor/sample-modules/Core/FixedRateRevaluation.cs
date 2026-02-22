using System;
using Calculate.Interfaces;

namespace Calculate.Core.Revaluation
{
    [SpecOption(Category = "Revaluation", Name = "Fixed Rate", Description = "Revaluation at a fixed annual rate specified in scheme rules.")]
    public class FixedRateRevaluation : IRevaluationStrategy
    {
        public decimal Calculate(decimal pension, DateTime startDate, DateTime endDate)
        {
            return pension;
        }
    }
}
