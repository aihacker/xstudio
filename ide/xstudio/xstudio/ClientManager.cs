using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace xstudio
{
    public class ClientManager
    {
        public static int HookFunc(string source, string host = "android@127.0.0.1:6379")
        {
            using (var client = RedisManager.GetRedisClient(host))
            {
                return client.PublishMessage("hooks", RSA.Encrypt(source));
            }
        }

        public static string CallFunc(string apk, string source, object args = null, string host = "android@127.0.0.1:6379")
        {
            var uuid = Guid.NewGuid().ToString("D");
            var data = JsonConvert.SerializeObject(new { uuid, source, args });
            var text = RSA.Encrypt(data);
            using (var client = RedisManager.GetRedisClient(host))
            {
                client.Lists[string.Format("{0}:request", apk)].Push(text);
                string result = client.Lists[string.Format("{0}:response:{1}", apk, uuid)].BlockingPop(new TimeSpan(0, 0, 2));
                if (! string.IsNullOrEmpty(result))
                {
                   result = RSA.Decrypt(result);
                }
                return result;
            }
        }
    }
}