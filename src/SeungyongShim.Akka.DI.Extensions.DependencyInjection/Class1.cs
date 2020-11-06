using System;
using Akka.Actor;
using Akka.DI.Core;
using Akka.DI.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IPropsFactory<T> where T : ActorBase
    {
        Props GetProps();
    }

    public static class ServiceProviderActorSystemExtensions
    {
        public static ActorSystem UseServiceProvider(this ActorSystem system, IServiceProvider serviceProvider)
        {
            system.RegisterExtension(DIExtension.DIExtensionProvider);
            DIExtension.DIExtensionProvider.Get(system).Initialize(serviceProvider);

            var _ = new ServiceProviderDependencyResolver(serviceProvider, system);
            return system;
        }
    }
}




