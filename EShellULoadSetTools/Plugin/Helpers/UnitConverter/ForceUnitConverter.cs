using System;
using System.Collections.Generic;

namespace EShellULoadSetTools.Helpers.UnitConverter
{
    /// <summary>
    /// Provides scale factors for converting force values between ETABS and SAFE units.
    /// </summary>
    internal static class ForceUnitConverter
    {
        private static readonly Dictionary<string, double> ForceToNewton = new(StringComparer.OrdinalIgnoreCase)
        {
            { "N", 1.0 },
            { "kN", 1_000.0 },
            { "MN", 1_000_000.0 },
            { "lb", 4.4482216152605 },
            { "kip", 4_448.2216152605 },
            { "kgf", 9.80665 },
            { "Tonf", 9_806.65 },
        };

        /// <summary>
        /// Returns the multiplier required to convert a value expressed in <paramref name="fromUnit"/>
        /// to the equivalent value in <paramref name="toUnit"/>.
        /// </summary>
        internal static double GetScaleFactor(string fromUnit, string toUnit)
        {
            double fromFactor = GetNewtonFactor(fromUnit);
            double toFactor = GetNewtonFactor(toUnit);

            return toFactor.Equals(0) ? 1.0 : fromFactor / toFactor;
        }

        private static double GetNewtonFactor(string unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
            {
                return 1.0;
            }

            return ForceToNewton.TryGetValue(unit.Trim(), out double factor) ? factor : 1.0;
        }
    }
}
