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
        /// Returns the multiplier required to convert a value from the ETABS model units
        /// (<paramref name="etabsUnit"/>) to the equivalent value in the target
        /// <paramref name="targetUnit"/>. The ETABS unit is treated as the canonical
        /// reference for the conversion.
        /// </summary>
        internal static double GetScaleFactorFromEtabsUnit(string etabsUnit, string targetUnit)
        {
            bool etabsOk = TryGetNewtonFactor(etabsUnit, out double etabsFactor);
            bool targetOk = TryGetNewtonFactor(targetUnit, out double targetFactor);

            if (!etabsOk || !targetOk)
            {
                return 1.0;
            }

            return targetFactor.Equals(0) ? 1.0 : etabsFactor / targetFactor;
        }

        private static bool TryGetNewtonFactor(string unit, out double factor)
        {
            if (string.IsNullOrWhiteSpace(unit))
            {
                factor = 1.0;
                return false;
            }

            return ForceToNewton.TryGetValue(unit.Trim(), out factor);
        }
    }
}
