using System;
using System.IO;
using ETABSv1;

namespace EShellULoadSetTools.Helpers.ETABSHelpers
{
    internal static class EtabsModelInfoHelper
    {
        /// <summary>
        /// Retrieves the current ETABS model file name (without the full path)
        /// for display purposes. Returns a friendly fallback if the model has
        /// not been saved yet or if the API call fails.
        /// </summary>
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
                // Ignore and fall through to the fallback text below.
            }

            return "(Unsaved Model)";
        }

        /// <summary>
        /// Retrieves the current ETABS present units (length, force and temperature) as strings.
        /// </summary>
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
                // Ignore, use fallback defaults defined above.
            }

            return (lengthUnit, forceUnit, temperatureUnit);
        }
    }
}
