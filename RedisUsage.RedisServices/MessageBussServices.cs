using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace RedisUsage.RedisServices
{

    public static class MessageBussServices
    {

        public enum ProcessType
        {
            /// <summary>
            /// Allow many worker process same data at a time
            /// </summary>
            Topic,
            /// <summary>
            /// Allow only one worker process data at a time
            /// </summary>
            Queue,
            /// <summary>
            /// Allow only one worker process data at a time
            /// </summary>
            Stack
        }

        const string _keyListChannelName = "MessageBussServices_ListChannelName_";

        static Dictionary<string, int> _mapedHandleCounter = new Dictionary<string, int>();

        static Dictionary<string, List<Thread>> _mapedHandleWorkers = new Dictionary<string, List<Thread>>();

        static MessageBussServices()
        {
            new Thread(() =>
            {
                while (true)// scan to register worker
                {
                    try
                    {
                        ScanToRegisterWorker();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            }).Start();
        }

        public static void ScanToRegisterWorker()
        {
            var allChannel = RedisServices.HashGetAll(_keyListChannelName);

            foreach (var channel in allChannel)
            {
                RegisterNotifyWorkerAndStart(channel.Key);
            }
        }

        #region build key redis

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">channel</param>
        /// <returns></returns>
        private static string GetKeyQueueDataForChannel(string type)
        {
            return _keyListChannelName + type + "_QueueData";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">channel</param>
        /// <returns></returns>
        private static string GetKeySubscribersForChannel(string type)
        {
            return _keyListChannelName + type + "_Subscriber";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">channel</param>
        /// <returns></returns>
        private static string GetKeyToRealPublishFromChannelToSubscriber(string subscriberName, string type, string processTypeOfChannel)
        {
            return _keyListChannelName + type + "_QueueData_" + subscriberName + "_ProcessType_" + processTypeOfChannel;
        }

        private static string GetProcessTypeOfChannel(string type)
        {
            var processTypeOfChannel = RedisServices.HashGet(_keyListChannelName, type);

            if (string.IsNullOrEmpty(processTypeOfChannel)) processTypeOfChannel = ProcessType.Topic.ToString();
            return processTypeOfChannel;
        }

        #endregion

        public static void Publish<T>(T data, ProcessType processType = ProcessType.Topic)
        {
            var type = typeof(T).FullName;

            RedisServices.HashSet(_keyListChannelName, new KeyValuePair<string, string>(type, processType.ToString()));

            string queueDataName = GetKeyQueueDataForChannel(type);
            RedisServices.TryEnqueue(queueDataName, JsonConvert.SerializeObject(data));
        }
        
        public static void Subscribe<T>(string subscriberName, Action<T> handle)
        {
            var type = typeof(T).FullName;
            string channelSubscriber = GetKeySubscribersForChannel(type);

           if(RedisServices.HashExisted(channelSubscriber, subscriberName))
            {
                throw new MessageBussServices.ExistedSubscriber($"Existed subscriber {subscriberName} in channel {channelSubscriber}");
            }

            RedisServices.HashSet(channelSubscriber, new KeyValuePair<string, string>(subscriberName, subscriberName));

            string channelPubSubChild = GetKeyToRealPublishFromChannelToSubscriber(subscriberName, type, ProcessType.Topic.ToString());
            //regist to process data for data structure pubsub
            RedisServices.Subscribe(channelPubSubChild, (data) =>
            {
                TryDoJob<T>(data, handle);
            });
            string channelPubSubParent = GetKeyToRealPublishFromChannelToSubscriber(string.Empty, type, ProcessType.Topic.ToString());
            //regist to process if pubsub try dequeue get data and publish to subscriber
            RedisServices.Subscribe(channelPubSubParent, (msg) =>
            {
                string data;
                string queueDataName = GetKeyQueueDataForChannel(type);
                while (RedisServices.TryDequeue(queueDataName, out data))
                {
                    var subscribers = RedisServices.HashGetAll(channelSubscriber);

                    foreach (var subc in subscribers)
                    {
                        string queueDataNameChannelSubscriber = GetKeyToRealPublishFromChannelToSubscriber(subc.Key, type, ProcessType.Topic.ToString());
                        //channelPubSubChild
                        RedisServices.Publish(queueDataNameChannelSubscriber, data);
                    }
                }
            });

            string channelQueue = GetKeyToRealPublishFromChannelToSubscriber(subscriberName, type, ProcessType.Queue.ToString());
            //regist to process if data structure is queue
            RedisServices.Subscribe(channelQueue, (msg) =>
            {
                string data;
                string queueDataName = GetKeyQueueDataForChannel(type);
                while (RedisServices.TryDequeue(queueDataName, out data))
                {
                    TryDoJob<T>(data, handle);
                }
            });

            string channelStack = GetKeyToRealPublishFromChannelToSubscriber(subscriberName, type, ProcessType.Stack.ToString());
            //regist to process if data structure is stack
            RedisServices.Subscribe(channelStack, (msg) =>
            {
                string data;
                string queueDataName = GetKeyQueueDataForChannel(type);
                while (RedisServices.TryPop(queueDataName, out data))
                {
                    TryDoJob<T>(data, handle);
                }
            });
        }

        static void TryDoJob<T>(string data, Action<T> handle)
        {
            var type = typeof(T).FullName;
            var queueName = GetKeyQueueDataForChannel(type);
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(data);
                handle(obj);
                string successQueueDataName = queueName + "_Success";
                RedisServices.TryEnqueue(successQueueDataName, data);
                Thread.Sleep(1);
            }
            catch (Exception)
            {
                string errorQueueDataName = queueName + "_Error";
                RedisServices.TryEnqueue(errorQueueDataName, data);
            }
        }

        public static void Unsubscribe(string channel, string subscriberName)
        {
            string processTypeOfChannel = GetProcessTypeOfChannel(channel);
            string queueDataNameChannelSubscriber = GetKeyToRealPublishFromChannelToSubscriber(subscriberName, channel, processTypeOfChannel);

            RedisServices.UnSubscribe(queueDataNameChannelSubscriber);

            string channelSubscriber = GetKeySubscribersForChannel(channel);
            RedisServices.HashDelete(channelSubscriber, subscriberName);
        }

        #region worker to notify
        /// <summary>
        /// type = channel
        /// </summary>
        /// <param name="type"></param>
        static void RegisterNotifyWorkerAndStart(string type)
        {
            var processTypeOfChannel = RedisServices.HashGet(_keyListChannelName, type);

            var channelRegistered = type + "_" + processTypeOfChannel;

            lock (_mapedHandleCounter)
            {
                int count;
                if (_mapedHandleCounter.TryGetValue(channelRegistered, out count))
                {
                    if (count > 0)
                    {
                        _mapedHandleCounter[channelRegistered] = 1;
                        // allow only one worker (thread dequeue data)
                        return;
                    }
                    ////if allow more than one thread to dequeue
                    //List<Thread> listWorker;

                    //if (_mapedHandleWorkers.TryGetValue(type, out listWorker))
                    //{
                    //    var nextWorker = CreateDequeuWorker(type);
                    //    listWorker.Add(nextWorker);
                    //    nextWorker.Start();
                    //    _mapedHandleWorkers[type] = listWorker;
                    //    _mapedHandleCounter[type] = count + 1;
                    //}
                }
                else
                {
                    _mapedHandleCounter[channelRegistered] = 1;

                    var firstWorker = CreateNotifyWorker(type);

                    _mapedHandleWorkers[channelRegistered] = new List<Thread>() { firstWorker };
                    //make sure alway have at less one worker to dequeue
                    firstWorker.Start();
                }
            }

        }

        private static Thread CreateNotifyWorker(string type)
        {
            return new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        string queueDataName = GetKeyQueueDataForChannel(type);

                        var processTypeOfChannel = RedisServices.HashGet(_keyListChannelName, type);
                        string channelSubscriber = GetKeySubscribersForChannel(type);

                        var subscribers = RedisServices.HashGetAll(channelSubscriber);

                        if (RedisServices.QueueHasValue(queueDataName))
                        {
                            ProcessType processType = GetProcessTypeByName(processTypeOfChannel);
                            if (processType == ProcessType.Topic)
                            {
                                string queueDataNameChannelSubscriber = GetKeyToRealPublishFromChannelToSubscriber(string.Empty, type, processTypeOfChannel);
                                //notify to parent, then parent will find subscribers to process same data
                                RedisServices.Publish(queueDataNameChannelSubscriber, processTypeOfChannel);
                            }
                            else
                            {
                                foreach (var subc in subscribers)
                                {
                                    string queueDataNameChannelSubscriber = GetKeyToRealPublishFromChannelToSubscriber(subc.Key, type, processTypeOfChannel);

                                    RedisServices.Publish(queueDataNameChannelSubscriber, processTypeOfChannel);
                                    //notify to subscribers to dequeue data and process
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }

        private static ProcessType GetProcessTypeByName(string processTypeName)
        {
            if (ProcessType.Queue.ToString().Equals(processTypeName, StringComparison.InvariantCultureIgnoreCase))
            {
                return ProcessType.Queue;
            }
            else if (ProcessType.Stack.ToString().Equals(processTypeName, StringComparison.InvariantCultureIgnoreCase))
            {
                return ProcessType.Stack;
            }
            else
            {
                return ProcessType.Topic;
            }
        }
        #endregion

        public static void Dispose()
        {
            var allChannel = RedisServices.HashGetAll(_keyListChannelName);
            foreach (var c in allChannel)
            {
                string channelSubscriber = GetKeySubscribersForChannel(c.Key);

                var subscribers = RedisServices.HashGetAll(channelSubscriber);

                foreach (var sub in subscribers)
                {
                    Unsubscribe(c.Key, sub.Key);
                }
            }
        }

        #region api monitor

        public static List<ChannelInfo> GetAllChannelInfo()
        {
            var allChannel = RedisServices.HashGetAll(_keyListChannelName);
            return allChannel.Select(d => new ChannelInfo()
            {
                DataStructure = d.Value,
                Name = d.Key,
                Subscribers = GetSubscribers(d.Key),
                PendingDataQueueLength = GetPendingDataQueueLength(d.Value),
                SuccessDataQueueLength = GetSuccessDataQueueLength(d.Value),
                ErrorDataQueueLength = GetErrorDataQueueLength(d.Value)
            }).ToList();
        }
        public static ChannelInfo GetChannelInfo(string channelName)
        {
            var channelVal = RedisServices.HashGet(_keyListChannelName, channelName);
            return new ChannelInfo()
            {
                DataStructure = channelVal,
                Name = channelName,
                Subscribers = GetSubscribers(channelName),
                PendingDataQueueLength = GetPendingDataQueueLength(channelName),
                SuccessDataQueueLength = GetSuccessDataQueueLength(channelName),
                ErrorDataQueueLength = GetErrorDataQueueLength(channelName)
            };
        }

        public static List<SubscriberInfo> GetSubscribers(string channelName)
        {
            string channelSubscriber = GetKeySubscribersForChannel(channelName);
            var subscribers = RedisServices.HashGetAll(channelSubscriber);
            return subscribers.Select(i => new SubscriberInfo { Name = i.Key }).ToList();
        }

        public static long GetPendingDataQueueLength(string channelName)
        {
            var queueName = GetKeyQueueDataForChannel(channelName);
            return RedisServices.QueueLength(queueName);
        }

        public static long GetErrorDataQueueLength(string channelName)
        {
            var queueName = GetKeyQueueDataForChannel(channelName) + "_Error";
            return RedisServices.QueueLength(queueName);
        }

        public static long GetSuccessDataQueueLength(string channelName)
        {
            var queueName = GetKeyQueueDataForChannel(channelName) + "_Success";
            return RedisServices.QueueLength(queueName);
        }

        public class ChannelInfo
        {
            public string Name { get; set; }
            public string DataStructure { get; set; }

            public List<SubscriberInfo> Subscribers { get; set; }
            public long PendingDataQueueLength { get; set; }
            public long SuccessDataQueueLength { get; set; }
            public long ErrorDataQueueLength { get; set; }
        }

        public class SubscriberInfo
        {
            public string Name { get; set; }
        }

        [Serializable]
        private class ExistedSubscriber : Exception
        {
            public ExistedSubscriber()
            {
            }

            public ExistedSubscriber(string message) : base(message)
            {
            }

            public ExistedSubscriber(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected ExistedSubscriber(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        #endregion
    }

}
