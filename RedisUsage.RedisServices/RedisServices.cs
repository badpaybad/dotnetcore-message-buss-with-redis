using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace RedisUsage.RedisServices
{
    public static class RedisServices
    {
        static IServer _server;
        static SocketManager _socketManager;
        static IConnectionMultiplexer _connectionMultiplexer;
        static ConfigurationOptions _options = null;

        public static bool IsEnable { get; private set; }

        static RedisServices()
        {

        }

        static IConnectionMultiplexer RedisConnectionMultiplexer
        {
            get
            {
                if (_connectionMultiplexer != null && _connectionMultiplexer.IsConnected)
                    return _connectionMultiplexer;

                if (_connectionMultiplexer != null && !_connectionMultiplexer.IsConnected)
                {
                    _connectionMultiplexer.Dispose();
                }

                _connectionMultiplexer = GetConnection();
                if (!_connectionMultiplexer.IsConnected)
                {
                    var exception = new Exception("Can not connect to redis");
                    Console.WriteLine(exception);
                    throw exception;
                }
                return _connectionMultiplexer;
            }
        }

        static IDatabase RedisDatabase
        {
            get
            {
                var redisDatabase = RedisConnectionMultiplexer.GetDatabase();

                return redisDatabase;
            }
        }

        static ISubscriber RedisSubscriber
        {
            get
            {
                var redisSubscriber = RedisConnectionMultiplexer.GetSubscriber();

                return redisSubscriber;
            }
        }

        public static void Init(string endPoint, int? port, string pwd)
        {
            IsEnable = !string.IsNullOrEmpty(endPoint);

            var soketName = endPoint ?? "127.0.0.1";
            _socketManager = new SocketManager(soketName);

            port = port ?? 6379;

            _options = new ConfigurationOptions()
            {
                EndPoints =
                {
                    {endPoint, port.Value}
                },
                Password = pwd,
                AllowAdmin = false,
                SyncTimeout = 5 * 1000,
                SocketManager = _socketManager,
                AbortOnConnectFail = false,
                ConnectTimeout = 5 * 1000,
            };
        }

        public static TimeSpan Ping()
        {
            return RedisDatabase.Ping();
        }

        static ConnectionMultiplexer GetConnection()
        {
            if (_options == null) throw new Exception($"Must call {nameof(RedisServices.Init)}");
            return ConnectionMultiplexer.Connect(_options);
        }

        public static void Subscribe(string channel, Action<string> handleMessage)
        {
            RedisSubscriber.Subscribe(channel).OnMessage((msg) =>
            {               
                //Console.WriteLine(msg.Channel);
                //Console.WriteLine(msg.SubscriptionChannel);

                handleMessage(msg.Message);
            });
        }

        public static void UnSubscribe(string channel)
        {
            RedisSubscriber.Unsubscribe(channel);
        }

        public static void Publish(string channel, string message)
        {
            RedisSubscriber.Publish(channel, message);
        }

        public static T Get<T>(string key)
        {
            if (!IsEnable)
            {
                return default(T);
            }

            var val = RedisDatabase.StringGet(key);
            if (val.HasValue == false) return default(T);

            return JsonConvert.DeserializeObject<T>(val);
        }

        public static void Set<T>(string key, T val, TimeSpan? expireAfter = null)
        {
            if (!IsEnable)
            {
                return;
            }

            RedisDatabase.StringSet(key, JsonConvert.SerializeObject(val), expireAfter);
        }

        public static string Get(string key)
        {
            if (!IsEnable)
            {
                return null;
            }

            var val = RedisDatabase.StringGet(key);
            return val;
        }

        public static void Set(string key, string val, TimeSpan? expireAfter = null)
        {
            if (!IsEnable)
            {
                return;
            }

            RedisDatabase.StringSet(key, val, expireAfter);
        }

        public static void HashSet(string key, KeyValuePair<string, string> val)
        {
            if (!IsEnable)
            {
                throw new PlatformNotSupportedException("No Redis enable");
            }

            RedisDatabase.HashSet(key, val.Key, val.Value);
        }

        public static string HashGet(string key, string fieldName)
        {
            if (!IsEnable)
            {
                throw new PlatformNotSupportedException("No Redis enable");
            }
            return RedisDatabase.HashGet(key, fieldName);
        }

        public static void HashDelete(string key, string fieldName)
        {
            if (!IsEnable)
            {
                throw new PlatformNotSupportedException("No Redis enable");
            }
            RedisDatabase.HashDelete(key, fieldName);
        }

        public static Dictionary<string, string> HashGetAll(string key)
        {
            if (!IsEnable)
            {
                throw new PlatformNotSupportedException("No Redis enable");
            }
            var data = RedisDatabase.HashGetAll(key);

            Dictionary<string, string> temp = new Dictionary<string, string>();
            foreach (var d in data)
            {
                temp.Add(d.Name, d.Value);
            }
            return temp;
        }

        public static bool QueueHasValue(string key)
        {
            if (!IsEnable)
            {
                throw new PlatformNotSupportedException("No Redis enable");
            }

            if (RedisDatabase.KeyExists(key) == false) return false;

            return RedisDatabase.ListLength(key) > 0;

            //return RedisDatabase.KeyExists(key);
        }

        public static long QueueLength(string key)
        {
            if (RedisDatabase.KeyExists(key) == false) return 0;

            return RedisDatabase.ListLength(key) ;
        }

        public static bool TryEnqueue(string key, params string[] values)
        {
            if (!IsEnable)
            {
                throw new PlatformNotSupportedException("No Redis enable");
            }
            RedisDatabase.ListLeftPush(key, values.ToRedisValueArray());
            return true;
        }

        public static bool TryDequeue(string key, out string val)
        {
            if (!IsEnable)
            {
                throw new PlatformNotSupportedException("No Redis enable");
            }
            val = string.Empty;

            if (!RedisDatabase.KeyExists(key)) return false;

            //var lockKey = key + "_locker";
            //if (RedisDatabase.KeyExists(lockKey))
            //{
            //    return false;               
            //}

            //RedisDatabase.StringSet(lockKey, "true");
            var temp = RedisDatabase.ListRightPop(key);
            //RedisDatabase.KeyDelete(lockKey);

            if (temp.HasValue == false) return false;
            val = temp;


            //val = RedisDatabase.ListRightPop(key);

            return true;
        }


        public static bool TryPop(string key, out string val)
        {
            if (!IsEnable)
            {
                throw new PlatformNotSupportedException("No Redis enable");
            }
            val = string.Empty;

            if (!RedisDatabase.KeyExists(key)) return false;

            //var lockKey = key + "_locker";
            //if (RedisDatabase.KeyExists(lockKey))
            //{
            //    return false;
            //}

            //RedisDatabase.StringSet(lockKey, "true");
            var temp = RedisDatabase.ListLeftPop(key);
            //RedisDatabase.KeyDelete(lockKey);

            if (temp.HasValue == false) return false;

            val = temp;


            //val = RedisDatabase.ListLeftPop(key);

            return true;
        }
    }
}
