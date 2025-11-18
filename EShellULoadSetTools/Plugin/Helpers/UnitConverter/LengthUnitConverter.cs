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
        /// Returns the multiplier required to convert a value from the ETABS model units
        /// (<paramref name="etabsUnit"/>) to the equivalent value in the target
        /// <paramref name="targetUnit"/>. The ETABS unit is treated as the canonical
        /// reference for the conversion.
        /// </summary>
        internal static double GetScaleFactorFromEtabsUnit(string etabsUnit, string targetUnit)
        {
            bool etabsOk = TryGetMeterFactor(etabsUnit, out double etabsFactor);
            bool targetOk = TryGetMeterFactor(targetUnit, out double targetFactor);

            if (!etabsOk || !targetOk)
            {
                return 1.0;
            }

            return targetFactor.Equals(0) ? 1.0 : etabsFactor / targetFactor;
        }

        private static bool TryGetMeterFactor(string unit, out double factor)
        {
            if (string.IsNullOrWhiteSpace(unit))
            {
                factor = 1.0;
                return false;
            }

            return LengthToMeter.TryGetValue(unit.Trim(), out factor);
        }
    }
}
