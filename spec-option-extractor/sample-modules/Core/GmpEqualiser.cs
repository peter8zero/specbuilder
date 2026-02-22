using System;
using Calculate.Interfaces;

namespace Calculate.Core.Gmp
{
    [SpecOption(
        Category = "GMP",
        Name = "GMP Equalisation (Dual Record)",
        Description = "Applies GMP equalisation using the dual-record method per Lloyds.",
        WhyItMatters = "Required for schemes with members who have GMP service between 1990-1997."
    )]
    public class GmpEqualiser : IGmpStrategy
    {
        [SpecCapability(
            Category = "GMP",
            Name = "Anti-Franking Check",
            Description = "Checks whether excess pension above GMP is sufficient to cover GMP increases.",
            WhyItMatters = "Prevents schemes from using GMP step-ups to reduce the total pension paid."
        )]
        public bool CheckAntiFranking(decimal totalPension, decimal gmpAmount, decimal gmpIncrease)
        {
            return (totalPension - gmpAmount) >= gmpIncrease;
        }

        [SpecCapability(
            Category = "GMP",
            Name = "Section 148 Revaluation",
            Description = "Applies s148 orders to revalue GMP between leaving and GMP pension age."
        )]
        public decimal ApplySection148(decimal gmp, decimal revaluationFactor)
        {
            return gmp * revaluationFactor;
        }

        public decimal Equalise(decimal malePension, decimal femalePension)
        {
            return Math.Max(malePension, femalePension);
        }
    }
}
