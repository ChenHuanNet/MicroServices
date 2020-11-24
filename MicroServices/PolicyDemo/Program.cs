using System;
using System.Net.Http;
using Polly;
using Polly.CircuitBreaker;

namespace PolicyDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //跨服务调用一般会用到策略Polly  比如服务A调用服务B   自身业务代码一般用try catch

            // try catch 和 polly 是不一样的  业务代码该try catch的还是要加的    自身业务有问题再怎么策略业务错的

            // 三步
            //1、定义故障   把一种异常定义成一个故障   当发生这个异常的时候，就会触发策略
            //2、指定策略  FallBack就是降级策略
            //3、执行策略

            //回退 就是降级
            Policy.Handle<ArgumentException>().Fallback(() =>
            {
                Console.WriteLine("Polly Fallback!");
            }).Execute(() =>
            {
                //业务方法  ，一般这里是跨服务的调用
                Console.WriteLine("DoSomething");
                throw new ArgumentException("Hello Polly");  //出现异常 调到 Fallback  
            });


            //单个异常类型的故障
            Policy.Handle<Exception>();


            //带条件的异常类型
            Policy.Handle<Exception>(ex => ex.Message == "Hello");


            //多个异常类型
            Policy.Handle<HttpRequestException>().Or<ArgumentException>().Or<ArgumentNullException>();

            //多个异常带条件
            Policy.Handle<HttpRequestException>(ex => ex.Message == "Hello").Or<ArgumentException>().Or<ArgumentNullException>();


            //弹性策略  响应性策略 ,重试 ，断路器

            //重试  没传就是默认1次
            Policy.Handle<Exception>().Retry();
            Policy.Handle<Exception>().Retry(3, (ex, i) => { });
            Policy.Handle<Exception>().Retry(3, (ex, i, context) => { });

            //一直重试直到成功  一般不用  非高并发场景
            Policy.Handle<Exception>().RetryForever();

            //重试且等待  第一次重试等1秒，第二次等2秒 第二次等3秒
            Policy.Handle<Exception>().WaitAndRetry(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) });

            //重试且等待 2的i次方
            Policy.Handle<Exception>().WaitAndRetry(3, i => TimeSpan.FromSeconds(Math.Pow(2, i)));

            //一直重试直到成功  一般不用  非高并发场景  有等待时间
            Policy.Handle<Exception>().WaitAndRetryForever(i => TimeSpan.FromSeconds(Math.Pow(2, i)));

            //每个策略都有委托 


            //断路器  实用

            //普通断路器

            //[连续]触发指定次数（2）的故障后，开启断路器（OPEN）,进入熔断状态 1分钟
            var breaker = Policy.Handle<Exception>().CircuitBreaker(2, TimeSpan.FromMinutes(1),
                   onBreak: (ex, span) =>
                   {
                       //onBreak  进入熔断 OPEN
                   },
                   onReset: () =>
                   {
                       //onReset  一分钟后就恢复了  CLOSED
                   },
                   onHalfOpen: () =>
                   {

                   });

            CircuitState cs = breaker.CircuitState;
            //断路器有 3个状态  OPEN , CLOSED, HALF-OPEN

            //Polly 断路器策略里特殊的状态   手动开启
            CircuitState tmp = CircuitState.Isolated;
            //手动开启断路器  （默认是CLOSED）
            breaker.Isolate();
            //手动关闭断路器
            breaker.Reset();


            //高级断路器
            //如果在故障采样持续时间内，导致处理异常的操作比例超过故障阔值，则发生熔断，

            // 10秒内 执行8次 ，有4次发生故障 则进入熔断状态 30秒  ，熔断结束后进入 HALF-OPEN 状态  断路器尝试释放1次 操作，尝试去请求，如果成功变成 CLOSED ，如果失败  断路器OPEN  30秒
            Policy.Handle<Exception>().AdvancedCircuitBreaker(
                0.5,  //故障阔值 50%
                TimeSpan.FromSeconds(10), //故障采样时间
                8,  // 最小吞吐量   10秒内执行8次  
                TimeSpan.FromSeconds(30)); // 熔断时间 30秒


            //超时  服务调用 他也没有挂 ，就是慢！！ 慢本身也是一种故障 
            Policy.Timeout(3, (context, span, task, ex) =>
            {
                //异常
            }).Execute(() =>
                 {
                     //业务代码
                 });


            //舱壁隔离

            //最大并发12，超过全部拒绝不执行  ,排队100  超过112 拒绝
            Policy.Bulkhead(12, 100).Execute(() =>
             {
                 //业务代码
             });

            //缓存比较复杂，要依赖其他库 可以集合redis
            // Policy.Cache()


            //策略包装 ，策略组合
            var fallback = Policy.Handle<Exception>().Fallback(() =>
             {
                 Console.WriteLine("Polly Fallback!");
             });

            var retry = Policy.Handle<Exception>().Retry(3, (ex, i) => { Console.WriteLine($"retry:{i}"); });

            //策略组合 如果重试3次还失败，就降级   策略是从右往左
            var policy = Policy.Wrap(fallback, retry);
            policy.Execute(() =>
            {
                Console.WriteLine("Polly Beigin!");
                throw new Exception("Error");
            });

            //最后不会用单独策略，只会用策略组合

            Console.Read();
        }
    }
}
