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

        #region private Apply method

        string _version;

        void Apply(SampleCreated e)
        {
            Id = e.SampleId;
            _version = e.SampleVersion;
        }


        void Apply(SampleVersionChanged e)
        {
            _version = e.SampleVersion;

        }

        #endregion

        public ProjectSampleDdd(Guid sampleId, string sampleVersion, string sampleJsonData)
        {

            ApplyChange(new SampleCreated(sampleId, sampleVersion, sampleJsonData));
        }

        public void Create(string title) { }
                
        public void ChangeVersion(string sampleVersion)
        {
            var sampleId = Id;

            ApplyChange(new SampleVersionChanged(sampleId, sampleVersion));
        }
    }
}
