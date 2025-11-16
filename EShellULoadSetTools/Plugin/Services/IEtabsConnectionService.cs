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
    }
}
