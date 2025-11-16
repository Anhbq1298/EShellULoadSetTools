using System;
using EShellULoadSetTools.Helpers.SAFEHelpers;
using SAFEv1;

namespace EShellULoadSetTools.Services
{
    /// <summary>
    /// Provides helper methods to attach to the active SAFE instance via the CSI helper API
    /// and exposes the resulting cSapModel reference.
    /// </summary>
    public class CSISafeConnectionService : ICSISafeConnectionService
    {
        private readonly cHelper _helper;
        private cOAPI? _safeApi;
        private cSapModel? _sapModel;

        public CSISafeConnectionService()
            : this(new Helper())
        {
        }

        internal CSISafeConnectionService(cHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public bool IsInitialized => _sapModel is not null;

        public void Initialize()
        {
            try
            {
                //get the active object
                //SAFE uses API infrastructure from ETABS; as a consequence, the main connection object is called the ETABSObject
                var (safeObject, sapModel) = SafeApiHelper.AttachToActiveSafe(_helper);

                _safeApi = safeObject;
                _sapModel = sapModel;
            }
            catch (Exception ex)
            {
                _safeApi = null;
                _sapModel = null;
                throw new InvalidOperationException("Unable to connect to the active SAFE instance.", ex);
            }
        }

        private cOAPI SafeApi =>
            _safeApi ?? throw new InvalidOperationException("SAFE connection has not been initialized.");

        private cSapModel SapModel =>
            _sapModel ?? throw new InvalidOperationException("SAFE connection has not been initialized.");

        public cOAPI GetSafeObject()
        {
            return SafeApi;
        }

        public cSapModel GetSafeModel()
        {
            return SapModel;
        }
    }
}
