namespace PostalRegistry.Projections.Syndication.Municipality
{
    public enum MunicipalityEvent
    {
        MunicipalityWasRegistered,
        MunicipalityWasNamed,
        MunicipalityNameWasCleared,
        MunicipalityNameWasCorrected,
        MunicipalityNameWasCorrectedToCleared,
        MunicipalityNisCodeWasDefined,
        MunicipalityNisCodeWasCorrected,
        MunicipalityOfficialLanguageWasAdded,
        MunicipalityOfficialLanguageWasRemoved,
        MunicipalityFacilityLanguageWasAdded,
        MunicipalityFacilityLanguageWasRemoved,

        MunicipalityWasDrawn,
        MunicipalityWasMerged,
        MunicipalityGeometryWasCleared,
        MunicipalityGeometryWasCorrected,
        MunicipalityGeometryWasCorrectedToCleared,
        MunicipalityBecameCurrent,
        MunicipalityWasRetired,
        MunicipalityWasCorrectedToCurrent,
        MunicipalityWasCorrectedToRetired,
    }
}
