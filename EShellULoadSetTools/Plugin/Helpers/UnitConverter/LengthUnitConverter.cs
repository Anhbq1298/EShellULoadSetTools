using System;
using System.Collections.Generic;

namespace EShellULoadSetTools.Helpers.UnitConverter
{
    /// <summary>
    /// Provides scale factors for converting length values between ETABS and SAFE units.
    /// </summary>
    internal static class LengthUnitConverter
    {
        private static readonly Dictionary<string, double> LengthToMeter = new(StringComparer.OrdinalIgnoreCase)
        {
            { "m", 1.0 },
            { "cm", 0.01 },
            { "mm", 0.001 },
            { "ft", 0.3048 },
            { "in", 0.0254 },
        };

        /// <summary>
        /// Returns the multiplier required to convert a value expressed in <paramref name="fromUnit"/>
        /// to the equivalent value in <paramref name="toUnit"/>.
        /// </summary>
        internal static double GetScaleFactor(string fromUnit, string toUnit)
        {
            double fromFactor = GetMeterFactor(fromUnit);
            double toFactor = GetMeterFactor(toUnit);

            return toFactor.Equals(0) ? 1.0 : fromFactor / toFactor;
        }

        private static double GetMeterFactor(string unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
            {
                return 1.0;
            }

            return LengthToMeter.TryGetValue(unit.Trim(), out double factor) ? factor : 1.0;
        }
    }
}
