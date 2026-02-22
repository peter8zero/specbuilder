using System;
using Calculate.Interfaces;

namespace Calculate.Core.Transfers
{
    public class TransferCalculator : ITransferStrategy
    {
        [SpecCapability(
            Category = "Transfers",
            Name = "Partial CETV",
            Description = "Calculates a partial cash equivalent transfer value.",
            WhyItMatters = "Needed when a member transfers only part of their benefits."
        )]
        public decimal CalculatePartialCetv(decimal totalCetv, decimal proportion)
        {
            return totalCetv * proportion;
        }

        [SpecCapability(
            Category = "Transfers",
            Name = "Club Transfer Value",
            Description = "Calculates transfer value under the Public Sector Transfer Club."
        )]
        public decimal CalculateClubTransfer(decimal pension, decimal clubFactor)
        {
            return pension * clubFactor;
        }
    }
}
