using System;
using System.Collections.Generic;
using System.Linq;
using EShellULoadSetTools.Models;
using SAFEv1;

namespace EShellULoadSetTools.Helpers.SAFEHelpers
{
    /// <summary>
    /// Writes area uniform load set assignments into the SAFE database table using the CSI DatabaseTables API.
    /// </summary>
    public static class AreaUniformLoadSetAssignmentImporter
    {
        private const string TableKey = "Area Load Assignments - Uniform Load Sets";

        public static void Import(
            cSapModel sapModel,
            IEnumerable<AreaUniformLoadSetAssignmentRow> rows)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            var rowList = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.SafeUniqueName) && !string.IsNullOrWhiteSpace(r.UniformLoadSetName))
                .GroupBy(r => r.SafeUniqueName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new AreaUniformLoadSetAssignmentRow
                {
                    SafeUniqueName = g.Key,
                    UniformLoadSetName = g.Last().UniformLoadSetName
                })
                .ToList();

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
                    int areaNameIndex = SafeDatabaseHelper.FindFieldIndex(keys, "unique", "area", "object", "name");
                    int loadSetIndex = SafeDatabaseHelper.FindFieldIndex(keys, "load set", "uniform load set", "uload set", "set");

                    if (areaNameIndex < 0 || loadSetIndex < 0)
                    {
                        throw new InvalidOperationException("SAFE table fields for area unique name or uniform load set could not be determined.");
                    }

                    int fieldCount = keys.Length;
                    string[] tableData = new string[rowList.Count * fieldCount];

                    for (int r = 0; r < rowList.Count; r++)
                    {
                        var row = rowList[r];
                        int offset = r * fieldCount;

                        tableData[offset + areaNameIndex] = row.SafeUniqueName;
                        tableData[offset + loadSetIndex] = row.UniformLoadSetName;
                    }

                    return tableData;
                });

            SafeDatabaseHelper.ApplyEditedTablesOrThrow(db, TableKey);
        }
    }
}
