using ProjectSample.CommandsAndEvents;
using RedisUsage.CqrsCore;
using RedisUsage.CqrsCore.CqrsRepository;
using System;

namespace ProjectSample.SampleCommandHandlers
{
    public class ProjectSampleCommandHandlers : ICommandHandle<CreateSample>
        , ICommandHandle<ChangeVersionOfSample>
    {
        public ICqrsEventSourcingRepository Repository => new CqrsEventSourcingRepository(new EventPublisher());

        ICqrsEventSourcingRepository ICommandHandle<ChangeVersionOfSample>.Repository => throw new NotImplementedException();

        public void Handle(CreateSample c)
        {
            //valid command here
            if (string.IsNullOrEmpty(c.SampleVersion) || c.SampleId == Guid.Empty)
            {
                throw new ArgumentNullException($"SampleVersion or SampleId was empty");
            }
            //can ensure permission by token c.TokenSession            

            //do business
            Repository.CreateNew<ProjectSampleDdd>(new ProjectSampleDdd(c.SampleId, c.SampleVersion, c.SampleJsonData));
        }

        void ICommandHandle<ChangeVersionOfSample>.Handle(ChangeVersionOfSample c)
        {
            //valid command here
            if (string.IsNullOrEmpty(c.SampleVersion) || c.SampleId == Guid.Empty)
            {
                throw new ArgumentNullException($"SampleVersion or SampleId was empty");
            }
            //can ensure permission by token c.TokenSession        

            //do business
            Repository.GetDoSave<ProjectSampleDdd>(c.SampleId, a => a.ChangeVersion(c.SampleVersion));
        }
    }
}
