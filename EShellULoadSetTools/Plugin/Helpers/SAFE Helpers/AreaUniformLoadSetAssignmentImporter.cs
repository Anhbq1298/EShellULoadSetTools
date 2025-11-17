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

        public static void Import(cSapModel sapModel, IEnumerable<AreaUniformLoadSetAssignmentRow> rows)
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

            (int tableVersion, string[] fieldKeys) tableFields;
            try
            {
                tableFields = SafeDatabaseHelper.GetTableFields(db, TableKey);
            }
            catch (InvalidOperationException)
            {
                bool initialized = SafeDatabaseHelper.StageEmptyTableFromMetadata(db, TableKey);
                if (!initialized)
                {
                    throw;
                }

                tableFields = SafeDatabaseHelper.GetTableFields(db, TableKey);
            }

            int tableVersion = tableFields.tableVersion;
            string[] fieldKeys = tableFields.fieldKeys;

            bool retriedAfterInitialization = false;

            while (true)
            {
                int areaNameIndex = SafeDatabaseHelper.FindFieldIndex(fieldKeys, "unique", "area", "object", "name");
                int loadSetIndex = SafeDatabaseHelper.FindFieldIndex(fieldKeys, "load set", "uniform load set", "uload set", "set");

                if (areaNameIndex < 0 || loadSetIndex < 0)
                {
                    throw new InvalidOperationException("SAFE table fields for area unique name or uniform load set could not be determined.");
                }

                int fieldCount = fieldKeys.Length;
                string[] tableData = new string[rowList.Count * fieldCount];

                for (int r = 0; r < rowList.Count; r++)
                {
                    var row = rowList[r];
                    int offset = r * fieldCount;

                    tableData[offset + areaNameIndex] = row.SafeUniqueName;
                    tableData[offset + loadSetIndex] = row.UniformLoadSetName;
                }

                string[] fieldsKeysIncluded = fieldKeys;

                int ret = db.SetTableForEditingArray(
                    TableKey,
                    ref tableVersion,
                    ref fieldsKeysIncluded,
                    rowList.Count,
                    ref tableData);
                if (ret == 0)
                {
                    break;
                }

                if (retriedAfterInitialization)
                {
                    throw new InvalidOperationException($"SAFE returned error code {ret} when staging '{TableKey}'.");
                }

                bool initialized = SafeDatabaseHelper.StageEmptyTableFromMetadata(db, TableKey);
                if (!initialized)
                {
                    throw new InvalidOperationException($"SAFE returned error code {ret} when staging '{TableKey}'.");
                }

                tableFields = SafeDatabaseHelper.GetTableFields(db, TableKey);
                tableVersion = tableFields.tableVersion;
                fieldKeys = tableFields.fieldKeys;
                retriedAfterInitialization = true;
            }

            bool fillImportLog = true;
            int numFatalErrors = 0;
            int numErrorMessages = 0;
            int numWarningMessages = 0;
            int numInfoMessages = 0;
            string importLog = string.Empty;

            int applyResult = db.ApplyEditedTables(
                fillImportLog,
                ref numFatalErrors,
                ref numErrorMessages,
                ref numWarningMessages,
                ref numInfoMessages,
                ref importLog);

            if (applyResult != 0 || numFatalErrors > 0)
            {
                string details = string.IsNullOrWhiteSpace(importLog)
                    ? string.Empty
                    : $" Import log: {importLog}";

                throw new InvalidOperationException($"SAFE returned error code {applyResult} when applying '{TableKey}'.{details}");
            }
        }
    }
}
