using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CSharpExamPractice.ManageProgramFlow
{
    [Trait("3) Manage Program Flow", "Implement Program Flow")]
    public class ImplementPorgramFlow
    {
        private ITestOutputHelper _output;

        public ImplementPorgramFlow(ITestOutputHelper output)
        {
            _output = output;
        }

        //Iterate across collection and array items
        [Fact]
        public void UsingForLoop()
        {
            int[] values = { 1, 2, 3, 4, 5, 6 };
            for (int index = 0; index < values.Length; index++)
            {
                _output.WriteLine(values[index].ToString());
            }
            //Note: 'break' will also end loop, 'continue' could be use to move to next item
        }

        [Fact]
        public void UsingForEachLoop()
        {
            int[] values = { 1, 2, 3, 4, 5, 6 };
            foreach (int i in values)
            {
                _output.WriteLine(i.ToString());
            }
            //Note: You can't change values in the foreach statement, doing so would result in a compile error
        }

        [Fact]
        public void UsingWhileLoop()
        {
            int[] values = { 1, 2, 3, 4, 5, 6 };
            {
                int index = 0;
                while (index < values.Length)
                {
                    _output.WriteLine(values[index].ToString());
                    index++;
                }
            }
        }

        [Fact]
        public void UsingDoWhileLoop()
        {
            do
            {
                _output.WriteLine("Executed once!");
            }
            while (false);
        }

        //using switch statements
        [Fact]
        public void UsingSwitchStatement()
        {
            var input = 'a';
            switch (input)
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                    {
                        _output.WriteLine("Input is a vowel");
                        break;
                    }
                case 'y':
                    {
                        _output.WriteLine(" is sometimes a vowel.");
                        goto default;//jumps to default
                    }
                default:
                    {
                        _output.WriteLine("Input is a consonant");
                        break;
                    }
            }
        }

        //using if/then, and operators
        [Fact]
        public void UsingIfElse()
        {
            bool b = false;
            bool c = true;
            if (b)
            {
                _output.WriteLine("b is true");
            }
            else if (c)
            {
                _output.WriteLine("c is true");
            }
            else
            {
                _output.WriteLine("b and c are false");
            }
        }

        [Fact]
        public void NullCoalescingOperator()
        {
            int? x = null;
            int y = x ?? -1;
            y.Should().Be(-1);
        }

        [Fact]
        public void UsingConditonalOperator()
        {
            GetValue(true).Should().Be(1);
            GetValue(false).Should().Be(0);
        }
        private static int GetValue(bool p) => p ? 1 : 0;

        //evaluate expressions
        [Fact]
        public void WorkingWithBooleanExpressions()
        {
            int x = 42;
            int y = 1;
            int z = 42;

            Assert.True(x == z);
            Assert.False(y == z);
        }
    }
}
