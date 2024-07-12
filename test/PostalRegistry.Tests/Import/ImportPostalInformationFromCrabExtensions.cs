namespace PostalRegistry.Tests.Import
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using PostalInformation.Commands.Crab;

    public static class ImportPostalInformationFromCrabExtensions
    {
        public static ImportPostalInformationFromCrab WithSubCantonCode(this ImportPostalInformationFromCrab command, CrabSubCantonCode subCantonCode)
        {
            return new ImportPostalInformationFromCrab(
                command.PostalCode,
                command.SubCantonId,
                subCantonCode,
                command.NisCode,
                command.MunicipalityName,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
