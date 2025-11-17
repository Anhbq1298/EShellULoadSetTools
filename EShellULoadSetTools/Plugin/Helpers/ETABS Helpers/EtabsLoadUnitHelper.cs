using System;
using ETABSv1;

namespace EShellULoadSetTools.Helpers.ETABSHelpers
{
    internal static class EtabsLoadUnitHelper
    {
        /// <summary>
        /// Builds a display string for shell uniform load units based on the
        /// current ETABS present units (Force / Length^2).
        /// </summary>
        internal static string GetAreaLoadUnitString(cSapModel sapModel)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));

            try
            {
                eForce force = 0;
                eLength length = 0;
                eTemperature temperature = 0;

                int ret = sapModel.GetPresentUnits_2(ref force, ref length, ref temperature);
                if (ret == 0)
                {
                    string forceUnit = force.ToString();
                    string lengthUnit = length.ToString();

                    if (!string.IsNullOrWhiteSpace(forceUnit) && !string.IsNullOrWhiteSpace(lengthUnit))
                    {
                        // Example: kN/m², lb/ft², etc. The "²" character keeps the
                        // exponent visually superscripted in the UI as requested.
                        return $"{forceUnit}/{lengthUnit}\u00B2";
                    }
                }
            }
            catch
            {
                // Ignore and fall back to generic text below.
            }

            return "Force/Length\u00B2";
        }
    }
}
