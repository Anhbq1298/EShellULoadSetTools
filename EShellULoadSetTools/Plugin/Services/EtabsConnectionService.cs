using System;
using System.Collections.Generic;
using ETABSv1;
using EShellULoadSetTools.Helpers.ETABSHelpers;
using EShellULoadSetTools.Models;

namespace EShellULoadSetTools.Services
{
    /// <summary>
    /// Default implementation that stores the ETABS cSapModel reference and
    /// delegates helper calls to read data from the model.
    /// </summary>
    public class EtabsConnectionService : IEtabsConnectionService
    {
        private cSapModel? _sapModel;

        public bool IsInitialized => _sapModel is not null;

        public void Initialize(cSapModel sapModel)
        {
            _sapModel = sapModel ?? throw new ArgumentNullException(nameof(sapModel));
        }

        private cSapModel SapModel =>
            _sapModel ?? throw new InvalidOperationException("ETABS connection has not been initialized.");

        public IReadOnlyList<ShellUniformLoadSetRecord> GetShellUniformLoadSetRecords()
        {
            return UniformLoadSetEtabsHelper.GetShellUniformLoadSetRecords(SapModel);
        }

        public string GetAreaLoadUnitString()
        {
            return UniformLoadSetEtabsHelper.GetAreaLoadUnitString(SapModel);
        }

        public string GetModelFileName()
        {
            return UniformLoadSetEtabsHelper.GetModelFileName(SapModel);
        }

        public (string lengthUnit, string forceUnit, string temperatureUnit) GetPresentUnitStrings()
        {
            return UniformLoadSetEtabsHelper.GetPresentUnitStrings(SapModel);
        }
    }
}
