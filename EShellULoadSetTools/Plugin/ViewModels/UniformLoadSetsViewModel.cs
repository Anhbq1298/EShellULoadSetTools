// -------------------------------------------------------------
// File    : UniformLoadSetsViewModel.cs
// Author  : Anh Bui
// Purpose : Main ViewModel holding the tree and loading data from ETABS.
// -------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using EShellULoadSetTools.Helpers.SAFEHelpers;
using EShellULoadSetTools.Helpers.UnitConverter;
using EShellULoadSetTools.Models;
using EShellULoadSetTools.Services;

namespace EShellULoadSetTools.ViewModels
{
    /// <summary>
    /// Main ViewModel exposed to the View (Window).
    /// </summary>
    public class UniformLoadSetsViewModel : BaseViewModel
    {
        private readonly IEtabsConnectionService _etabsConnectionService;
        private readonly ICSISafeConnectionService? _safeConnectionService;
        private UniformLoadSetNodeViewModel? _selectedNode;
        private string _currentModelFileName = "(Unknown Model)";
        private string _currentAreaLoadUnit = "Force/Length\u00B2";
        private string _currentLengthUnit = "Length";
        private string _currentForceUnit = "Force";
        private string _currentTemperatureUnit = "Temperature";
        private string _currentSafeModelFileName = "(Unknown SAFE Model)";
        private string _currentSafeLengthUnit = "Length";
        private string _currentSafeForceUnit = "Force";
        private string _currentSafeTemperatureUnit = "Temperature";
        private readonly List<UniformLoadSetNodeViewModel> _trackedLeafNodes = new();
        private bool _isSafeModelAttached;

        /// <summary>
        /// Root nodes of the TreeView.
        /// For this plugin we will only have one root:
        /// "Shell Uniform Load Sets".
        /// </summary>
        public ObservableCollection<UniformLoadSetNodeViewModel> RootNodes { get; }

        /// <summary>
        /// Records corresponding to either the currently selected node or the set of nodes
        /// explicitly pinned by the user via the TreeView checkboxes.
        /// </summary>
        public ObservableCollection<ShellUniformLoadSetRecord> SelectedRecords { get; }

        /// <summary>
        /// Exposes the rows shown in the UI in a DTO format expected by the importer.
        /// </summary>
        public IReadOnlyList<UniformLoadSetRow> SelectedRowsForImport => SelectedRecords
            .Select(r => new UniformLoadSetRow
            {
                Name = r.ShellUniformLoadSetName,
                LoadPattern = r.LoadSetLoadPattern,
                LoadValue = r.LoadPatternValue
            })
            .ToList();

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
                RefreshSelectedRecords();
            }
        }

        /// <summary>
        /// Display name of the ETABS model currently connected to the plugin.
        /// </summary>
        public string CurrentModelFileName
        {
            get => _currentModelFileName;
            private set
            {
                if (_currentModelFileName == value) return;
                _currentModelFileName = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Display string for the active ETABS force/area units.
        /// </summary>
        public string CurrentAreaLoadUnit
        {
            get => _currentAreaLoadUnit;
            private set
            {
                if (_currentAreaLoadUnit == value) return;
                _currentAreaLoadUnit = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string CurrentLengthUnit
        {
            get => _currentLengthUnit;
            private set
            {
                if (_currentLengthUnit == value) return;
                _currentLengthUnit = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentUnitsSummary));
            }
        }

        public string CurrentForceUnit
        {
            get => _currentForceUnit;
            private set
            {
                if (_currentForceUnit == value) return;
                _currentForceUnit = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentUnitsSummary));
            }
        }

        public string CurrentTemperatureUnit
        {
            get => _currentTemperatureUnit;
            private set
            {
                if (_currentTemperatureUnit == value) return;
                _currentTemperatureUnit = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentUnitsSummary));
            }
        }

        public string CurrentUnitsSummary =>
            $"{CurrentLengthUnit}-{CurrentForceUnit}-{CurrentTemperatureUnit}";

        public string CurrentSafeModelFileName
        {
            get => _currentSafeModelFileName;
            private set
            {
                if (_currentSafeModelFileName == value) return;
                _currentSafeModelFileName = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string CurrentSafeLengthUnit
        {
            get => _currentSafeLengthUnit;
            private set
            {
                if (_currentSafeLengthUnit == value) return;
                _currentSafeLengthUnit = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentSafeUnitsSummary));
            }
        }

        public string CurrentSafeForceUnit
        {
            get => _currentSafeForceUnit;
            private set
            {
                if (_currentSafeForceUnit == value) return;
                _currentSafeForceUnit = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentSafeUnitsSummary));
            }
        }

        public string CurrentSafeTemperatureUnit
        {
            get => _currentSafeTemperatureUnit;
            private set
            {
                if (_currentSafeTemperatureUnit == value) return;
                _currentSafeTemperatureUnit = value ?? string.Empty;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentSafeUnitsSummary));
            }
        }

        public string CurrentSafeUnitsSummary =>
            $"{CurrentSafeLengthUnit}-{CurrentSafeForceUnit}-{CurrentSafeTemperatureUnit}";

        public UniformLoadSetsViewModel(
            IEtabsConnectionService etabsConnectionService,
            ICSISafeConnectionService? safeConnectionService = null)
        {
            _etabsConnectionService = etabsConnectionService ??
                throw new ArgumentNullException(nameof(etabsConnectionService));
            _safeConnectionService = safeConnectionService;
            RootNodes = new ObservableCollection<UniformLoadSetNodeViewModel>();
            SelectedRecords = new ObservableCollection<ShellUniformLoadSetRecord>();
            IsSafeModelAttached = _safeConnectionService?.IsInitialized == true;
        }

        /// <summary>
        /// Indicates whether a SAFE model is currently attached and available for transfers.
        /// </summary>
        public bool IsSafeModelAttached
        {
            get => _isSafeModelAttached;
            private set
            {
                if (_isSafeModelAttached == value) return;
                _isSafeModelAttached = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<string> GetLoadSetNames()
        {
            return GetLeafNodes()
                .Where(n => n.HasRecords)
                .Select(n => n.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public TransferULoadSetAssignmentViewModel CreateTransferAssignmentViewModel()
        {
            return new TransferULoadSetAssignmentViewModel(
                _etabsConnectionService,
                GetLoadSetNames(),
                _safeConnectionService);
        }

        public void ApplyToSafe()
        {
            if (_safeConnectionService?.IsInitialized != true)
            {
                throw new InvalidOperationException("Attach to SAFE before applying changes.");
            }

            var safeModel = _safeConnectionService.GetSafeModel();

            double areaLoadScaleFactor = GetAreaLoadScaleFactor();

            var scaledRows = SelectedRowsForImport
                .Select(r => new UniformLoadSetRow
                {
                    Name = r.Name,
                    LoadPattern = r.LoadPattern,
                    LoadValue = r.LoadValue * areaLoadScaleFactor
                })
                .ToList();

            ShellUniformLoadSetImporter.Import(safeModel, scaledRows);

            // Refresh SAFE info in case the connection changed units or model metadata.
            LoadSafeModelInfo();
        }

        public void AttachToSafe()
        {
            if (_safeConnectionService == null)
            {
                SetSafePlaceholders("(SAFE connection unavailable)");
                IsSafeModelAttached = false;
                return;
            }

            try
            {
                _safeConnectionService.Initialize();
                LoadSafeModelInfo();
                IsSafeModelAttached = _safeConnectionService.IsInitialized;
            }
            catch
            {
                SetSafePlaceholders("(Unable to attach to SAFE)");
                IsSafeModelAttached = false;
            }
        }

        /// <summary>
        /// Load the Shell Uniform Load Sets from the ETABS model provided by
        /// the connection service and populate the tree structure.
        /// </summary>
        public void LoadFromEtabs()
        {
            if (!_etabsConnectionService.IsInitialized)
                throw new InvalidOperationException("ETABS connection has not been initialized.");

            // 1) Read full model data from ETABS: one record per pattern/value.
            IReadOnlyList<ShellUniformLoadSetRecord> records =
                _etabsConnectionService.GetShellUniformLoadSetRecords();

            CurrentModelFileName = _etabsConnectionService.GetModelFileName();
            CurrentAreaLoadUnit = _etabsConnectionService.GetAreaLoadUnitString();
            var presentUnits = _etabsConnectionService.GetPresentUnitStrings();
            CurrentLengthUnit = presentUnits.lengthUnit;
            CurrentForceUnit = presentUnits.forceUnit;
            CurrentTemperatureUnit = presentUnits.temperatureUnit;

            LoadSafeModelInfo();

            // 2) Group by shellUniformLoadSetName for the tree.
            var groups = records
                .GroupBy(r => r.ShellUniformLoadSetName, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key);

            RootNodes.Clear();
            UnregisterLeafNodeHandlers();

            // Root node: "Shell Uniform Load Sets"
            var root = new UniformLoadSetNodeViewModel("Shell Uniform Load Sets");

            foreach (var group in groups)
            {
                // Each child node represents one set (ULoadSet1, ULoadSet2, ...)
                var childNode = new UniformLoadSetNodeViewModel(group.Key, group);
                childNode.IsExpanded = true;
                root.Children.Add(childNode);
                RegisterLeafNodeHandler(childNode);
            }

            root.IsExpanded = true;
            RootNodes.Add(root);

            // Optionally select the first child by default
            SelectedNode = root.Children.FirstOrDefault();
            RefreshSelectedRecords();
        }

        private void SetSafePlaceholders(string message)
        {
            CurrentSafeModelFileName = message;
            CurrentSafeLengthUnit = "Length";
            CurrentSafeForceUnit = "Force";
            CurrentSafeTemperatureUnit = "Temperature";
        }

        private void LoadSafeModelInfo()
        {
            if (_safeConnectionService?.IsInitialized != true)
            {
                IsSafeModelAttached = false;
                return;
            }

            try
            {
                var safeModel = _safeConnectionService.GetSafeModel();
                CurrentSafeModelFileName = SafeModelInfoHelper.GetModelFileName(safeModel);
                var presentUnits = SafeModelInfoHelper.GetPresentUnitStrings(safeModel);
                CurrentSafeLengthUnit = presentUnits.lengthUnit;
                CurrentSafeForceUnit = presentUnits.forceUnit;
                CurrentSafeTemperatureUnit = presentUnits.temperatureUnit;
                IsSafeModelAttached = true;
            }
            catch
            {
                // Ignore SAFE errors and keep the default placeholder text.
                IsSafeModelAttached = false;
            }
        }

        private void RegisterLeafNodeHandler(UniformLoadSetNodeViewModel node)
        {
            if (node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    RegisterLeafNodeHandler(child);
                }

                return;
            }

            node.PropertyChanged += LeafNodeOnPropertyChanged;
            _trackedLeafNodes.Add(node);
        }

        private void UnregisterLeafNodeHandlers()
        {
            foreach (var node in _trackedLeafNodes)
            {
                node.PropertyChanged -= LeafNodeOnPropertyChanged;
            }

            _trackedLeafNodes.Clear();
        }

        private void LeafNodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UniformLoadSetNodeViewModel.IsPinned))
            {
                RefreshSelectedRecords();
            }
        }

        public void SelectAllLeafNodes()
        {
            SetPinStateForLeafNodes(true);
        }

        public void DeselectAllLeafNodes()
        {
            SetPinStateForLeafNodes(false);
        }

        private void SetPinStateForLeafNodes(bool isPinned)
        {
            foreach (var node in GetLeafNodes())
            {
                if (!node.HasRecords)
                {
                    continue;
                }

                node.IsPinned = isPinned;
            }

            RefreshSelectedRecords();
        }

        private void RefreshSelectedRecords()
        {
            var nodesToDisplay = GetLeafNodes()
                .Where(n => n.IsPinned && n.HasRecords)
                .OrderBy(n => n.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            SelectedRecords.Clear();

            if (nodesToDisplay.Count == 0)
            {
                return;
            }

            foreach (var node in nodesToDisplay)
            {
                foreach (var record in node.Records)
                {
                    SelectedRecords.Add(record);
                }
            }
        }

        private IEnumerable<UniformLoadSetNodeViewModel> GetLeafNodes()
        {
            foreach (var node in RootNodes)
            {
                foreach (var leaf in GetLeafNodes(node))
                {
                    yield return leaf;
                }
            }
        }

        private IEnumerable<UniformLoadSetNodeViewModel> GetLeafNodes(UniformLoadSetNodeViewModel node)
        {
            if (node.Children.Count == 0)
            {
                yield return node;
                yield break;
            }

            foreach (var child in node.Children)
            {
                foreach (var leaf in GetLeafNodes(child))
                {
                    yield return leaf;
                }
            }
        }

        private double GetAreaLoadScaleFactor()
        {
            double forceScale = ForceUnitConverter.GetScaleFactor(CurrentForceUnit, CurrentSafeForceUnit);
            double lengthScale = LengthUnitConverter.GetScaleFactor(CurrentLengthUnit, CurrentSafeLengthUnit);

            return forceScale / Math.Pow(lengthScale, 2);
        }
    }
}
