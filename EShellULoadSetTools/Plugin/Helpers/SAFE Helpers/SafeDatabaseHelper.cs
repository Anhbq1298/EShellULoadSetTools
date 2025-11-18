using System;
using System.Collections.Generic;
using System.Linq;
using SAFEv1;

namespace EShellULoadSetTools.Helpers.SAFEHelpers
{
    internal static class SafeDatabaseHelper
    {
        internal static IReadOnlyList<string> GetAvailableTableKeys(cDatabaseTables db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            int availableCount = 0;
            string[] availableTableKeys = Array.Empty<string>();
            string[] availableTableNames = Array.Empty<string>();
            int[] availableImportTypes = Array.Empty<int>();

            int ret = db.GetAvailableTables(
                ref availableCount,
                ref availableTableKeys,
                ref availableTableNames,
                ref availableImportTypes);

            if (ret != 0 || availableCount <= 0 || availableTableKeys.Length == 0)
            {
                throw new InvalidOperationException("SAFE returned no available tables.");
            }

            return availableTableKeys;
        }

        internal static bool EnsureTableAvailable(cDatabaseTables db, string tableKey, Func<bool> createTableIfMissing)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableKey)) throw new ArgumentNullException(nameof(tableKey));

            if (IsTableAvailable(GetAvailableTableKeys(db), tableKey))
            {
                return true;
            }

            if (createTableIfMissing != null && createTableIfMissing())
            {
                return IsTableAvailable(GetAvailableTableKeys(db), tableKey);
            }

            return false;
        }

        internal static bool StageEmptyTableFromMetadata(cDatabaseTables db, string tableKey)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableKey)) throw new ArgumentNullException(nameof(tableKey));

            if (!TryGetTableFields(db, tableKey, out int tableVersion, out string[] fieldKeys))
            {
                return false;
            }

            string[] fieldsKeysIncluded = fieldKeys.ToArray();
            string[] tableData = Array.Empty<string>();

            int ret = db.SetTableForEditingArray(
                tableKey,
                ref tableVersion,
                ref fieldsKeysIncluded,
                0,
                ref tableData);

            if (ret != 0)
            {
                return false;
            }

            bool fillImportLog = false;
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

            return ret == 0 && numFatalErrors == 0;
        }

        internal static (int tableVersion, string[] fieldKeys) EnsureTableFields(cDatabaseTables db, string tableKey)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableKey)) throw new ArgumentNullException(nameof(tableKey));

            try
            {
                return GetTableFields(db, tableKey);
            }
            catch (InvalidOperationException)
            {
                if (!StageEmptyTableFromMetadata(db, tableKey))
                {
                    throw;
                }

                return GetTableFields(db, tableKey);
            }
        }

        internal static void StageTableForEditing(
            cDatabaseTables db,
            string tableKey,
            ref int tableVersion,
            ref string[] fieldKeys,
            int numberRecords,
            Func<string[], string[]> buildTableData)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableKey)) throw new ArgumentNullException(nameof(tableKey));
            if (buildTableData == null) throw new ArgumentNullException(nameof(buildTableData));

            bool retriedAfterInitialization = false;

            while (true)
            {
                string[] tableData = buildTableData(fieldKeys);
                string[] fieldsKeysIncluded = fieldKeys;

                int ret = db.SetTableForEditingArray(
                    tableKey,
                    ref tableVersion,
                    ref fieldsKeysIncluded,
                    numberRecords,
                    ref tableData);

                if (ret == 0)
                {
                    return;
                }

                if (retriedAfterInitialization)
                {
                    throw new InvalidOperationException($"SAFE returned error code {ret} when staging '{tableKey}'.");
                }

                if (!StageEmptyTableFromMetadata(db, tableKey))
                {
                    throw new InvalidOperationException($"SAFE returned error code {ret} when staging '{tableKey}'.");
                }

                (tableVersion, fieldKeys) = GetTableFields(db, tableKey);
                retriedAfterInitialization = true;
            }
        }

        internal static void ApplyEditedTablesOrThrow(cDatabaseTables db, string tableKey, bool fillImportLog = true)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableKey)) throw new ArgumentNullException(nameof(tableKey));

            int numFatalErrors = 0;
            int numErrorMessages = 0;
            int numWarningMessages = 0;
            int numInfoMessages = 0;
            string importLog = string.Empty;

            int ret = db.ApplyEditedTables(
                fillImportLog,
                ref numFatalErrors,
                ref numErrorMessages,
                ref numWarningMessages,
                ref numInfoMessages,
                ref importLog);

            if (ret != 0 || numFatalErrors > 0)
            {
                string details = string.IsNullOrWhiteSpace(importLog)
                    ? string.Empty
                    : $" Import log: {importLog}";

                throw new InvalidOperationException($"SAFE returned error code {ret} when applying '{tableKey}'.{details}");
            }
        }

        internal static (int tableVersion, string[] fieldKeys) GetTableFields(cDatabaseTables db, string tableKey)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(tableKey)) throw new ArgumentNullException(nameof(tableKey));

            if (!TryGetTableFields(db, tableKey, out int tableVersion, out string[] fieldKeys))
            {
                throw new InvalidOperationException($"SAFE table '{tableKey}' returned no fields.");
            }

            return (tableVersion, fieldKeys);
        }

        internal static int FindFieldIndex(IReadOnlyList<string> fieldKeys, params string[] tokens)
        {
            if (fieldKeys == null) throw new ArgumentNullException(nameof(fieldKeys));

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

        private static bool TryGetTableFields(
            cDatabaseTables db,
            string tableKey,
            out int tableVersion,
            out string[] fieldKeys)
        {
            tableVersion = 0;
            int numberFields = 0;
            fieldKeys = Array.Empty<string>();
            string[] fieldNames = Array.Empty<string>();
            string[] descriptions = Array.Empty<string>();
            string[] unitStrings = Array.Empty<string>();
            bool[] isImportable = Array.Empty<bool>();

            int ret = db.GetAllFieldsInTable(
                tableKey,
                ref tableVersion,
                ref numberFields,
                ref fieldKeys,
                ref fieldNames,
                ref descriptions,
                ref unitStrings,
                ref isImportable);

            return ret == 0 && numberFields > 0 && fieldKeys.Length > 0;
        }

        private static bool IsTableAvailable(IEnumerable<string> availableTableKeys, string tableKey)
        {
            return availableTableKeys?.Any(t => string.Equals(t, tableKey, StringComparison.OrdinalIgnoreCase)) == true;
        }
    }
}
