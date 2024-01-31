namespace PostalRegistry.Tests.AutoFixture
{
    using global::AutoFixture;
    using global::AutoFixture.Kernel;

    public class WithIntegerNisCode : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var nisCode = new NisCode(fixture.Create<int>().ToString());
            fixture.Register(() => nisCode);

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(nisCode.ToString()),
                    new ParameterSpecification(
                        typeof(string),
                        "nisCode")));
        }
    }

    public class WithFixedPostalCode : ICustomization
    {
        public void Customize(IFixture fixture)
        {

            var postalCode = new PostalCode("9000");

            fixture.Register(() => postalCode);

            fixture.Customizations.Add(
                new FilteringSpecimenBuilder(
                    new FixedBuilder(postalCode),
                    new ParameterSpecification(
                        typeof(string),
                        "postalCode")));
        }
    }
}
