# SAFE Helpers

Utility classes supporting SAFE API operations and database imports.

## Classes
- **SafeApiHelper** – attaches to the active SAFE instance and retrieves the `cSapModel` reference using the CSI helper API.
- **SafeAreaGeometryHelper** – builds indices and identifiers for SAFE area objects based on control points to assist geometry lookups.
- **SafeModelInfoHelper** – extracts model metadata such as design code version, units, and active file paths from the SAFE model context.
- **SafeDatabaseHelper** – centralizes SAFE database table operations, including ensuring table availability, staging edits, and applying changes.
- **ShellUniformLoadSetImporter** – stages and writes shell uniform load set records into SAFE using shared database helper routines.
- **AreaUniformLoadSetAssignmentImporter** – imports area uniform load set assignments by mapping control-point identifiers to SAFE area objects and writing records via the database API.
