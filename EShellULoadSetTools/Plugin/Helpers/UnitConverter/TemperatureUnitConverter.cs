using System;
using System.Collections.Generic;

namespace EShellULoadSetTools.Helpers.UnitConverter
{
    /// <summary>
    /// Provides scale factors for converting temperature differences between ETABS and SAFE units.
    /// </summary>
    internal static class TemperatureUnitConverter
    {
        private static readonly Dictionary<string, double> TemperatureToCelsiusDelta = new(StringComparer.OrdinalIgnoreCase)
        {
            { "C", 1.0 },
            { "K", 1.0 },
            { "F", 5.0 / 9.0 },
        };

        /// <summary>
        /// Returns the multiplier required to convert a temperature difference expressed in
        /// <paramref name="fromUnit"/> to the equivalent difference in <paramref name="toUnit"/>.
        /// </summary>
        internal static double GetScaleFactor(string fromUnit, string toUnit)
        {
            double fromFactor = GetCelsiusDeltaFactor(fromUnit);
            double toFactor = GetCelsiusDeltaFactor(toUnit);

            return toFactor.Equals(0) ? 1.0 : fromFactor / toFactor;
        }

        private static double GetCelsiusDeltaFactor(string unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
            {
                return 1.0;
            }

            return TemperatureToCelsiusDelta.TryGetValue(unit.Trim(), out double factor) ? factor : 1.0;
        }
    }
}
