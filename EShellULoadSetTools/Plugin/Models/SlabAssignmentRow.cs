// -------------------------------------------------------------
// File    : SlabAssignmentRow.cs
// Purpose : View model row for transferring slab assignments.
// -------------------------------------------------------------

namespace EShellULoadSetTools.Models
{
    /// <summary>
    /// Represents a slab row displayed in the Transfer ULoadSet Assignment grid.
    /// </summary>
    public class SlabAssignmentRow
    {
        public string EtabsGuid { get; set; } = string.Empty;

        public string EtabsUniqueName { get; set; } = string.Empty;

        public string EtabsLabel { get; set; } = string.Empty;

        public string AssignedLoadSet { get; set; } = string.Empty;

        public string SafeUniqueName { get; set; } = string.Empty;
    }
}
