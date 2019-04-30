namespace PostalRegistry.Infrastructure.Modules
{
    using Autofac;
    using PostalInformation;
    using Repositories;

    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterType<PostalInformationSet>()
                .As<IPostalInformationSet>();
        }
    }
}
