using ServiceDiscovery.LoadBalancer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery.Builder
{
    public interface IServiceBuilder
    {
        /// <summary>
        /// 服务提供者
        /// </summary>
        IServiceProvider ServiceProvider { get; set; }

        string ServiceName { get; set; }

        /// <summary>
        /// Uri方案  http还是https
        /// </summary>
        string UriScheme { get; set; }

        /// <summary>
        /// 用哪种策略负载循环
        /// </summary>
        ILoadBalancer LoadBalancer { get; set; }

        Task<Uri> BuildAsync(string path);
    }
}
