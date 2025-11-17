using System.Collections.Generic;
using ETABSv1;
using EShellULoadSetTools.Models;

namespace EShellULoadSetTools.Services
{
    /// <summary>
    /// Provides a centralized access point to the ETABS cSapModel instance and
    /// helper methods that retrieve data from the model.
    /// </summary>
    public interface IEtabsConnectionService
    {
        /// <summary>
        /// Initializes the service with the ETABS model reference provided by the plugin entry point.
        /// </summary>
        /// <param name="sapModel">Active ETABS model reference.</param>
        void Initialize(cSapModel sapModel);

        /// <summary>
        /// Indicates whether the ETABS model reference has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Retrieves all Shell Uniform Load Set records from the ETABS model.
        /// </summary>
        IReadOnlyList<ShellUniformLoadSetRecord> GetShellUniformLoadSetRecords();

        /// <summary>
        /// Returns the formatted unit string for shell uniform load values using the current ETABS units.
        /// </summary>
        string GetAreaLoadUnitString();

        /// <summary>
        /// Returns the active ETABS model file name for display purposes.
        /// </summary>
        string GetModelFileName();

        /// <summary>
        /// Returns the current ETABS present unit strings for length, force and temperature.
        /// </summary>
        (string lengthUnit, string forceUnit, string temperatureUnit) GetPresentUnitStrings();

        /// <summary>
        /// Retrieves the currently selected floor (area) object names from the active ETABS model.
        /// </summary>
        IReadOnlyList<string> GetSelectedShellUniqueNames();

        /// <summary>
        /// Retrieves the currently selected floor (area) objects and their labels from the active ETABS model.
        /// </summary>
        IReadOnlyList<ShellAreaIdentifier> GetSelectedShellAreaIdentifiers();

        /// <summary>
        /// Retrieves the shell uniform load set assignments for area objects in the ETABS model,
        /// keyed by area unique name.
        /// </summary>
        IReadOnlyDictionary<string, string> GetShellAreaUniformLoadSetAssignments();

        /// <summary>
        /// Builds the control point coordinate identifier for the specified ETABS area object.
        /// </summary>
        /// <param name="areaUniqueName">ETABS area object unique name.</param>
        /// <returns>Formatted identifier string or <c>null</c> if unavailable.</returns>
        string? GetShellAreaControlPointIdentifier(string areaUniqueName);
    }
}
