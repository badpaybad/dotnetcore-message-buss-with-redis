# dotnetcore-message-buss-with-redis
Use redis as message buss, main mechanism Pub Sub. We create channel (topic) by full type nam of object (Commands, Event). Everytime push (publish) data we save to queue by full type name of object. Then auto notify to subscriber to dequeue then process. So that no lose message if no subscriber. 

    https://redis.io
    https://github.com/StackExchange/StackExchange.Redis
    https://github.com/MicrosoftArchive/redis

## The main idea
We Publish(an instance of Class) and We Subscribe(type of Class) to process. The type full name of Class will auto become an topic. Support distributed for subscrbiers with data structure

.queue, stack (can deploy many consumer and each consumer process difference item at a time )

.topic (can deploy many consumer and consumer process the same item at a time)

# RedisUsage.Test
The console sample to push data to queue
When push can decide the type process eg: stack, queue, topic

# RedisUsage.ConsoleHandle
The console sample of Handle data and process

# Code usage

        //push data to topic with full type name of "SampleTest"
        MessageBussServices.Publish<SampleTest>(new SampleTest
                            {
                                Message = msg,
                                CreatedDate = DateTime.Now
        });
  
        //received data from topic with full type name of "SampleTest" to process
        RedisServices.MessageBussServices.Subscribe<SampleTest>("console handle1", (obj) =>
                    {
                        Console.WriteLine("console handle1");
                        Console.WriteLine(JsonConvert.SerializeObject(obj));
        });

# Code structure

## CommandsEvents
### Comands should be publish as Queue, Stack. Commands should sequence to process. We can deploy many consumer to see the same queue or stack to dequeue or pop data to process. Process one by one, we have more consumer so item in queue or stack can be faster dequeue or pop to process. After commands processed, we should fire Events. Many consumer can process the same time and diference data (the same type of data).
### Event should be publish an Topic. Many consumer can subscribe Topic to process the same time and the same data (the same type of data).

    Command -> CommandHandler (consumer) -> processed -> fire Event -> notify to Subscribers -> EventHandlers (consumers) -> process Event data
    
### DDD and Eventsourcing 
We can add DDD and Eventsourcing in CommandHandler. We can call to Aggregate Repository to build Domain business and call function.
    https://github.com/badpaybad/cqrs-dot-net-core

