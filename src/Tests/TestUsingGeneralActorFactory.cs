namespace Tests
{
    using System.Threading.Tasks;
    using Akka.Actor;
    using Akka.DI.Core;
    using Akka.Event;
    using Akka.TestKit.Xunit2;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using SeungyongShim.Akka.Extensions.DependencyInjection.TestKit;
    using Xunit;

    public class TestUsingGeneralActorFactory : TestKit
    {
        [Fact]
        public async Task Has_Child_PropsFactory()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ActorSystem>(sp => Sys);
            services.AddSingleton<IActorRef>(sp => TestActor);
            services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor, MockChildActor>>();
            services.AddSingleton<IPropsFactory<ParentActor>, PropsFactory<ParentActor>>();
            services.AddTransient<ParentActor>();
            services.AddTransient<MockChildActor>();
            Sys.UseDependencyInjectionServiceProvider(services.BuildServiceProvider());

            ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

            var testProbe = CreateTestProbe();

            ExpectMsg<string>().Should().Be("Hello, Kid");
            ExpectMsg<string>().Should().Be("Hello, Kid");

            var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
            child2.Path.Name.Should().Be("Child2");
        }

        [Fact]
        public async Task Production()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ActorSystem>(sp => Sys);
            services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor>>();
            services.AddSingleton<IPropsFactory<ParentActor>, PropsFactory<ParentActor>>();
            services.AddTransient<ParentActor>();
            services.AddTransient<ChildActor>();
            Sys.UseDependencyInjectionServiceProvider(services.BuildServiceProvider());

            ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

            ExpectNoMsg();

            var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
            child2.Path.Name.Should().Be("Child2");

        }

        private class ChildActor : ReceiveActor
        {
            public ChildActor()
            {
                Receive<string>(_ => { });
            }
        }

        private class ParentActor : ReceiveActor
        {
            public ParentActor()
            {
                var childActor1 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create(), "Child1");
                var childActor2 = Context.ActorOf(Context.DI().PropsFactory<ChildActor>().Create(), "Child2");

                childActor1.Tell("Hello, Kid");
                childActor2.Tell("Hello, Kid");
            }
        }
    }
}
