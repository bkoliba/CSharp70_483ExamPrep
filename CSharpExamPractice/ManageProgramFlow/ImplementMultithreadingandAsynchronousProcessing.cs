using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using System.Linq;
using System.Collections.Concurrent;

namespace CSharpExamPractice.ManageProgramFlow
{
    public class ImplementMultithreadingandAsynchronousProcessing
    {
        //Disclaimer these aren't unit test, they are methods demostrating program flow!

        Fixture _fixture;
        ITestOutputHelper _output;
        public ImplementMultithreadingandAsynchronousProcessing(ITestOutputHelper output)
        {
            _output = output;
            _fixture = new Fixture();
        }

        //using the Task Parallel library(TPL) : https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/data-parallelism-task-parallel-library
        [Fact]
        public void UsingTPLForEachMethod()
        {
            var items = GetItemsCollection();
            var startTime = DateTime.Now;

            //Sequential version
            foreach (var item in items)
            {
                ProcessItem(item);
            }

            DisplayTimeSpanOutput(startTime, DateTime.Now, "Sequential");
            startTime = DateTime.Now;

            //Parallel equivalent
            Parallel.ForEach(items, item => ProcessItem(item));
            /*Task Scheduler partitions the task based on system resources and workload*/

            DisplayTimeSpanOutput(startTime, DateTime.Now);
        }

        //using (TPL) Parallel.For() : https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.parallel.for?view=netframework-4.7.1
        [Fact]
        public void UsingTPLForMethod()
        {
            var items = GetItemsCollection(10).ToArray();
            var sequentialItems = new List<string>();
            var parallelItems = new List<string>();
            var testValue = _fixture.Create<string>();

            //sequential version
            for (int i = 0; i < items.Length; i++)
            {
                if (i == 1)
                    items[i] = testValue;
                sequentialItems.Add(items[i]);
            }

            //parallel version
            var result = Parallel.For(0, items.Length, i =>
            {
                if (i == 1)
                    items[i] = testValue;
                parallelItems.Add(items[i]);
            });

            Assert.Equal(sequentialItems.Contains(testValue), parallelItems.Contains(testValue));

            //parallel stop flow with ParrallelLoopState
            long breakIteration = 0;
            var stopResult = Parallel.For(0, items.Length, (i, state) =>
            {
                if (i == 1)
                {
                    state.Break();
                    if (state.LowestBreakIteration.HasValue)
                    {
                        _output.WriteLine($"Broke iteration at {state.LowestBreakIteration.Value}");
                        breakIteration = state.LowestBreakIteration.Value;
                    }
                }
            });

            Assert.False(stopResult.IsCompleted);
            Assert.Equal(1, breakIteration);
        }


        //using (TPL) Parallel LINQ (PLINQ) : https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/custom-partitioners-for-plinq-and-tpl
        [Fact]
        public void UsingParallelLINQ()
        {

        }


        //PLINQ and the Task Parallel Library(TPL) provide default partitioners that work transparently when you write a parallel query or ForEach loop
        [Fact]
        public void UsingParallelLINQPartitioning()
        {
            var items = GetItemsCollection(10, true).ToArray();
            var loadBalancedPartitioner = Partitioner.Create(items, true); //dynamically load balance items
            var query = loadBalancedPartitioner.AsParallel().Select(i => i);

            query.ForAll(i => _output.WriteLine(i));

            _output.WriteLine("End of load balance partitioning.");

            var staticRangePartitoner = Partitioner.Create(0, items.Length);
            double[] results = new double[items.Length];
            Parallel.ForEach(staticRangePartitoner, (range, loopState) =>
            {
                // Loop over each range element without a delegate invocation.
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    _output.WriteLine(items[i]);
                }
            });

            _output.WriteLine("End of range partitioning.");
        }


        //using Tasks



        //create continuation tasks

        //spawn threads by using TreadPool

        //unblock the UI

        //use async and await

        //manage data using concurrent collections

        private void ProcessItem(string item) { Thread.Sleep(1); }
        private List<string> GetItemsCollection(int size = 100, bool displayItems = false)
        {
            var items = new List<string>();

            for (int i = 0; i < size; i++)
            {
                var item = _fixture.Create<string>();
                items.Add(item);
                if (displayItems)
                    _output.WriteLine(item);
            }

            if (displayItems)
                _output.WriteLine("End of item list."); 
            return items;
        }
        private void DisplayTimeSpanOutput(DateTime startTime, DateTime endTime, string processingType = "Parallel")
        {
            var timespan = endTime - startTime;
            _output.WriteLine($"{processingType} Time to process items: {timespan.Milliseconds}");
        }
    }
}
