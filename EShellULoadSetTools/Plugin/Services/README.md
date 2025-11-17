# Service layering overview

This plugin currently separates the SAFE connection into two layers:

- `SafeAttachService` performs the raw attach via `SafeApiHelper.AttachToActiveSafe`, returning the `cOAPI` and `cSapModel` references.
- `CSISafeConnectionService` composes the attach helper, exposes initialization status, and wraps errors with a more descriptive message for consumers.

For ETABS, the plugin exposes `EtabsConnectionService` directly. It accepts the `cSapModel` provided by the plugin entry point and delegates data retrieval to purpose-specific helper classes such as `EtabsModelInfoHelper`, `EtabsLoadUnitHelper`, and `EtabsSelectionHelper`. If you want parity with the SAFE layering pattern, you could introduce an attach-focused helper (e.g., `EtabsAttachService`) that encapsulates obtaining the active ETABS model before passing it to `EtabsConnectionService` or another facade. The current single-layer design keeps ETABS lean because the plugin entry point already supplies the active `cSapModel`.
