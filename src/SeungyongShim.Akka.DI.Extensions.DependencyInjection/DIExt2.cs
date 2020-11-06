namespace SeungyongShim.Akka.DI.Extensions.DependencyInjection
{
    using System;
    using global::Akka.Actor;
    using Microsoft.Extensions.DependencyInjection;

    public class DIExt2 : IExtension
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public void Initialize(IServiceProvider serviceProvider) => ServiceProvider = serviceProvider;

        public IPropsFactory<T> PropsFactory<T>() where T : ActorBase =>
            ServiceProvider.GetRequiredService<IPropsFactory<T>>();
    }
}
