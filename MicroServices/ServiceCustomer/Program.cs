using ServiceCustomWithPolly;
using ServiceDiscovery;
using ServiceDiscovery.LoadBalancer;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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
                builder.LoadBalancer = TypeLoadBalancer.RoundRobin;
                builder.UriScheme = Uri.UriSchemeHttp;
            });


            var policy = PolicyBuilder.CreatePolly();

            var httpClient = new HttpClient();
            for (int i = 0; i < 100; i++)
            {
                policy.Execute(() =>
                {
                    try
                    {
                        var uri = myServiceA.BuildAsync("/health").Result;
                        Console.WriteLine($"{DateTime.Now} - 正在调用:{uri}");
                        var content = httpClient.GetAsync(uri).Result;
                        Console.WriteLine($"调用结果:{content?.StatusCode}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("业务逻辑异常 ex:" + ex.GetType());
                        throw ;
                    }
                });

                Task.Delay(1000).Wait();
            }

            Console.Read();
        }
    }
}
