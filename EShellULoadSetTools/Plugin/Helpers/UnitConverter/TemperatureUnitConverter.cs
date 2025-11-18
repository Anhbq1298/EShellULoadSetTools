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
        /// Returns the multiplier required to convert a temperature difference from the ETABS
        /// model units (<paramref name="etabsUnit"/>) to the equivalent difference in the
        /// target <paramref name="targetUnit"/>. The ETABS unit is treated as the canonical
        /// reference for the conversion.
        /// </summary>
        internal static double GetScaleFactorFromEtabsUnit(string etabsUnit, string targetUnit)
        {
            double etabsFactor = GetCelsiusDeltaFactor(etabsUnit);
            double targetFactor = GetCelsiusDeltaFactor(targetUnit);

            return targetFactor.Equals(0) ? 1.0 : etabsFactor / targetFactor;
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
