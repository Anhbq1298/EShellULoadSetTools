// -------------------------------------------------------------
// File    : ShellUniformLoadSetRecord.cs
// Author  : Anh Bui
// Purpose : Domain model for a single Shell Uniform Load Set row.
// -------------------------------------------------------------

namespace EShellULoadSetTools.Models
{
    /// <summary>
    /// Represents one record in the Shell Uniform Load Set table.
    /// </summary>
    public class ShellUniformLoadSetRecord
    {
        /// <summary>
        /// Name of the shell uniform load set (e.g. "ULoadSet1").
        /// </summary>
        public string ShellUniformLoadSetName { get; set; } = string.Empty;

        /// <summary>
        /// Load pattern contained in this set (e.g. "SDL", "LL", "LLa", ...).
        /// </summary>
        public string LoadSetLoadPattern { get; set; } = string.Empty;

        /// <summary>
        /// Value of the uniform load for this pattern (units as in ETABS model,
        /// typically kN/m2 or similar).
        /// </summary>
        public double LoadPatternValue { get; set; }

        /// <summary>
        /// Unit for the uniform load value, derived from the current ETABS units.
        /// Example: "kN/m^2" or "lb/ft^2".
        /// </summary>
        public string Unit { get; set; } = string.Empty;
    }
}
