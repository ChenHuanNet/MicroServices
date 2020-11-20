using ServiceDiscovery;
using System;

namespace MicroServices
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("服务发现者");

            var serviceProvider = new ConsulServiceProvider(new Uri("http://127.0.0.1:8500"));
            var services = serviceProvider.GetServicesAsync("MyServiceA").Result;
            foreach (var service in services)
            {
                Console.WriteLine(service);
            }
            Console.WriteLine("服务发现结束");

            Console.Read();
        }
    }
}
