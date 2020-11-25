using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace ConsulServiceRegistration
{
    public static class ConsulRegitstrationExtension
    {
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="service"></param>
        public static void AddConsul(this IServiceCollection service)
        {
            var config = new ConfigurationBuilder().AddJsonFile("service.config.json").Build();
            service.Configure<ConsulServiceOptions>(config);
        }


        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            //获取主机生命周期管理窗口
            var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            //获取服务配置项
            var serviceOptions = app.ApplicationServices.GetRequiredService<IOptions<ConsulServiceOptions>>().Value;

            //服务ID 必须保证唯一
            serviceOptions.ServiceId = DateTime.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(999).ToString().PadLeft(3, '0');


            var consulClient = new ConsulClient(configuration =>
            {
                //服务注册地址，集群中的任意一个地址
                configuration.Address = new Uri(serviceOptions.ConsulAddress);
            });

            //获取当前服务地址和端口 
            //string address = serviceOptions.LocalAddress; //读配置文件

            var features = app.Properties["server.Features"] as FeatureCollection;
            var address = features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();
            if (string.IsNullOrEmpty(address))
            {
                address = serviceOptions.LocalAddress; //新版本不会增加默认的端口
            }

            var uri = new Uri(address);

            var registration = new AgentServiceRegistration()
            {
                ID = serviceOptions.ServiceId,
                Name = serviceOptions.ServiceName,
                Address = uri.Host,
                Port = uri.Port,
                Check = new AgentServiceCheck()
                {
                    //注册超时
                    Timeout = TimeSpan.FromSeconds(5),
                    //服务停止多久后注销
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    //健康检查地址
                    HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}{serviceOptions.HealthCheck}",
                    //健康检查时间间隔
                    Interval = TimeSpan.FromSeconds(10),
                }
            };

            //注册服务
            consulClient.Agent.ServiceRegister(registration).Wait();

            //应用程序停止的时候注销服务
            lifetime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(serviceOptions.ServiceId).Wait();
            });

            return app;
        }
    }
}
