using System;
using System.IO;
using SAFEv1;

namespace EShellULoadSetTools.Helpers.SAFEHelpers
{
    internal static class SafeModelInfoHelper
    {
        internal static string GetModelFileName(cSapModel sapModel)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));

            try
            {
                string fileName = sapModel.GetModelFilename();
                string displayName = Path.GetFileName(fileName);

                return string.IsNullOrWhiteSpace(displayName)
                    ? fileName
                    : displayName;
            }
            catch
            {
                // Ignore and fall through to fallback value.
            }

            return "(Unknown SAFE Model)";
        }

        internal static (string lengthUnit, string forceUnit, string temperatureUnit) GetPresentUnitStrings(
            cSapModel sapModel)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));

            string lengthUnit = "Length";
            string forceUnit = "Force";
            string temperatureUnit = "Temperature";

            try
            {
                eForce force = 0;
                eLength length = 0;
                eTemperature temperature = 0;

                int ret = sapModel.GetPresentUnits_2(ref force, ref length, ref temperature);
                if (ret == 0)
                {
                    lengthUnit = length.ToString();
                    forceUnit = force.ToString();
                    temperatureUnit = temperature.ToString();
                }
            }
            catch
            {
                // Ignore errors and use default placeholders above.
            }

            return (lengthUnit, forceUnit, temperatureUnit);
        }
    }
}
