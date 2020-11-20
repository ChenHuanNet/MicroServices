using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDiscovery.LoadBalancer
{
    public class RoundRobinLoadBalancer : ILoadBalancer
    {
        private readonly object _lock = new object();
        private int _index = 0;

        /// <summary>
        /// 这个有锁，肯定对性能有影响
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public string Resolve(IList<string> services)
        {
            lock (_lock)
            {
                if (_index >= services.Count)
                {
                    _index = 0;
                }
                return services[_index++];
            }
        }
    }
}
