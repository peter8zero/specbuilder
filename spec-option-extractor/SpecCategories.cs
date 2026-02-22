namespace Calculate.Attributes
{
    /// <summary>
    /// Constrains valid category strings for SpecOption and SpecCapability attributes.
    /// Use these constants instead of raw strings to prevent typos and category drift.
    ///
    /// Usage:
    ///   [SpecOption(Category = SpecCategories.Revaluation, Name = "CPI-Capped (s101)", ...)]
    /// </summary>
    public static class SpecCategories
    {
        public const string Revaluation = "Revaluation";
        public const string Commutation = "Commutation";
        public const string GMP = "GMP";
        public const string Transfers = "Transfers";
        public const string Builder = "Builder";
    }
}
