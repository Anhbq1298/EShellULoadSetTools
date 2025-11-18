# ETABS Helpers

Helper classes for interacting with ETABS models and translating ETABS data for load set tooling.

## Classes
- **EtabsAreaGeometryHelper** – gathers ETABS area object geometry and maps control point identifiers to facilitate SAFE alignment.
- **EtabsDatabaseHelper** – reads shell uniform load set records from the ETABS database tables API and converts them to domain models.
- **EtabsLoadUnitHelper** – determines appropriate area load units from the active ETABS model and supplies unit strings for conversions.
- **EtabsModelInfoHelper** – retrieves metadata about the active ETABS model, including file details and design code information.
- **EtabsSelectionHelper** – manages ETABS selection state, enabling temporary selection scopes while executing targeted operations.
