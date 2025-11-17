using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EShellULoadSetTools.Helpers.SAFEHelpers;
using EShellULoadSetTools.Services;
using EShellULoadSetTools.Models;

namespace EShellULoadSetTools.ViewModels
{
    /// <summary>
    /// ViewModel used by the Transfer ULoadSet Assignment window.
    /// Maintains the selected floor names from ETABS and the list of available load sets.
    /// </summary>
    public class TransferULoadSetAssignmentViewModel : BaseViewModel
    {
        private readonly IEtabsConnectionService _etabsConnectionService;
        private readonly ICSISafeConnectionService? _safeConnectionService;

        /// <summary>
        /// Floors (area objects) selected by the user in ETABS.
        /// </summary>
        public ObservableCollection<SlabAssignmentRow> SelectedFloors { get; } = new();

        /// <summary>
        /// Available shell uniform load set names that can be assigned.
        /// </summary>
        public ObservableCollection<string> AvailableLoadSets { get; } = new();

        public TransferULoadSetAssignmentViewModel(
            IEtabsConnectionService etabsConnectionService,
            IEnumerable<string> loadSetNames,
            ICSISafeConnectionService? safeConnectionService = null)
        {
            _etabsConnectionService = etabsConnectionService ??
                throw new ArgumentNullException(nameof(etabsConnectionService));
            _safeConnectionService = safeConnectionService;

            foreach (var name in loadSetNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                AvailableLoadSets.Add(name);
            }
        }

        /// <summary>
        /// Retrieves the currently selected floor (area) objects from the ETABS model
        /// and refreshes the bound collection.
        /// </summary>
        public void RefreshSelectionFromEtabs()
        {
            SelectedFloors.Clear();

            IReadOnlyList<ShellAreaIdentifier> selectedFloors = _etabsConnectionService.GetSelectedShellAreaIdentifiers();
            var loadSetAssignments = _etabsConnectionService.GetShellAreaUniformLoadSetAssignments();
            var safeControlPointIndex = BuildSafeControlPointIndex();
            foreach (var floor in selectedFloors)
            {
                loadSetAssignments.TryGetValue(floor.UniqueName, out string? assignedLoadSet);

                string? controlPointId = _etabsConnectionService.GetShellAreaControlPointIdentifier(floor.UniqueName);
                string safeUniqueName = string.Empty;

                if (!string.IsNullOrWhiteSpace(controlPointId) &&
                    safeControlPointIndex.TryGetValue(controlPointId, out var safeMatches) &&
                    safeMatches.Count > 0)
                {
                    safeUniqueName = safeMatches[0];
                }

                SelectedFloors.Add(new SlabAssignmentRow
                {
                    EtabsGuid = floor.Guid,
                    EtabsUniqueName = floor.UniqueName,
                    EtabsLabel = floor.Label,
                    AssignedLoadSet = assignedLoadSet ?? string.Empty,
                    SafeUniqueName = safeUniqueName
                });
            }
        }

        private IReadOnlyDictionary<string, List<string>> BuildSafeControlPointIndex()
        {
            if (_safeConnectionService?.IsInitialized != true)
            {
                return new Dictionary<string, List<string>>();
            }

            try
            {
                var safeModel = _safeConnectionService.GetSafeModel();
                return SafeAreaGeometryHelper.GetAreaControlPointIndex(safeModel);
            }
            catch
            {
                return new Dictionary<string, List<string>>();
            }
        }
    }
}
