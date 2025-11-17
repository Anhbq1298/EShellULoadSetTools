using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SAFEv1;

namespace EShellULoadSetTools.Helpers.SAFEHelpers
{
    /// <summary>
    /// Helper methods for retrieving SAFE area object geometry information.
    /// </summary>
    internal static class SafeAreaGeometryHelper
    {
        /// <summary>
        /// Builds a lookup of SAFE area objects keyed by their control point coordinate identifier.
        /// The identifier follows the pattern "[x1,y1]-[x2,y2]-...-[xn,yn]" using the order of
        /// points returned by SAFE.
        /// </summary>
        /// <param name="sapModel">Active SAFE model reference.</param>
        /// <returns>Dictionary keyed by control point identifier with one or more SAFE unique names.</returns>
        internal static IReadOnlyDictionary<string, List<string>> GetAreaControlPointIndex(cSapModel sapModel)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));

            var index = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            try
            {
                int numberAreas = 0;
                string[] areaNames = Array.Empty<string>();

                int ret = sapModel.AreaObj.GetNameList(ref numberAreas, ref areaNames);
                if (ret != 0 || numberAreas <= 0 || areaNames.Length == 0)
                {
                    return index;
                }

                foreach (string areaName in areaNames.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    string? controlPointId = GetAreaControlPointIdentifier(sapModel, areaName);
                    if (string.IsNullOrWhiteSpace(controlPointId))
                    {
                        continue;
                    }

                    if (!index.TryGetValue(controlPointId, out var names))
                    {
                        names = new List<string>();
                        index[controlPointId] = names;
                    }

                    if (!names.Contains(areaName, StringComparer.OrdinalIgnoreCase))
                    {
                        names.Add(areaName);
                    }
                }
            }
            catch
            {
                // Ignore SAFE lookup errors and return what we have.
            }

            return index;
        }

        private static string? GetAreaControlPointIdentifier(cSapModel sapModel, string areaUniqueName)
        {
            if (sapModel == null) throw new ArgumentNullException(nameof(sapModel));
            if (string.IsNullOrWhiteSpace(areaUniqueName)) return null;

            try
            {
                int numberPoints = 0;
                string[] pointNames = Array.Empty<string>();

                int ret = sapModel.AreaObj.GetPoints(areaUniqueName, ref numberPoints, ref pointNames);
                if (ret != 0 || numberPoints <= 0 || pointNames.Length == 0)
                {
                    return null;
                }

                var coordinates = new List<string>();

                for (int i = 0; i < numberPoints && i < pointNames.Length; i++)
                {
                    string pointName = pointNames[i];
                    if (string.IsNullOrWhiteSpace(pointName))
                    {
                        continue;
                    }

                    double x = 0.0, y = 0.0, z = 0.0;
                    sapModel.PointObj.GetCoordCartesian(pointName, ref x, ref y, ref z);

                    coordinates.Add(FormatCoordinate(x, y));
                }

                if (coordinates.Count == 0)
                {
                    return null;
                }

                return string.Join('-', coordinates);
            }
            catch
            {
                return null;
            }
        }

        private static string FormatCoordinate(double x, double y)
        {
            return $"[{x.ToString("G15", CultureInfo.InvariantCulture)},{y.ToString("G15", CultureInfo.InvariantCulture)}]";
        }
    }
}
