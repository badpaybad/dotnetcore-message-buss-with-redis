using ProjectSample.CommandsAndEvents;
using RedisUsage.CqrsCore.Ddd;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSample.SampleCommandHandlers
{
    public class ProjectSampleDdd : AggregateRoot
    {
        public override Guid Id { get; set; }

        public ProjectSampleDdd() { }
               
        void Apply(SampleCreated e)
        {
            Id = e.SampleId;

        }


        public ProjectSampleDdd(Guid sampleId, string sampleVersion, string sampleJsonData)
        {

            ApplyChange(new SampleCreated(Guid.NewGuid(), sampleId, sampleVersion, sampleJsonData));
        }

        public void Create(string title) { }
                
        public void ChangeVersion(string sampleVersion)
        {
            var sampleId = Id;

            ApplyChange(new SampleVersionChanged(Guid.NewGuid(), sampleId, sampleVersion));
        }
    }
}
