// -------------------------------------------------------------
// File    : UniformLoadSetNodeViewModel.cs
// Author  : Anh Bui
// Purpose : ViewModel representing a single TreeView node.
// -------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using EShellULoadSetTools.Models;

namespace EShellULoadSetTools.ViewModels
{
    /// <summary>
    /// Represents a single node in the TreeView.
    /// Example structure:
    ///   Root node : "Shell Uniform Load Sets"
    ///   Child node: "ULoadSet1"
    /// </summary>
    public class UniformLoadSetNodeViewModel : BaseViewModel
    {
        private string _name;
        private bool _isPinned;
        private bool _isExpanded;

        /// <summary>
        /// Text shown in the TreeView.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Child nodes of this node. For this plugin, only the root has children,
        /// and each child node represents one shell uniform load set.
        /// </summary>
        public ObservableCollection<UniformLoadSetNodeViewModel> Children { get; }

        /// <summary>
        /// All records (pattern + value) that belong to this uniform load set.
        /// This is the Model data: shellUniformLoadSetName, loadSetLoadPattern, loadPatternValue.
        /// </summary>
        public ObservableCollection<ShellUniformLoadSetRecord> Records { get; }

        /// <summary>
        /// Indicates whether this node has been explicitly selected (checked) by the user
        /// so that its records appear in the detail grid together with other checked nodes.
        /// </summary>
        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                if (_isPinned == value) return;
                _isPinned = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Indicates whether the node should be shown expanded in the TreeView.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Helper property used by the View to know whether the node represents an actual
        /// load set (leaf node) and therefore should present checkboxes to the user.
        /// </summary>
        public bool HasRecords => Records.Count > 0;

        public UniformLoadSetNodeViewModel(
            string name,
            IEnumerable<ShellUniformLoadSetRecord>? records = null)
        {
            _name = name;
            Children = new ObservableCollection<UniformLoadSetNodeViewModel>();
            Records = records != null
                ? new ObservableCollection<ShellUniformLoadSetRecord>(records)
                : new ObservableCollection<ShellUniformLoadSetRecord>();
        }
    }
}
