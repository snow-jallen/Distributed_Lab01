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

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<Job>> GetWork()
        {
            return new[]
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
                               new MessageDto("G","3")})
            };
        }


        // POST api/values
        [HttpPost]
        public ActionResult<Job> DoJob([FromBody]Job job)
        {
            Counter.Increment();
            Console.WriteLine($"Current Users: {Counter.CurrentRequests}");
            foreach (var msg in job.Messages)
            {
                System.IO.File.WriteAllText(msg.Key, msg.Value);
                Console.WriteLine($"{msg.Key} / {msg.Value}");
            }
            job.Result = $"Saved on server at {DateTime.Now} (Current Users: {Counter.CurrentRequests})";
            Counter.Decrement();
            return job;
        }
    }  
}
