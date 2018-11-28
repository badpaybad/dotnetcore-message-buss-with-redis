using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RedisUsage.RedisServices
{
    public enum ProcessType
    {
        /// <summary>
        /// Allow many worker process same data at a time
        /// </summary>
        PubSub,
        /// <summary>
        /// Allow only one worker process data at a time
        /// </summary>
        Queue,
        /// <summary>
        /// Allow only one worker process data at a time
        /// </summary>
        Stack
    }

    public static class MessageBussServices
    {
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
                        var allChannel = RedisServices.HashGetAll(_keyListChannelName);

                        foreach (var channel in allChannel)
                        {
                            RegisterNotifyWorkerAndStart(channel.Key);
                        }
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

        public static void Publish<T>(T data, ProcessType processType = ProcessType.PubSub)
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
            RedisServices.HashSet(channelSubscriber, new KeyValuePair<string, string>(subscriberName, subscriberName));

            string channelPubSubChild = GetKeyToRealPublishFromChannelToSubscriber(subscriberName, type, ProcessType.PubSub.ToString());
            //regist to process
            RedisServices.Subscribe(channelPubSubChild, (msg) =>
            {
                var obj = JsonConvert.DeserializeObject<T>(msg);
                handle(obj);
            });
            string channelPubSubParent = GetKeyToRealPublishFromChannelToSubscriber(string.Empty, type, ProcessType.PubSub.ToString());
            //regist to process
            RedisServices.Subscribe(channelPubSubParent, (msg) =>
            {
                string data;
                string queueDataName = GetKeyQueueDataForChannel(type);
                while (RedisServices.TryDequeue(queueDataName, out data))
                {
                    var subscribers = RedisServices.HashGetAll(channelSubscriber);

                    foreach (var subc in subscribers)
                    {
                        string queueDataNameChannelSubscriber = GetKeyToRealPublishFromChannelToSubscriber(subc.Key, type, ProcessType.PubSub.ToString());
                        //channelPubSubChild
                        RedisServices.Publish(queueDataNameChannelSubscriber, data);
                    }
                }
            });

            string channelQueue = GetKeyToRealPublishFromChannelToSubscriber(subscriberName, type, ProcessType.Queue.ToString());
            //regist to process
            RedisServices.Subscribe(channelQueue, (msg) =>
            {
                string data;
                string queueDataName = GetKeyQueueDataForChannel(type);
                while (RedisServices.TryDequeue(queueDataName, out data))
                {
                    var obj = JsonConvert.DeserializeObject<T>(data);
                    handle(obj);
                    Thread.Sleep(1);
                }
            });

            string channelStack = GetKeyToRealPublishFromChannelToSubscriber(subscriberName, type, ProcessType.Stack.ToString());
            //regist to process
            RedisServices.Subscribe(channelStack, (msg) =>
            {
                string data;
                string queueDataName = GetKeyQueueDataForChannel(type);
                while (RedisServices.TryPop(queueDataName, out data))
                {
                    var obj = JsonConvert.DeserializeObject<T>(data);
                    handle(obj);
                    Thread.Sleep(1);
                }
            });
        }

        private static string GetProcessTypeOfChannel(string type)
        {
            var processTypeOfChannel = RedisServices.HashGet(_keyListChannelName, type);

            if (string.IsNullOrEmpty(processTypeOfChannel)) processTypeOfChannel = ProcessType.PubSub.ToString();
            return processTypeOfChannel;
        }

        public static void Unsubscribe(string channel, string subscriberName)
        {
            string processTypeOfChannel = GetProcessTypeOfChannel(channel);
            string queueDataNameChannelSubscriber = GetKeyToRealPublishFromChannelToSubscriber(subscriberName, channel, processTypeOfChannel);

            RedisServices.UnSubscribe(queueDataNameChannelSubscriber);

            string channelSubscriber = GetKeySubscribersForChannel(channel);
            RedisServices.HashDelete(channelSubscriber, subscriberName);
        }

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
                             if (processType == ProcessType.PubSub)
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
                return ProcessType.PubSub;
            }
        }

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
    }
}
