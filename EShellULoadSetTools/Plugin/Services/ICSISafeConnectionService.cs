using SAFEv1;

namespace EShellULoadSetTools.Services
{
    /// <summary>
    /// Provides a centralized access point to the SAFE cSapModel instance using the CSI helper infrastructure.
    /// </summary>
    public interface ICSISafeConnectionService
    {
        /// <summary>
        /// Attempts to attach to the active SAFE instance and cache the helper objects.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Indicates whether the SAFE helper references were successfully initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Returns the active SAFE ETABSObject reference previously retrieved via the helper API.
        /// </summary>
        cOAPI GetSafeObject();

        /// <summary>
        /// Returns the SAFE cSapModel reference retrieved from the ETABSObject.
        /// </summary>
        cSapModel GetSafeModel();
    }
}
