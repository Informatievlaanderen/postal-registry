namespace PostalRegistry.Api.Extract.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;

    public class ApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();
        }
    }
}
