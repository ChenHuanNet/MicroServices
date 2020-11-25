using System;
using Polly;

namespace ServiceCustomWithPolly
{
    public class PolicyBuilder
    {
        /// <summary>
        /// 超时 重试 连续故障熔断 回退
        /// </summary>
        /// <returns></returns>
        public static ISyncPolicy CreatePolly()
        {
            var timeoutPolicy = Policy.Timeout(10, (context, timespan, task) =>
            {
                Console.WriteLine("执行超时了");
            });

            var retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetry(
                2,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(2),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"{DateTime.Now} - 重试 {retryCount} 次 - 抛出{exception.GetType()}");
                });

            var circuitBreakerPolicy = Policy.Handle<Exception>()
                .CircuitBreaker(
                //熔断前运行出现2次故障
                2,
                //熔断时间
                TimeSpan.FromMinutes(5),
                   onBreak: (ex, span) =>
                   {
                       //onBreak  进入熔断 OPEN
                       Console.WriteLine($"{DateTime.Now} - 断路器:开启状态（熔断时触发）");
                   },
                   onReset: () =>
                   {
                       //onReset  一分钟后就恢复了  CLOSED
                       Console.WriteLine($"{DateTime.Now} - 断路器:关闭状态（熔断恢复时触发）");
                   },
                   onHalfOpen: () =>
                   {
                       Console.WriteLine($"{DateTime.Now} - 断路器:半开启状态（熔断时间到了之后触发）");
                   });


            var fallBackPolicy = Policy.Handle<Exception>().Fallback(
                fallbackAction: () =>
                {
                    Console.WriteLine("这是一个FallBack");
                },
                onFallback: (ex) =>
                {
                    Console.WriteLine($"FallBack异常:{ex.GetType()}");
                });

            var policy = Policy.Wrap(fallBackPolicy, circuitBreakerPolicy, retryPolicy, timeoutPolicy);

            return policy;
        }
    }
}
