// -------------------------------------------------------------
// File    : UniformLoadSetEtabsHelper.cs
// Purpose : Centralized helper for ETABS Shell Uniform Load Set API calls.
// -------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ETABSv1;
using EShellULoadSetTools.Models;

namespace EShellULoadSetTools.Helpers.ETABSHelpers
{
    internal static class UniformLoadSetEtabsHelper
    {
        /// <summary>
        /// Builds a display string for shell uniform load units based on the
        /// current ETABS present units (Force / Length^2).
        /// </summary>
        internal static string GetAreaLoadUnitString(cSapModel sapModel)
        {
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
                string fileName = string.Empty;
                fileName = sapModel.GetModelFilename();

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

        /// <summary>
        /// Returns the unique names (GUIDs) and labels of area objects currently selected in ETABS.
        /// </summary>
        internal static IReadOnlyList<ShellAreaIdentifier> GetSelectedAreaIdentifiers(cSapModel sapModel)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));

            try
            {
                int numberItems = 0;
                int[] objectTypes = Array.Empty<int>();
                string[] objectNames = Array.Empty<string>();

                int ret = sapModel.SelectObj.GetSelected(ref numberItems, ref objectTypes, ref objectNames);
                if (ret != 0 || numberItems <= 0)
                {
                    return Array.Empty<ShellAreaIdentifier>();
                }

                // Only keep ETABS Area objects (documented object type = 5).
                const int documentedAreaObjectType = 5;

                var identifiers = new List<ShellAreaIdentifier>();

                for (int i = 0; i < numberItems; i++)
                {
                    if (i >= objectTypes.Length || i >= objectNames.Length)
                    {
                        continue;
                    }

                    if (objectTypes[i] != documentedAreaObjectType)
                    {
                        continue;
                    }

                    string objectName = objectNames[i];
                    if (string.IsNullOrWhiteSpace(objectName))
                    {
                        continue;
                    }

                    string uniqueName = objectName;
                    string guid = string.Empty;

                    try
                    {
                        sapModel.AreaObj.GetGUID(objectName, ref guid);
                    }
                    catch
                    {
                        // Ignore GUID retrieval failures, leave it empty.
                    }

                    string label = string.Empty;
                    try
                    {
                        string story = string.Empty;
                        sapModel.AreaObj.GetLabelFromName(objectName, ref label, ref story);
                    }
                    catch
                    {
                        // Ignore label retrieval failures, fallback below.
                    }

                    if (string.IsNullOrWhiteSpace(label))
                    {
                        label = uniqueName;
                    }

                    if (!string.IsNullOrWhiteSpace(uniqueName))
                    {
                        identifiers.Add(new ShellAreaIdentifier
                        {
                            Guid = guid,
                            UniqueName = uniqueName,
                            Label = label
                        });
                    }
                }

                return identifiers
                    .GroupBy(id => id.UniqueName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();
            }
            catch
            {
                return Array.Empty<ShellAreaIdentifier>();
            }
        }

        /// <summary>
        /// Returns the unique names (GUIDs) of area objects currently selected in ETABS.
        /// </summary>
        internal static IReadOnlyList<string> GetSelectedAreaUniqueNames(cSapModel sapModel)
        {
            return GetSelectedAreaIdentifiers(sapModel)
                .Select(identifier => identifier.UniqueName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

    }
}
