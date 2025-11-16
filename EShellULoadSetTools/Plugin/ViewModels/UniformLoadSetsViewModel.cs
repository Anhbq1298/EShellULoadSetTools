// -------------------------------------------------------------
// File    : UniformLoadSetsViewModel.cs
// Author  : Anh Bui
// Purpose : Main ViewModel holding the tree and loading data from ETABS.
// -------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ETABSv1;
using EShellULoadSetTools.Models;

namespace EShellULoadSetTools.ViewModels
{
    /// <summary>
    /// Main ViewModel exposed to the View (Window).
    /// </summary>
    public class UniformLoadSetsViewModel : BaseViewModel
    {
        private UniformLoadSetNodeViewModel? _selectedNode;

        /// <summary>
        /// Root nodes of the TreeView.
        /// For this plugin we will only have one root:
        /// "Shell Uniform Load Sets".
        /// </summary>
        public ObservableCollection<UniformLoadSetNodeViewModel> RootNodes { get; }

        /// <summary>
        /// Currently selected node in the TreeView. This is used by the View
        /// to display the detail DataGrid for the selected shell uniform load set.
        /// </summary>
        public UniformLoadSetNodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (_selectedNode == value) return;
                _selectedNode = value;
                OnPropertyChanged();
            }
        }

        public UniformLoadSetsViewModel()
        {
            RootNodes = new ObservableCollection<UniformLoadSetNodeViewModel>();
        }

        /// <summary>
        /// Load the Shell Uniform Load Sets from the given ETABS model
        /// and populate the tree structure.
        /// </summary>
        public void LoadFromSapModel(cSapModel sapModel)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));

            // 1) Read full model data from ETABS: one record per pattern/value.
            List<ShellUniformLoadSetRecord> records = GetShellUniformLoadSetRecords(sapModel);

            // 2) Group by shellUniformLoadSetName for the tree.
            var groups = records
                .GroupBy(r => r.ShellUniformLoadSetName, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key);

            RootNodes.Clear();

            // Root node: "Shell Uniform Load Sets"
            var root = new UniformLoadSetNodeViewModel("Shell Uniform Load Sets");

            foreach (var group in groups)
            {
                // Each child node represents one set (ULoadSet1, ULoadSet2, ...)
                var childNode = new UniformLoadSetNodeViewModel(group.Key, group);
                root.Children.Add(childNode);
            }

            RootNodes.Add(root);

            // Optionally select the first child by default
            SelectedNode = root.Children.FirstOrDefault();
        }

        
        /// <summary>
        /// Reads Shell Uniform Load Sets via the DatabaseTables API.
        /// Returns a list of domain model records:
        ///   shellUniformLoadSetName, loadSetLoadPattern, loadPatternValue, Unit.
        /// </summary>
        private static List<ShellUniformLoadSetRecord> GetShellUniformLoadSetRecords(cSapModel sapModel)
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
        private static string GetAreaLoadUnitString(cSapModel sapModel)
        {
            try
            {
                eForce force = 0;
                eLength length = 0;
                eTemperature temperature = 0;

                int ret = sapModel.GetPresentUnits(ref force, ref length, ref temperature);
                if (ret == 0)
                {
                    string forceUnit = force.ToString();
                    string lengthUnit = length.ToString();

                    if (!string.IsNullOrWhiteSpace(forceUnit) && !string.IsNullOrWhiteSpace(lengthUnit))
                    {
                        // Example: kN/m^2, lb/ft^2, etc.
                        return $"{forceUnit}/{lengthUnit}^2";
                    }
                }
            }
            catch
            {
                // Ignore and fall back to generic text below.
            }

            return "Force/Length^2";
        }
    }
}
