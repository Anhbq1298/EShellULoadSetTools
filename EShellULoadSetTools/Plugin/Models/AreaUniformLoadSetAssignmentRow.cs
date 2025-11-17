// -------------------------------------------------------------
// File    : AreaUniformLoadSetAssignmentRow.cs
// Purpose : Represents a SAFE area uniform load set assignment row.
// -------------------------------------------------------------

namespace EShellULoadSetTools.Models
{
    /// <summary>
    /// Represents a uniform load set assignment for a SAFE area object.
    /// </summary>
    public class AreaUniformLoadSetAssignmentRow
    {
        public string SafeUniqueName { get; set; } = string.Empty;

        public string UniformLoadSetName { get; set; } = string.Empty;
    }
}
