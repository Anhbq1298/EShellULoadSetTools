using System;
using EShellULoadSetTools.Helpers.SAFEHelpers;
using SAFEv1;

namespace EShellULoadSetTools.Services
{
    /// <summary>
    /// Helper service that attaches to an active SAFE instance and exposes the cSapModel reference.
    /// </summary>
    public class SafeAttachService
    {
        private readonly cHelper _helper;
        private cOAPI? _safeObject;
        private cSapModel? _sapModel;

        public SafeAttachService()
            : this(new Helper())
        {
        }

        internal SafeAttachService(cHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public bool IsAttached => _sapModel is not null;

        public void Attach()
        {
            var (safeObject, sapModel) = SafeApiHelper.AttachToActiveSafe(_helper);
            _safeObject = safeObject;
            _sapModel = sapModel;
        }

        public cOAPI GetSafeObject()
        {
            return _safeObject ?? throw new InvalidOperationException("SAFE has not been attached.");
        }

        public cSapModel GetSafeModel()
        {
            return _sapModel ?? throw new InvalidOperationException("SAFE has not been attached.");
        }
    }
}
