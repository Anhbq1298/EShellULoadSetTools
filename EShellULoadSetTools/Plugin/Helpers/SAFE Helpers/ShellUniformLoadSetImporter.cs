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
            string[] availableTables = Array.Empty<string>();
            db.GetAvailableTables(ref availableTables);
            if (!availableTables.Any(t => string.Equals(t, TableKey, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"SAFE table '{TableKey}' is not available in this model.");
            }

            string[] fieldKeys = Array.Empty<string>();
            int ret = db.GetAllFieldsInTable(TableKey, ref fieldKeys);
            if (ret != 0 || fieldKeys.Length == 0)
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

            ret = db.SetTableForEditingArray(TableKey, fieldKeys, rowList.Count, ref tableData);
            if (ret != 0)
            {
                throw new InvalidOperationException($"SAFE returned error code {ret} when staging '{TableKey}'.");
            }

            ret = db.ApplyEditedTables();
            if (ret != 0)
            {
                throw new InvalidOperationException($"SAFE returned error code {ret} when applying '{TableKey}'.");
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
