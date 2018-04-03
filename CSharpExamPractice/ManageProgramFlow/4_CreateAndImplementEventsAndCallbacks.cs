using FluentAssertions;
using System;
using Xunit;
using Xunit.Abstractions;

namespace CSharpExamPractice.ManageProgramFlow
{
    [Trait("4) Manage Program Flow", "CreateAndImplementEventsAndCallbacks")]
    public class CreateAndImplementEventsAndCallbacks
    {
        private ITestOutputHelper _output;

        public CreateAndImplementEventsAndCallbacks(ITestOutputHelper output)
        {
            _output = output;
        }

        //Create event handlers
        [Fact]
        public void CreateEventHandlers()
        {
            Pub p = new Pub();
            p.OnChange += (sender, e) => e.Value.Should().Be(42);
            p.Raise();
        }
        public class MyArgs : EventArgs
        {
            public MyArgs(int value)
            {
                Value = value;
            }
            public int Value { get; set; }
        }
        public class Pub
        {
            public event EventHandler<MyArgs> OnChange = delegate { };
            //public delegate void EventHandler(object sender, EventArgs e);
            public void Raise()
            {
                OnChange(this, new MyArgs(42));
            }
        }

        //subscribe to and unsubscribe from events
        [Fact]
        public void SubscribeAndUnsubscribeEvents()
        {
            Pub pub = new Pub();
            pub.OnChange += MessageEventHandler;
            pub.OnChange += MessageEventHandler;
            pub.OnChange += MessageEventHandler;
            pub.OnChange -= MessageEventHandler;
            pub.Raise();//calls MessageEventHandler twice
        }
        void MessageEventHandler(object sender, MyArgs e)
        {
            _output.WriteLine(e.Value.ToString());
        }

        //use built-in delegate types to create events
        [Fact]
        public void BuildingEventWithDelegateTypes()
        {
            var test = new TestEvent();
            test.OnChange += () => _output.WriteLine("Hello");
            test.Raise();
        }
        public class TestEvent
        {
            public event Action OnChange = delegate { };
            public void Raise()
            {
                OnChange();
            }
        }

        //create delegates
        [Fact]
        public void UsingDelegates()
        {
            int Add(int x, int y) { return x + y; }
            int Multiply(int x, int y) { return x * y; }
            Calculate calc = Add;
            calc(3, 4).Should().Be(7);
            calc = Multiply;
            calc(3, 4).Should().Be(12);
        }
        delegate int Calculate(int x, int y);

        [Fact]
        public void UsingMulticastDelegates()
        {
            Del d = MethodOne;
            d += MethodTwo;
            d();
        }
        public void MethodOne()
        {
            _output.WriteLine("MethodOne");
        }
        public void MethodTwo()
        {
            _output.WriteLine("MethodTwo");
        }
        public delegate void Del();

        //lambda expressions
        [Fact]
        public void UsingLambdaExpressionsToCreateDelegates()
        {
            Calculate calc = (x, y) => x + y;
            calc(3, 4).Should().Be(7);
            calc = (x, y) => x * y;
            calc(3, 4).Should().Be(12);
        }

        //anonymous methods
        delegate void NumberChanger(int n);

        [Fact]
        public void UsingAnonymousMethods()
        {
            NumberChanger nc = delegate (int x) {
                _output.WriteLine("Anonymous Method: {0}", x);
            };
        }
    }
}
