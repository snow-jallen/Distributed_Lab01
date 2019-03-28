using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared;
using System.Threading;

namespace Server.Controllers
{
    public static class Counter
    {
        private static long currentRequests;
        public static long CurrentRequests
        {
            get { return Interlocked.Read(ref currentRequests); }
        }

        internal static void Increment()
        {
            Interlocked.Increment(ref currentRequests);
        }

        internal static void Decrement()
        {
            Interlocked.Decrement(ref currentRequests);
        }

    }

    public static class WorkManager
    {
        static WorkManager()
        {
            AllowedFailures = int.Parse(Environment.GetEnvironmentVariable("ALLOWED_FAILURES"));
            WaitAfterFailure = double.Parse(Environment.GetEnvironmentVariable("WAIT_AFTER_FAILURE"));
            WaitAfterSuccess = double.Parse(Environment.GetEnvironmentVariable("WAIT_AFTER_SUCCESS"));
            WorkToGive = new Queue<IEnumerable<Job>>();
            var jobSource = new[]
            {
                new Job(new []{new MessageDto("A","1"),
                               new MessageDto("B","1"),
                               new MessageDto("C","1"),
                               new MessageDto("D","1"),
                               new MessageDto("E","1")}),
                new Job(new []{new MessageDto("B","2"),
                               new MessageDto("D","2"),
                               new MessageDto("E","2"),
                               new MessageDto("F","2"),
                               new MessageDto("G","2")}),
                new Job(new []{new MessageDto("C","3"),
                               new MessageDto("A","3"),
                               new MessageDto("B","3"),
                               new MessageDto("D","3"),
                               new MessageDto("G","3")}),
                new Job(new []{new MessageDto("A","4"),
                               new MessageDto("B","4"),
                               new MessageDto("C","4"),
                               new MessageDto("D","4"),
                               new MessageDto("E","4")}),
                new Job(new []{new MessageDto("B","5"),
                               new MessageDto("D","5"),
                               new MessageDto("E","5"),
                               new MessageDto("F","5"),
                               new MessageDto("G","5")}),
                new Job(new []{new MessageDto("C","6"),
                               new MessageDto("A","6"),
                               new MessageDto("B","6"),
                               new MessageDto("D","6"),
                               new MessageDto("G","6")}),
                new Job(new []{new MessageDto("A","7"),
                               new MessageDto("B","7"),
                               new MessageDto("C","7"),
                               new MessageDto("D","7"),
                               new MessageDto("E","7")}),
                new Job(new []{new MessageDto("B","8"),
                               new MessageDto("D","8"),
                               new MessageDto("E","8"),
                               new MessageDto("F","8"),
                               new MessageDto("G","8")}),
                new Job(new []{new MessageDto("C","9"),
                               new MessageDto("A","9"),
                               new MessageDto("B","9"),
                               new MessageDto("D","9"),
                               new MessageDto("G","9")}),
                new Job(new []{new MessageDto("A","10"),
                               new MessageDto("B","10"),
                               new MessageDto("C","10"),
                               new MessageDto("D","10"),
                               new MessageDto("E","10")}),
                new Job(new []{new MessageDto("B","11"),
                               new MessageDto("D","11"),
                               new MessageDto("E","11"),
                               new MessageDto("F","11"),
                               new MessageDto("G","11")}),
                new Job(new []{new MessageDto("C","12"),
                               new MessageDto("A","12"),
                               new MessageDto("B","12"),
                               new MessageDto("D","12"),
                               new MessageDto("G","12")})
            };

            for(int i=0;i<24;i++)
            {
                var shuffledJobs = RandomPermutation(jobSource);
                WorkToGive.Enqueue(shuffledJobs);
            }
        }
        public static double WaitAfterSuccess { get; private set; }
        public static double WaitAfterFailure { get; private set; }
        public static int AllowedFailures { get; private set; }
        public static Queue<IEnumerable<Job>> WorkToGive { get; private set; }
        static Random random = new Random();

        /// <summary>
        /// From https://stackoverflow.com/questions/375351/most-efficient-way-to-randomly-sort-shuffle-a-list-of-integers-in-c-sharp/375446#375446
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IEnumerable<T> RandomPermutation<T>(IEnumerable<T> sequence)
        {
            T[] retArray = sequence.ToArray();


            for (int i = 0; i < retArray.Length - 1; i += 1)
            {
                int swapIndex = random.Next(i, retArray.Length);
                if (swapIndex != i)
                {
                    T temp = retArray[i];
                    retArray[i] = retArray[swapIndex];
                    retArray[swapIndex] = temp;
                }
            }

            return retArray;
        }
    }


    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<Job>> GetWork()
        {
            var jobs = WorkManager.WorkToGive.Dequeue();
            return new ActionResult<IEnumerable<Job>>(jobs);
        }

        // POST api/values
        [HttpPost]
        public ActionResult<Job> DoJob([FromBody]Job job)
        {
            Counter.Increment();
            Console.WriteLine($"Got job from {job.Requestor}: {String.Join("; ", job.Messages.Select(m => $"{m.Key}/{m.Value}"))} (Current Users: {Counter.CurrentRequests})");

            Thread.Sleep(TimeSpan.FromSeconds(.5));
            var filesInUse = new Dictionary<string, FileStream>();
            var messageQueue = new Queue<MessageDto>(job.Messages);
            var totalFailures = 0;
            var openedAllFilesOK = true;
            while(messageQueue.Count > 0)
            {
                var msg = messageQueue.Peek();
                try
                {
                    FileStream stream = System.IO.File.OpenWrite(msg.Key);

                    filesInUse.Add(msg.Key, stream);
                    messageQueue.Dequeue();//it worked...actually take it off the queue
                }
                catch(System.IO.IOException)
                {
                    if (totalFailures++ > WorkManager.AllowedFailures)
                    {
                        Console.WriteLine($"** Unable to open {msg.Key} for {job.Requestor}.  Giving up.");
                        openedAllFilesOK = false;
                        break;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(WorkManager.WaitAfterFailure));
                }
            }

            if (openedAllFilesOK)
            {
                Thread.Sleep(TimeSpan.FromSeconds(WorkManager.WaitAfterSuccess));
                foreach (var item in filesInUse)
                {
                    using (var writer = new StreamWriter(item.Value))
                    {
                        var msg = job.Messages.Single(m => m.Key == item.Key);
                        writer.WriteLineAsync(msg.Value);
                    }
                }
                Console.WriteLine($"Success!  Saved all values from {job.Requestor}");
                job.ResultMessage = $"Saved on server at {DateTime.Now} (Current Users: {Counter.CurrentRequests})";
            }
            else
            {
                job.ResultMessage = "Failed to open all files. :(";
            }
            job.Success = openedAllFilesOK;

            Console.WriteLine($"Closing files {String.Join(" ", filesInUse.Keys)} for {job.Requestor}");
            foreach(var item in filesInUse)
            {
                item.Value.Close();
            }
            Counter.Decrement();
            return job;
        }
    }  
}
