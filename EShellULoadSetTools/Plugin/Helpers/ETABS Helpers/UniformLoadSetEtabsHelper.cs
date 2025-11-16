// -------------------------------------------------------------
// File    : UniformLoadSetEtabsHelper.cs
// Purpose : Centralized helper for ETABS Shell Uniform Load Set API calls.
// -------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ETABSv1;
using EShellULoadSetTools.Models;

namespace EShellULoadSetTools.Helpers.ETABSHelpers
{
    internal static class UniformLoadSetEtabsHelper
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
            string areaUnit = GetAreaLoadUnitString(sapModel);

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
                int ret = sapModel.GetModelFilename(ref fileName);

                if (ret == 0 && !string.IsNullOrWhiteSpace(fileName))
                {
                    string displayName = Path.GetFileName(fileName);
                    return string.IsNullOrWhiteSpace(displayName)
                        ? fileName
                        : displayName;
                }
            }
            catch
            {
                // Ignore and fall through to the fallback text below.
            }

            return "(Unsaved Model)";
        }

    }
}
