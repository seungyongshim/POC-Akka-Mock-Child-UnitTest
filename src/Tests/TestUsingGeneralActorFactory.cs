using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.DI.Core;
using Akka.DI.Extensions.DependencyInjection;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests
{
    public class TestUsingGeneralActorFactory : TestKit
    {
        [Fact]
        public void Production_Using_Genral_Props_Factory()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IPropsFactory<ChildActor>, GeneralPropsFactory<ChildActor>>();
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
            services.AddSingleton<IPropsFactory<ChildActor>, MockChildPropsFactory>();
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

        

        public class GeneralPropsFactory<T> : IPropsFactory<T> where T : ActorBase
        {
            public GeneralPropsFactory(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }

            public IServiceProvider ServiceProvider { get; }

            public Props GetProps() =>
                Props.Create(() => ServiceProvider.GetRequiredService<T>());
        }

        private class MockChildPropsFactory : IPropsFactory<ChildActor>
        {
            public MockChildPropsFactory(IActorRef testActor)
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
                    serviceProvider.GetRequiredService<IPropsFactory<ChildActor>>().GetProps());

                childActor.Tell("Hello, Kid");
            }
        }
    }
    public interface IPropsFactory<T> where T : ActorBase
    {
        Props GetProps();
    }

   
}
