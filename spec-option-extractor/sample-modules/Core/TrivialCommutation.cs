using System;
using Calculate.Interfaces;

namespace Calculate.Core.Commutation
{
    [SpecOption(
        Category = "Commutation",
        Name = "Trivial Commutation",
        Description = "Full commutation of small pots below the trivial commutation limit.",
        WhyItMatters = "Allows members with very small benefits to take a one-off lump sum instead of a pension."
    )]
    public class TrivialCommutation : ICommutationStrategy
    {
        public decimal Commute(decimal pension, decimal factor)
        {
            return pension * factor;
        }
    }
}
