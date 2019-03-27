using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Distributed_Lab01
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var serviceCollection = new ServiceCollection();
            Configure(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();
            var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();

            Console.WriteLine("Sleeping...");
            Thread.Sleep(TimeSpan.FromSeconds(5));

            var response = await client.GetStringAsync("http://server/api/values");
            Console.WriteLine(response);
        }

        public static void Configure(IServiceCollection services)
        {
            services.AddHttpClient();
        }
    }
}
