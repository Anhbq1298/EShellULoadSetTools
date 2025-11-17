# EShellULoadSetTools

ETABS plugin to display and manage shell uniform load sets.

## Why copying ETABSv1.dll into the plugin output causes startup errors (but SAFEv1.dll does not)
- The plugin entry point must exactly match the COM types that ETABS passes in: `public void Main(ref cSapModel SapModel, ref cPluginCallback ISapPlugin)`. This signature comes from `ETABSv1.dll` and is enforced at load time. If a different copy of `ETABSv1.dll` sits next to the plugin DLL, ETABS will load that copy instead of the registered one and the COM object it hands in cannot be cast to the locally loaded `cSapModel`, producing the startup conversion error.
- SAFE is only used through the CSI Helper API after the plugin has already loaded. No SAFE types appear in the plugin entry signature, so even if `SAFEv1.dll` is copied to the output folder it does not change the COM marshaling that ETABS performs when invoking `Main`.
- Because of this difference, you must keep `<Private>false</Private>` on `ETABSv1.dll` and ensure no stray ETABS assemblies are copied beside `EShellULoadSetTools.dll`, while stray SAFE copies are harmless but still unnecessary.

