using System;

namespace Calculate.Core.Internal
{
    // No SpecOption — this is an internal utility class
    public class InternalHelper
    {
        public static string FormatDate(DateTime d) => d.ToString("yyyy-MM-dd");
    }
}
