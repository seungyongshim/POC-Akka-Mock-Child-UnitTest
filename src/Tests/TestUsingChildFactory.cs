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
    public class TestUsingChildFactory : TestKit
    {
        [Fact]
        public void Production()
        {
            var services = new ServiceCollection();
            services.AddTransient<IChildFactory<ChildActor>, ChildFactory>();
            services.AddTransient<ParentActor>();
            Sys.UseServiceProvider(services.BuildServiceProvider());

            ActorOf(Sys.DI().Props<ParentActor>());

            ExpectNoMsg();
        }

        [Fact]
        public void Has_Child_Factory()
        {
            var services = new ServiceCollection();
            // MockChildActor에서 사용하는 TestActor 의존성 컨테이너를 작성합니다.
            services.AddSingleton<IActorRef>(sp => TestActor); 
            services.AddTransient<IChildFactory<ChildActor>, MockChildFactory>();
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
            public MockChildActor(IActorRef testActor)
            {
                Become(() =>
                {
                    ReceiveAny(o => testActor.Tell(o));
                });
            }
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
