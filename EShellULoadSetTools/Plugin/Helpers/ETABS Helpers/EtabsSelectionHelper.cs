using System;
using System.Collections.Generic;
using System.Linq;
using ETABSv1;
using EShellULoadSetTools.Models;

namespace EShellULoadSetTools.Helpers.ETABSHelpers
{
    internal static class EtabsSelectionHelper
    {
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
