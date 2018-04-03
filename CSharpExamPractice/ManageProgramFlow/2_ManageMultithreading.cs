using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CSharpExamPractice.ManageProgramFlow
{
    [Trait("2) Manage Program Flow", "ManageMultithreading")]
    public class ManageMultithreading
    {
        private ITestOutputHelper _output;

        //Synchronize resources https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/threading/thread-synchronization
        //For simple operations on integral numeric data types, synchronizing threads can be accomplished with members of the Interlocked class. 
        //For all other data types and non thread-safe resources, multithreading can only be safely performed using lock keyword, monitors, Synchronization Events and Wait Handles.

        public ManageMultithreading(ITestOutputHelper output)
        {
            _output = output;
        }

        //Note: In general, avoid locking on a public type, or instances beyond your code's control. 
        //Using Locking
        [Fact]
        public void UsingLocks()
        {
            int n = 0;
            object _lock = new object();
            var up = Task.Run(() =>
            {
                for (int i = 0; i < 1000000; i++)
                    lock (_lock)
                        n++;
            });
            for (int i = 0; i < 1000000; i++)
                lock (_lock)
                    n--;
            up.Wait();
            n.Should().Be(0);
        }

        [Fact]
        public void UsingInterlockedIncrementAndDecrement()
        {
            int n = 0;
            var up = Task.Run(() =>
            {
                for (int i = 0; i < 1000000; i++)
                    Interlocked.Increment(ref n);
            });
            for (int i = 0; i < 1000000; i++)
                Interlocked.Decrement(ref n);
            up.Wait();
            n.Should().Be(0);
        }

        /*
         Using Monitor
         lock (x)  
         {  
             DoSomething();  
         }  

         This is equivalent to:
         System.Object obj = (System.Object)x;  
         System.Threading.Monitor.Enter(obj);  
         try  
         {  
             DoSomething();  
         }  
         finally  
         {  
             System.Threading.Monitor.Exit(obj);  
         }  
         */

        //cancel a long-running task
        [Fact]
        public void CancelingTasks()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            Task task = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.Write("*");
                    Thread.Sleep(1000);
                }
            }, token);
            //Console.ReadLine();
            cancellationTokenSource.Cancel();
        }

        [Fact]
        public void AddContinuationTaskForCancellation()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            Task task = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    _output.WriteLine("*");
                    Thread.Sleep(1000);
                }
                token.ThrowIfCancellationRequested();
            }, token).ContinueWith((t) =>
            {
                t.Exception.Handle((e) => true);
                _output.WriteLine("You have canceled the task");
            }, TaskContinuationOptions.OnlyOnCanceled);

            cancellationTokenSource.Cancel();
        }

        [Fact]
        public void SettingATimeoutOnATask()
        {
            Task longRunning = Task.Run(() =>
            {
                Thread.Sleep(10000);
            });
            int index = Task.WaitAny(new[] { longRunning }, 1000);
            if (index == -1)
                _output.WriteLine("Task timed out");
        }

        //implement thread-safe methods to handle race conditions
    }
}
