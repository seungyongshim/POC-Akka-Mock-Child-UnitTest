using Akka.Actor;
using Akka.DI.Core;
using Akka.DI.Extensions.DependencyInjection;
using Akka.TestKit.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests
{
    public class TestNotUsingChildFactory : TestKit
    {
        [Fact]
        public void Production()
        {
            var services = new ServiceCollection();
            services.AddTransient<ChildActor>();
            Sys.UseServiceProvider(services.BuildServiceProvider());

            ActorOf<ParentActor>();

            ExpectNoMsg();
        }

        [Fact]
        public void Has_Not_Child_Factory()
        {
            var services = new ServiceCollection();
            // MockChildActor에서 사용하는 TestActor 의존성을 선언한다.
            services.AddSingleton<IActorRef>(sp => TestActor);
            services.AddTransient<ChildActor, MockChildActor>();
            Sys.UseServiceProvider(services.BuildServiceProvider());

            ActorOf<ParentActor>();

            ExpectMsg<string>().Should().Be("Hello, Kid");
        }


        private class ChildActor : ReceiveActor
        {
            public ChildActor()
            {
                // MockChildActor 생성자에서 호출 된다.
                // 여기 작성된 내용은 MockChildActor에도 반영된다.
                Receive<string>(_ => { });
            }
        }

        private class MockChildActor : ChildActor
        {
            public MockChildActor(IActorRef testActor)
            {
                // ChildActor 생성자에서 선언한 Receive를 오버라이딩 한다.
                Become(() =>
                {
                    ReceiveAny(o => testActor.Forward(o));
                });
            }
        }

        private class ParentActor : ReceiveActor
        {
            public ParentActor()
            {
                var childActor = Context.ActorOf(Context.DI().Props<ChildActor>());

                childActor.Tell("Hello, Kid");
            }
        }
    }
}
