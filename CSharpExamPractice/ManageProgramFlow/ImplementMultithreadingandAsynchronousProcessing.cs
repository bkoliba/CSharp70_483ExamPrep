using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AutoFixture;
using System.Linq;
using System.Collections.Concurrent;
using System.Net.Http;

namespace CSharpExamPractice.ManageProgramFlow
{
    [Trait("Manage Program Flow", "Multitheading/asynchronous")]
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


        // using (TPL) Parallel LINQ (PLINQ) : https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/introduction-to-plinq
        [Fact]
        public void UsingParallelLINQ()
        {
            var items = Enumerable.Range(0, 100);
            Func<int, bool> evenNumber = i => (i % 2) == 0;

            // opt-in to parallel execution if there is preformance gain
            var evenNumbers = items.AsParallel().Where(evenNumber).ToArray();
            Assert.Equal(50, evenNumbers.Length);

            // by default it uses all processor power but can use less with WithDegreeOfParallelism
            evenNumbers = items.AsParallel().Where(evenNumber).ToArray();
            Assert.Equal(50, evenNumbers.Length);

            //An AsOrdered sequence is still processed in parallel, but its results are buffered and sorted.
            evenNumbers = items.AsParallel().AsOrdered().Where(evenNumber).ToArray();
            Assert.Equal(50, evenNumbers.Length);

            //For faster query execution when order preservation is not required and when the processing of the results can itself be parallelized, use the ForAll method
            var query = items.AsParallel().Where(evenNumber);
            query.ForAll(i => _output.WriteLine(i.ToString()));
        }


        // PLINQ and the Task Parallel Library(TPL) provide default partitioners that work transparently when you write a parallel query or ForEach loop
        // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/custom-partitioners-for-plinq-and-tpl
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


        //'Tasks' (Represents an asynchronous operation)
        //Create and executing Task
        [Fact]
        public void CreateTasks()
        {
            Task t = Task.Run(() =>
            {
                for (int ctr = 0; ctr < 100; ctr++)
                {
                    _output.WriteLine(ctr.ToString());
                }
            });

            t.Wait();

            t = Task.Factory.StartNew(() =>
            {
                for (int ctr = 0; ctr < 100; ctr++)
                {
                    _output.WriteLine(ctr.ToString());
                }
            });

            t.Wait();
        }

        [Fact]
        public void WaitingandCheckingStatusOfATask()
        {
            Task t = Task.Run(() => Thread.Sleep(2000));
            _output.WriteLine(t.Status.ToString());
            t.Wait();
            _output.WriteLine(t.Status.ToString());

            t = Task.Run(() => Thread.Sleep(2000));
            t.Wait(1000);
            _output.WriteLine($"Task A completed: {t.IsCompleted}, Status: {t.Status}");

            if (!t.IsCompleted)
                _output.WriteLine("Timed out before task completed.");
        }


        //create continuation tasks
        [Fact]
        public void ContinuationWithATask()
        {
            Task<DayOfWeek> t = Task.Run(() => DateTime.Today.DayOfWeek);

            //Executes after the 't' task completes with output value being pushed into the Action
            t.ContinueWith(a => _output.WriteLine(a.Result.ToString()));
        }
        [Fact]
        public void ContinuationWithATaskMultipleLinesToExecute()
        {
            Task<DayOfWeek> t = Task.Run(() => DateTime.Today.DayOfWeek);
            t.ContinueWith(a =>
            {
                _output.WriteLine(a.Result.ToString());
                _output.WriteLine($"Today is {a.Result.ToString()}");
            });
        }

        //spawn threads by using ThreadPool
        [Fact] //https://msdn.microsoft.com/en-us/library/system.threading.threadpool(v=vs.110).aspx
        public void UsingThreadPoolToSpawnAThread()
        {
            // Queue the task.
            ThreadPool.QueueUserWorkItem(ThreadProc);
            _output.WriteLine("Main thread does some work, then sleeps.");
            Thread.Sleep(1000);

            _output.WriteLine("Main thread exits.");
        }

        // This thread procedure performs the task.
        static void ThreadProc(Object stateInfo)
        {
            // No state object was passed to QueueUserWorkItem, so stateInfo is null.
            Console.WriteLine("Hello from the thread pool.");
        }

        //unblock the UI
        [Fact]
        public async Task UnblockingUIThread()
        {
            // The await causes the handler to return immediately.
            await Task.Run(() => ComputeNextMove());
            // Now update the UI with the results.
            // ...
        }

        private async Task ComputeNextMove()
        {
            // Perform background work here.
            // Don't directly access UI elements from this method.
        }

        //use async and await
        [Fact]
        public async Task UseAsyncAndAwaitToGetWebpage()
        {
            _output.WriteLine(await GetGoogleHomepageAsync());
        }
        private async Task<string> GetGoogleHomepageAsync()
        {
            HttpClient client = new HttpClient();
            var task = client.GetStringAsync("https://google.com");

            return await task;
        }

        //manage data using concurrent collections
        [Fact]
        public void UsingConcurrentCollections()
        {
            int NUMITEMS = 64;
            int initialCapacity = 101;
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;

            ConcurrentDictionary<int, int> cd = new ConcurrentDictionary<int, int>(concurrencyLevel, initialCapacity);

            for (int i = 0; i < NUMITEMS; i++) cd[i] = i * i;

            _output.WriteLine("The square of 23 is {0} (should be {1})", cd[23], 23 * 23);
        }

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
