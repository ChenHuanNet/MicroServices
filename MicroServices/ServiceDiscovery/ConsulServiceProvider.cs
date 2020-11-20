using Consul;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery
{
    public class ConsulServiceProvider : IServiceProvider
    {

        private readonly ConsulClient _consulClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">consul服务注册中心地址</param>
        public ConsulServiceProvider(Uri uri)
        {
            _consulClient = new ConsulClient(consulConfig =>
            {
                consulConfig.Address = uri;
            });
        }

        public async Task<IList<string>> GetServicesAsync(string serviceName)
        {
            //tag 可以用标签给服务分类  passonly =true 只获取通过健康检查的
            //通过8500 拿到
            var queryResult = await _consulClient.Health.Service(serviceName, "", true);
            var result = new List<string>();
            foreach (var service in queryResult.Response)
            {
                result.Add(service.Service.Address + ":" + service.Service.Port);
            }
            return result;
        }
    }
}
