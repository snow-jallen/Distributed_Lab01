using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Shared;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;

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

            var response = await client.GetStringAsync("http://server/api/values");
            Console.WriteLine(response);

            var msg = new MessageDto(DateTime.Now.Ticks.ToString(), Environment.MachineName);
            var content2 = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json");
            var response3 = await client.PostAsync("http://server/api/values", content2);
            var stringResponse = await response3.Content.ReadAsStringAsync();
            var responseMessage = JsonConvert.DeserializeObject<MessageDto>(stringResponse);
            
            Console.WriteLine("Result from server: " + responseMessage.Result);
        }
    }
}
