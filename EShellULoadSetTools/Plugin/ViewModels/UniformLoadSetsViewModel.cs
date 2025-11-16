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
        private UniformLoadSetNodeViewModel? _selectedNode;
        private string _currentModelFileName = "(Unknown Model)";
        private string _currentAreaLoadUnit = "Force/Length\u00B2";
        private string _currentLengthUnit = "Length";
        private string _currentForceUnit = "Force";
        private string _currentTemperatureUnit = "Temperature";
        private readonly List<UniformLoadSetNodeViewModel> _trackedLeafNodes = new();

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

        public UniformLoadSetsViewModel(IEtabsConnectionService etabsConnectionService)
        {
            _etabsConnectionService = etabsConnectionService ??
                throw new ArgumentNullException(nameof(etabsConnectionService));
            RootNodes = new ObservableCollection<UniformLoadSetNodeViewModel>();
            SelectedRecords = new ObservableCollection<ShellUniformLoadSetRecord>();
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
                root.Children.Add(childNode);
                RegisterLeafNodeHandler(childNode);
            }

            RootNodes.Add(root);

            // Optionally select the first child by default
            SelectedNode = root.Children.FirstOrDefault();
            RefreshSelectedRecords();
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
    }
}
