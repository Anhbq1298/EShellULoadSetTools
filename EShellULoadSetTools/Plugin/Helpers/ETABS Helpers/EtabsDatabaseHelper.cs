using System;
using System.Collections.Generic;
using System.Globalization;
using ETABSv1;
using EShellULoadSetTools.Models;

namespace EShellULoadSetTools.Helpers.ETABSHelpers
{
    internal static class EtabsDatabaseHelper
    {
        /// <summary>
        /// Reads Shell Uniform Load Sets via the DatabaseTables API.
        /// Returns a list of domain model records:
        ///   shellUniformLoadSetName, loadSetLoadPattern, loadPatternValue, Unit.
        /// </summary>
        internal static List<ShellUniformLoadSetRecord> GetShellUniformLoadSetRecords(cSapModel sapModel)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));

            var result = new List<ShellUniformLoadSetRecord>();

            // Determine the display unit for shell uniform loads based on the
            // current ETABS units (Force / Length^2).
            string areaUnit = UniformLoadSetEtabsHelper.GetAreaLoadUnitString(sapModel);

            cDatabaseTables db = sapModel.DatabaseTables;

            // Internal table key; if it ever changes, check in ETABS:
            //  Display > Show Tables... > find the Shell Uniform Load Sets table.
            const string tableKey = "Shell Uniform Load Sets";

            string[] fieldKeyList = Array.Empty<string>();
            string groupName = "All";
            int tableVersion = 0;
            string[] fieldKeysIncluded = Array.Empty<string>();
            int numberRecords = 0;
            string[] tableData = Array.Empty<string>();

            int ret = db.GetTableForDisplayArray(
                tableKey,
                ref fieldKeyList,
                groupName,
                ref tableVersion,
                ref fieldKeysIncluded,
                ref numberRecords,
                ref tableData);

            if (ret != 0 || numberRecords <= 0 || fieldKeysIncluded.Length == 0)
                return result;

            int fieldCount = fieldKeysIncluded.Length;

            // Find column indices for: SetName, LoadPattern, Value
            int setNameColIndex = -1;
            int patternColIndex = -1;
            int valueColIndex = -1;

            for (int i = 0; i < fieldKeysIncluded.Length; i++)
            {
                string key = (fieldKeysIncluded[i] ?? string.Empty).ToLowerInvariant();

                // Set name
                if (setNameColIndex < 0 &&
                    (key.Contains("set") && key.Contains("name") ||
                     key == "name" ||
                     key.EndsWith(".name")))
                {
                    setNameColIndex = i;
                    continue;
                }

                // Load pattern
                if (patternColIndex < 0 &&
                    (key.Contains("pattern") || key.Contains("loadpat")))
                {
                    patternColIndex = i;
                    continue;
                }

                // Load value
                if (valueColIndex < 0 &&
                    (key.Contains("value") || key.Contains("magnitude") || key.Contains("val")))
                {
                    valueColIndex = i;
                    continue;
                }
            }

            // Fallback: if we could not find set name column, simply return empty.
            if (setNameColIndex < 0)
                return result;

            // Helper local function to safely get a cell value.
            string GetField(int recordIndex, int columnIndex)
            {
                if (columnIndex < 0) return string.Empty;

                int idx = recordIndex * fieldCount + columnIndex;
                if (idx < 0 || idx >= tableData.Length) return string.Empty;

                return tableData[idx]?.Trim() ?? string.Empty;
            }

            // Build records.
            for (int r = 0; r < numberRecords; r++)
            {
                string setName = GetField(r, setNameColIndex);
                if (string.IsNullOrWhiteSpace(setName))
                    continue;

                string pattern = GetField(r, patternColIndex);

                double value = 0.0;
                if (valueColIndex >= 0)
                {
                    string valueRaw = GetField(r, valueColIndex);
                    // Try to parse using invariant culture (dot decimal separator).
                    double.TryParse(
                        valueRaw,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out value);
                }

                var record = new ShellUniformLoadSetRecord
                {
                    ShellUniformLoadSetName = setName,
                    LoadSetLoadPattern = pattern,
                    LoadPatternValue = value,
                    Unit = areaUnit
                };

                result.Add(record);
            }

            return result;
        }

        /// <summary>
        /// Reads shell uniform load set assignments for area objects using the
        /// ETABS DatabaseTables API. Returns a dictionary keyed by area unique name.
        /// </summary>
        internal static IReadOnlyDictionary<string, string> GetAreaUniformLoadSetAssignments(cSapModel sapModel)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));

            var assignments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            cDatabaseTables db = sapModel.DatabaseTables;

            // Table key verified in ETABS under: Display > Show Tables... >
            // Area Load Assignments - Uniform Load Sets
            const string tableKey = "Area Load Assignments - Uniform Load Sets";

            string[] fieldKeyList = Array.Empty<string>();
            string groupName = "All";
            int tableVersion = 0;
            string[] fieldKeysIncluded = Array.Empty<string>();
            int numberRecords = 0;
            string[] tableData = Array.Empty<string>();

            int ret = db.GetTableForDisplayArray(
                tableKey,
                ref fieldKeyList,
                groupName,
                ref tableVersion,
                ref fieldKeysIncluded,
                ref numberRecords,
                ref tableData);

            if (ret != 0 || numberRecords <= 0 || fieldKeysIncluded.Length == 0)
                return assignments;

            int fieldCount = fieldKeysIncluded.Length;

            int areaNameColIndex = -1;
            int loadSetColIndex = -1;

            for (int i = 0; i < fieldKeysIncluded.Length; i++)
            {
                string key = (fieldKeysIncluded[i] ?? string.Empty).ToLowerInvariant();

                if (areaNameColIndex < 0 &&
                    (key.Contains("unique") && key.Contains("name") ||
                     key.Contains("area") && key.Contains("name") ||
                     key.Contains("object") && key.Contains("name") ||
                     key == "name" ||
                     key.EndsWith(".name")))
                {
                    areaNameColIndex = i;
                    continue;
                }

                if (loadSetColIndex < 0 &&
                    (key.Contains("load set") ||
                     key.Contains("uniform load set") ||
                     key.Contains("uload set") ||
                     (key.Contains("set") && !key.Contains("unique"))))
                {
                    loadSetColIndex = i;
                    continue;
                }
            }

            if (areaNameColIndex < 0)
                return assignments;

            string GetField(int recordIndex, int columnIndex)
            {
                if (columnIndex < 0) return string.Empty;

                int idx = recordIndex * fieldCount + columnIndex;
                if (idx < 0 || idx >= tableData.Length) return string.Empty;

                return tableData[idx]?.Trim() ?? string.Empty;
            }

            for (int r = 0; r < numberRecords; r++)
            {
                string areaName = GetField(r, areaNameColIndex);
                if (string.IsNullOrWhiteSpace(areaName))
                    continue;

                string loadSet = GetField(r, loadSetColIndex);

                if (!assignments.ContainsKey(areaName))
                {
                    assignments[areaName] = loadSet;
                }
            }

            return assignments;
        }
    }
}
