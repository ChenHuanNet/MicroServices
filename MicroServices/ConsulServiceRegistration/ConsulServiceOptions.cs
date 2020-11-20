using System;
using System.Collections.Generic;
using System.Text;

namespace ConsulServiceRegistration
{
    public class ConsulServiceOptions
    {
        /// <summary>
        /// 服务ID
        /// </summary>
        public string ServiceId { get; set; }

        /// <summary>
        /// 注册中心地址
        /// </summary>
        public string ConsulAddress { get; set; }

        public string ServiceName { get; set; }

        /// <summary>
        /// 健康检查地址
        /// </summary>
        public string HealthCheck { get; set; }

        /// <summary>
        /// 本机的IP和端口
        /// </summary>
        public string LocalAddress { get; set; }
    }
}
