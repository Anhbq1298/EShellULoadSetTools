// -------------------------------------------------------------
// File    : UniformLoadSetRow.cs
// Purpose : DTO representing a row in the Shell Uniform Load Sets table.
// -------------------------------------------------------------

namespace EShellULoadSetTools.Models
{
    /// <summary>
    /// Simple DTO for transferring shell uniform load set rows to SAFE.
    /// </summary>
    public class UniformLoadSetRow
    {
        public string Name { get; set; } = string.Empty;

        public string LoadPattern { get; set; } = string.Empty;

        public double LoadValue { get; set; }
    }
}
