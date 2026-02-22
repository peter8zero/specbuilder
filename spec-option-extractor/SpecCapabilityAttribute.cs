using System;

namespace Calculate.Attributes
{
    /// <summary>
    /// Marks a method as a browsable platform capability for the spec builder.
    /// Same shape as SpecOption but targets methods rather than classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SpecCapabilityAttribute : Attribute
    {
        /// <summary>
        /// The category grouping. Use SpecCategories constants (e.g. SpecCategories.Revaluation)
        /// rather than raw strings to prevent typos and category drift.
        /// </summary>
        public string Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string WhyItMatters { get; set; }
    }
}
