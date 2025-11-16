// -------------------------------------------------------------
// File    : UniformLoadSetsViewModel.cs
// Author  : Anh Bui
// Purpose : Main ViewModel holding the tree and loading data from ETABS.
// -------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public UniformLoadSetsViewModel(IEtabsConnectionService etabsConnectionService)
        {
            _etabsConnectionService = etabsConnectionService ??
                throw new ArgumentNullException(nameof(etabsConnectionService));
            RootNodes = new ObservableCollection<UniformLoadSetNodeViewModel>();
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
    }
}
