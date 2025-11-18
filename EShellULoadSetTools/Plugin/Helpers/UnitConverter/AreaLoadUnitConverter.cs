using System;

namespace EShellULoadSetTools.Helpers.UnitConverter
{
    /// <summary>
    /// Provides helpers for converting area load magnitudes between ETABS and SAFE units.
    /// </summary>
    internal static class AreaLoadUnitConverter
    {
        /// <summary>
        /// Returns the multiplier required to convert an area load magnitude expressed in
        /// ETABS force/length^2 units to the equivalent magnitude in the target SAFE units.
        /// </summary>
        /// <remarks>
        /// The conversion maintains ETABS as the canonical source units by composing the
        /// force and length scale factors. For example, converting a value of
        /// <c>0.003 N/mm²</c> (ETABS model units N-mm-C) into <c>lb/in²</c> (SAFE units lb-in-F)
        /// yields a scale factor of approximately 145.0377, producing a SAFE magnitude of
        /// about <c>0.4351 lb/in²</c>.
        /// </remarks>
        internal static double GetScaleFactorFromEtabsUnits(
            string etabsForceUnit,
            string etabsLengthUnit,
            string targetForceUnit,
            string targetLengthUnit)
        {
            double forceScale = ForceUnitConverter.GetScaleFactorFromEtabsUnit(etabsForceUnit, targetForceUnit);
            double lengthScale = LengthUnitConverter.GetScaleFactorFromEtabsUnit(etabsLengthUnit, targetLengthUnit);

            return forceScale / Math.Pow(lengthScale, 2);
        }

        /// <summary>
        /// Converts an area load magnitude expressed in ETABS units into the corresponding
        /// SAFE magnitude using the supplied unit labels.
        /// </summary>
        internal static double ConvertFromEtabsToTarget(
            double etabsAreaLoad,
            string etabsForceUnit,
            string etabsLengthUnit,
            string targetForceUnit,
            string targetLengthUnit)
        {
            return etabsAreaLoad * GetScaleFactorFromEtabsUnits(
                etabsForceUnit,
                etabsLengthUnit,
                targetForceUnit,
                targetLengthUnit);
        }
    }
}
