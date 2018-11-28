# dotnetcore-message-buss-with-redis
Use redis as message buss, main mechanism Pub Sub. We create channel (topic) by full type nam of object (Commands, Event). Everytime push (publish) data we save to queue by full type name of object. Then auto notify to subscriber to dequeue then process. So that no lose message if no subscriber. 

## The main idea
We Publish(an instance of Class) and We Subscribe(type of Class) to process. The type full name of Class will auto become an topic

# RedisUsage.Test
The console sample to push data to queue
When push can decide the type process eg: stack, queue, topic

# RedisUsage.ConsoleHandle
The console sample of Handle data and process
