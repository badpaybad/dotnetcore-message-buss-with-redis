using System;

namespace RedisUsage.CqrsCore
{
    public interface ICqrsEventSourcingRepository
    //<TAggregate> where TAggregate : IAggregateRoot
    {
        IEventPublisher EventPublisher { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        TAggregate Get<TAggregate>(Guid aggregateId) where TAggregate : IAggregateRoot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregate"></param>
        /// <param name="expectedVersion">-1: automatic get lastest version</param>
        void Save<TAggregate>(TAggregate aggregate, int expectedVersion = -1) where TAggregate : IAggregateRoot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregate"></param>
        void CreateNew<TAggregate>(TAggregate aggregate) where TAggregate : IAggregateRoot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <param name="aggregateDoActionsBeforeSave"></param>
        /// <param name="expectedVersion">-1: automatic get lastest version</param>
        void GetDoSave<TAggregate>(Guid aggregateId, Action<TAggregate> aggregateDoActionsBeforeSave, int expectedVersion = -1) where TAggregate : IAggregateRoot;
    }
}
