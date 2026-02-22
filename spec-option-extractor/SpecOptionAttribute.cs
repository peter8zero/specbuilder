using System;

namespace Calculate.Attributes
{
    /// <summary>
    /// Marks a class as a selectable option in the spec builder.
    /// Add this attribute to any calculation strategy class that should appear
    /// as a configurable option when building a pension calculation spec.
    ///
    /// Usage:
    ///   [SpecOption(
    ///       Category = "Revaluation",
    ///       Name = "CPI-Capped (s101)",
    ///       Description = "Statutory revaluation capped at CPI.",
    ///       WhyItMatters = "The standard for post-97 deferred benefits."
    ///   )]
    ///   public class CpiCappedRevaluation : IRevaluationStrategy { ... }
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SpecOptionAttribute : Attribute
    {
        /// <summary>
        /// The category grouping. Use SpecCategories constants (e.g. SpecCategories.Revaluation)
        /// rather than raw strings to prevent typos and category drift.
        /// </summary>
        public string Category { get; set; }

        /// <summary>Plain-English display name shown in the spec builder UI.</summary>
        public string Name { get; set; }

        /// <summary>What this option does — shown as a description in the UI.</summary>
        public string Description { get; set; }

        /// <summary>Why an analyst would care about this option. Optional.</summary>
        public string WhyItMatters { get; set; }
    }
}
