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
            Console.WriteLine("Hello World!");
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            var services = serviceCollection.BuildServiceProvider();
            var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();

            Console.WriteLine("Sleeping...");
            Thread.Sleep(TimeSpan.FromSeconds(5));

            Console.WriteLine("Getting work");
            var response = await client.GetStringAsync("http://server/api/values/GetWork");            
            var jobs = JsonConvert.DeserializeObject<IEnumerable<Job>>(response);
            Console.WriteLine("Got work.");

            foreach(var job in jobs)
            {
                Console.WriteLine("Submitting job to server...");
                var httpResponse = await client.PostAsJsonAsync("http://server/api/values/DoJob", job);                
                var stringResponse = await httpResponse.Content.ReadAsStringAsync();
                var responseJob = JsonConvert.DeserializeObject<Job>(stringResponse);
                Console.WriteLine("Result from server: " + responseJob.Result);
            }            
        }
    }
}
