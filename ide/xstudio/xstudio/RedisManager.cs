using ServiceStack.Net30.Collections.Concurrent;
using ServiceStack.Redis;

namespace xstudio
{
    public class RedisManager
    {
        private static readonly ConcurrentDictionary<string, PooledRedisClientManager> prcm =
            new ConcurrentDictionary<string, PooledRedisClientManager>();

        public static IRedisClient GetRedisClient(string host = "android@127.0.0.1:6379")
        {
            return prcm.GetOrAdd(host, s =>
            {
                string[] hosts = {s};
                return new PooledRedisClientManager(hosts, hosts, new RedisClientManagerConfig
                {
                    AutoStart = true,
                    DefaultDb = 0,
                    MaxReadPoolSize = 64,
                    MaxWritePoolSize = 128
                });
            }).GetClient();
        }
    }
}