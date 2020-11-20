using ServiceDiscovery;
using ServiceDiscovery.LoadBalancer;
using System;
using System.Net.Http;

namespace ServiceCustomer
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new ConsulServiceProvider(new Uri("http://127.0.0.1:8500"));
            var myServiceA = serviceProvider.CreateServiceBuilder((builder) =>
            {
                builder.ServiceName = "MyServiceA";
                builder.LoadBalancer = TypeLoadBalancer.RandomLoad;
                builder.UriScheme = Uri.UriSchemeHttp;
            });

            var httpClient = new HttpClient();
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var uri = myServiceA.BuildAsync("/health").Result;
                    Console.WriteLine($"{DateTime.Now} - 正在调用:{uri}");
                    var content = httpClient.GetAsync(uri).Result;
                    Console.WriteLine($"调用结果:{content}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("调用异常:" + ex.GetType());
                }
            }

            Console.Read();
        }
    }
}
