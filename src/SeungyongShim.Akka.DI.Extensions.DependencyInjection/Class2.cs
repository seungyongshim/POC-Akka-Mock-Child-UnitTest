namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Akka.Actor;

    public static class DIActorSystemAdapterExtension
    {
        public static IPropsFactory<T> PropsFactory<T>(this ActorSystem context) where T : ActorBase =>
            context.GetExtension<DIExt2>().PropsFactory<T>();

        public static IPropsFactory<T> PropsFactory<T>(this IActorContext context) where T : ActorBase =>
            context.System.GetExtension<DIExt2>().PropsFactory<T>();
    }

    public class DIExt2 : IExtension
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public void Initialize(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IPropsFactory<T> PropsFactory<T>() where T : ActorBase =>
            ServiceProvider.GetRequiredService<IPropsFactory<T>>();
    }

    public class DIExtension : ExtensionIdProvider<DIExt2>
    {
        public static DIExtension DIExtensionProvider { get; set; } = new DIExtension();

        public override DIExt2 CreateExtension(ExtendedActorSystem system)
        {
            var extension = new DIExt2();
            return extension;
        }
    }
}
