using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EShellULoadSetTools.Services;

namespace EShellULoadSetTools.ViewModels
{
    /// <summary>
    /// ViewModel used by the Transfer ULoadSet Assignment window.
    /// Maintains the selected floor names from ETABS and the list of available load sets.
    /// </summary>
    public class TransferULoadSetAssignmentViewModel : BaseViewModel
    {
        private readonly IEtabsConnectionService _etabsConnectionService;

        /// <summary>
        /// Floors (area objects) selected by the user in ETABS.
        /// </summary>
        public ObservableCollection<string> SelectedFloors { get; } = new();

        /// <summary>
        /// Available shell uniform load set names that can be assigned.
        /// </summary>
        public ObservableCollection<string> AvailableLoadSets { get; } = new();

        public TransferULoadSetAssignmentViewModel(
            IEtabsConnectionService etabsConnectionService,
            IEnumerable<string> loadSetNames)
        {
            _etabsConnectionService = etabsConnectionService ??
                throw new ArgumentNullException(nameof(etabsConnectionService));

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

            IReadOnlyList<string> selectedFloors = _etabsConnectionService.GetSelectedShellUniqueNames();
            foreach (var floor in selectedFloors)
            {
                SelectedFloors.Add(floor);
            }
        }
    }
}
