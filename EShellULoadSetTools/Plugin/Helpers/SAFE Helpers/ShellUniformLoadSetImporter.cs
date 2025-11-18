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
        public static void Import(
            cSapModel sapModel,
            IEnumerable<UniformLoadSetRow> rows)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            var rowList = rows.ToList();
            if (rowList.Count == 0)
            {
                return;
            }

            var db = sapModel.DatabaseTables;

            (int tableVersion, string[] fieldKeys) tableFields = SafeDatabaseHelper.EnsureTableFields(db, TableKey);

            int tableVersion = tableFields.tableVersion;
            string[] fieldKeys = tableFields.fieldKeys;

            SafeDatabaseHelper.StageTableForEditing(
                db,
                TableKey,
                ref tableVersion,
                ref fieldKeys,
                rowList.Count,
                keys =>
                {
                    int setNameIndex = SafeDatabaseHelper.FindFieldIndex(keys, "name", "set");
                    int patternIndex = SafeDatabaseHelper.FindFieldIndex(keys, "pattern", "loadpat");
                    int valueIndex = SafeDatabaseHelper.FindFieldIndex(keys, "value", "magnitude", "val");

                    if (setNameIndex < 0 || patternIndex < 0 || valueIndex < 0)
                    {
                        throw new InvalidOperationException("SAFE table fields for name, pattern, or value could not be determined.");
                    }

                    int fieldCount = keys.Length;
                    string[] tableData = new string[rowList.Count * fieldCount];

                    for (int r = 0; r < rowList.Count; r++)
                    {
                        var row = rowList[r];
                        int offset = r * fieldCount;

                        tableData[offset + setNameIndex] = row.Name ?? string.Empty;
                        tableData[offset + patternIndex] = row.LoadPattern ?? string.Empty;
                        tableData[offset + valueIndex] = row.LoadValue.ToString(CultureInfo.InvariantCulture);
                    }

                    return tableData;
                });

            SafeDatabaseHelper.ApplyEditedTablesOrThrow(db, TableKey);
        }

    }
}
