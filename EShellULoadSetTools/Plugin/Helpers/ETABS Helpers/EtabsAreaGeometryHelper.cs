using System;
using System.Collections.Generic;
using System.Globalization;
using ETABSv1;

namespace EShellULoadSetTools.Helpers.ETABSHelpers
{
    /// <summary>
    /// Helper methods for retrieving ETABS area object geometry information.
    /// </summary>
    internal static class EtabsAreaGeometryHelper
    {
        /// <summary>
        /// Builds a control point coordinate identifier for the specified area object using the
        /// order of points returned by ETABS. The identifier follows the pattern
        /// "[x1,y1]-[x2,y2]-...-[xn,yn]".
        /// </summary>
        /// <param name="sapModel">Active ETABS model reference.</param>
        /// <param name="areaUniqueName">ETABS area object unique name.</param>
        /// <returns>Formatted coordinate identifier or <c>null</c> when unavailable.</returns>
        internal static string? GetAreaControlPointIdentifier(cSapModel sapModel, string areaUniqueName)
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
