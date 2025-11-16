using System;
using SAFEv1;

namespace EShellULoadSetTools.Helpers.SAFEHelpers
{
    internal static class SafeApiHelper
    {
        internal static (cOAPI safeObject, cSapModel sapModel) AttachToActiveSafe(cHelper helper)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));

            var safeObject = helper.GetObject("CSI.SAFE.API.ETABSObject");
            if (safeObject == null)
            {
                throw new InvalidOperationException("SAFE helper returned a null ETABSObject instance.");
            }

            var sapModel = safeObject.SapModel;
            if (sapModel == null)
            {
                throw new InvalidOperationException("SAFE ETABSObject did not expose a SapModel reference.");
            }

            return (safeObject, sapModel);
        }
    }
}
