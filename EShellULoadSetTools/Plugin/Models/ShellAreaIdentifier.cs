// -------------------------------------------------------------
// File    : ShellAreaIdentifier.cs
// Purpose : DTO representing an ETABS shell object selection.
// -------------------------------------------------------------

namespace EShellULoadSetTools.Models
{
    /// <summary>
    /// Identifies a selected shell (area) object in ETABS using its unique
    /// name (GUID) and label.
    /// </summary>
    public class ShellAreaIdentifier
    {
        public string Guid { get; set; } = string.Empty;

        public string UniqueName { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }
}
