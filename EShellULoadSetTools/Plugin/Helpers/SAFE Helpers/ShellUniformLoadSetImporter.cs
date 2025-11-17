using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EShellULoadSetTools.Models;
using SAFEv1;

namespace EShellULoadSetTools.Helpers.SAFEHelpers
{
    /// <summary>
    /// Writes Shell Uniform Load Set rows into the SAFE database table using the CSI DatabaseTables API.
    /// </summary>
    public static class ShellUniformLoadSetImporter
    {
        private const string TableKey = "Shell Uniform Load Sets";

        /// <summary>
        /// Writes the provided rows into the SAFE "Shell Uniform Load Sets" table.
        /// </summary>
        public static void Import(cSapModel sapModel, IEnumerable<UniformLoadSetRow> rows)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            var rowList = rows.ToList();
            if (rowList.Count == 0)
            {
                return;
            }

            var db = sapModel.DatabaseTables;

            // Ensure the table is available in this version of SAFE.
            int availableCount = 0;
            string[] availableTableKeys = Array.Empty<string>();
            string[] availableTableNames = Array.Empty<string>();
            int[] availableImportTypes = Array.Empty<int>();

            int availableRet = db.GetAvailableTables(
                ref availableCount,
                ref availableTableKeys,
                ref availableTableNames,
                ref availableImportTypes);

            if (availableRet != 0 || availableCount <= 0 || availableTableKeys.Length == 0)
            {
                throw new InvalidOperationException("SAFE returned no available tables.");
            }

            if (!availableTableKeys.Any(t => string.Equals(t, TableKey, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"SAFE table '{TableKey}' is not available in this model.");
            }

            int tableVersion = 0;
            int numberFields = 0;
            string[] fieldKeys = Array.Empty<string>();
            string[] fieldNames = Array.Empty<string>();
            string[] descriptions = Array.Empty<string>();
            string[] unitStrings = Array.Empty<string>();
            bool[] isImportable = Array.Empty<bool>();

            int ret = db.GetAllFieldsInTable(
                TableKey,
                ref tableVersion,
                ref numberFields,
                ref fieldKeys,
                ref fieldNames,
                ref descriptions,
                ref unitStrings,
                ref isImportable);
            if (ret != 0 || numberFields <= 0 || fieldKeys.Length == 0)
            {
                throw new InvalidOperationException($"SAFE table '{TableKey}' returned no fields.");
            }

            int setNameIndex = FindFieldIndex(fieldKeys, "name", "set");
            int patternIndex = FindFieldIndex(fieldKeys, "pattern", "loadpat");
            int valueIndex = FindFieldIndex(fieldKeys, "value", "magnitude", "val");

            if (setNameIndex < 0 || patternIndex < 0 || valueIndex < 0)
            {
                throw new InvalidOperationException("SAFE table fields for name, pattern, or value could not be determined.");
            }

            int fieldCount = fieldKeys.Length;
            string[] tableData = new string[rowList.Count * fieldCount];

            for (int r = 0; r < rowList.Count; r++)
            {
                var row = rowList[r];
                int offset = r * fieldCount;

                tableData[offset + setNameIndex] = row.Name ?? string.Empty;
                tableData[offset + patternIndex] = row.LoadPattern ?? string.Empty;
                tableData[offset + valueIndex] = row.LoadValue.ToString(CultureInfo.InvariantCulture);
            }

            string[] fieldsKeysIncluded = fieldKeys;

            ret = db.SetTableForEditingArray(
                TableKey,
                ref tableVersion,
                ref fieldsKeysIncluded,
                rowList.Count,
                ref tableData);
            if (ret != 0)
            {
                throw new InvalidOperationException($"SAFE returned error code {ret} when staging '{TableKey}'.");
            }

            bool fillImportLog = true;
            int numFatalErrors = 0;
            int numErrorMessages = 0;
            int numWarningMessages = 0;
            int numInfoMessages = 0;
            string importLog = string.Empty;

            ret = db.ApplyEditedTables(
                fillImportLog,
                ref numFatalErrors,
                ref numErrorMessages,
                ref numWarningMessages,
                ref numInfoMessages,
                ref importLog);

            if (ret != 0 || numFatalErrors > 0)
            {
                string details = string.IsNullOrWhiteSpace(importLog)
                    ? ""
                    : $" Import log: {importLog}";

                throw new InvalidOperationException($"SAFE returned error code {ret} when applying '{TableKey}'.{details}");
            }
        }

        private static int FindFieldIndex(IReadOnlyList<string> fieldKeys, params string[] tokens)
        {
            for (int i = 0; i < fieldKeys.Count; i++)
            {
                string key = fieldKeys[i] ?? string.Empty;
                string lowered = key.ToLowerInvariant();

                foreach (string token in tokens)
                {
                    if (lowered.Contains(token))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }
}
