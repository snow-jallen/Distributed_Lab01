using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shared;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;

namespace Distributed_Lab01
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Hello World!  This is {Environment.MachineName} reporting for duty!");
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            var services = serviceCollection.BuildServiceProvider();
            var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();

            Console.WriteLine("Waiting for Server to come online...");
            Thread.Sleep(TimeSpan.FromSeconds(5));

            Console.WriteLine("Getting work");
            var response = await client.GetStringAsync("http://server/api/values/GetWork");            
            var jobs = JsonConvert.DeserializeObject<IEnumerable<Job>>(response);
            var jobQueue = new Queue<Job>(jobs);

            var finalResult = $"{Environment.MachineName} Final Result: ";

            var completedJobs = new List<Job>();
            while(jobQueue.Count > 0)
            {
                var job = jobQueue.Dequeue();
                job.Requestor = Environment.MachineName;
                Console.WriteLine("Submitting job to server...");
                var httpResponse = await client.PostAsJsonAsync("http://server/api/values/DoJob", job);                
                var stringResponse = await httpResponse.Content.ReadAsStringAsync();
                var responseJob = JsonConvert.DeserializeObject<Job>(stringResponse);
                if (responseJob.Success)
                {
                    completedJobs.Add(responseJob);
                }
                else
                {
                    Console.WriteLine("****** Attempt failed.  Putting job back on the queue to try again later. *******");
                    finalResult += $"; job retried";
                    jobQueue.Enqueue(job);//put it back on the list to try again.
                }
            }

            if (completedJobs.TrueForAll(j => j.Success))
                finalResult += " Success! :)";
            else
                finalResult += " :( FAILED!";

            while (true)
            {
                Console.WriteLine(finalResult);                
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }
    }
}
