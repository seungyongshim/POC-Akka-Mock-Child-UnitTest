namespace Tests
{
    using System;
    using Akka.Actor;
    using Akka.DI.Core;
    using Akka.DI.Extensions.DependencyInjection;
    using Akka.TestKit.Xunit2;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class TestUsingChildFactory : TestKit
    {
        [Fact]
        public void Production()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IChildFactory<ChildActor>, ChildFactory>();
            services.AddTransient<ParentActor>();
            Sys.UseServiceProvider(services.BuildServiceProvider());

            ActorOf(Sys.DI().Props<ParentActor>());

            ExpectNoMsg();
        }

        [Fact]
        public void Has_Child_Factory()
        {
            var services = new ServiceCollection();
            // MockChildActor에서 사용하는 TestActor 의존성을 선언한다.
            services.AddSingleton<IActorRef>(sp => TestActor);
            services.AddSingleton<IChildFactory<ChildActor>, MockChildFactory>();
            services.AddTransient<ParentActor>();
            Sys.UseServiceProvider(services.BuildServiceProvider());

            ActorOf(Sys.DI().Props<ParentActor>());

            ExpectMsg<string>().Should().Be("Hello, Kid");
        }

        private class ChildActor : ReceiveActor
        {
            public ChildActor()
            {
                Receive<string>(_ => { });
            }
        }

        public interface IChildFactory<T>
        {
            Props GetProps();
        }

        private class ChildFactory : IChildFactory<ChildActor>
        {
            public Props GetProps() =>
                 Props.Create(() => new ChildActor());
        }

        private class MockChildFactory : IChildFactory<ChildActor>
        {
            public MockChildFactory(IActorRef testActor)
            {
                TestActor = testActor;
            }

            public IActorRef TestActor { get; }

            public Props GetProps() =>
                 Props.Create(() => new MockChildActor(TestActor));
        }

        private class MockChildActor : ReceiveActor
        {
            public MockChildActor(IActorRef testActor) =>
                ReceiveAny(o => testActor.Forward(o));
        }

        private class ParentActor : ReceiveActor
        {
            public ParentActor(IServiceProvider serviceProvider)
            {
                var childActor = Context.ActorOf(
                    serviceProvider.GetRequiredService<IChildFactory<ChildActor>>().GetProps());

                childActor.Tell("Hello, Kid");
            }
        }
    }
}
