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
            IEnumerable<UniformLoadSetRow> rows,
            IProgress<int>? progress = null)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            var rowList = rows.ToList();
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
                int setNameIndex = SafeDatabaseHelper.FindFieldIndex(fieldKeys, "name", "set");
                int patternIndex = SafeDatabaseHelper.FindFieldIndex(fieldKeys, "pattern", "loadpat");
                int valueIndex = SafeDatabaseHelper.FindFieldIndex(fieldKeys, "value", "magnitude", "val");

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

                    ReportProgress(progress, r + 1, rowList.Count);
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

            ReportProgress(progress, rowList.Count, rowList.Count);
        }

        private static void ReportProgress(IProgress<int>? progress, int completed, int total)
        {
            if (progress == null || total <= 0)
            {
                return;
            }

            int percent = (int)Math.Round((double)completed * 100 / total);
            progress.Report(percent);
        }

    }
}
