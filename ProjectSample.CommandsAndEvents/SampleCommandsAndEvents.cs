using RedisUsage.CqrsCore;
using System;

namespace ProjectSample.CommandsAndEvents
{
    public class CreateSample : ICommand
    {
        public Guid? PublishedCommandId { get; set; }
        public string TokenSession { get; set; }

        public Guid SampleId { get; set; }

        public string SampleVersion { get; set; }
        public string SampleJsonData { get; set; }

        public CreateSample() { }

        public CreateSample( Guid sampleId, string sampleVersion, string sampleJsonData)
        {
            SampleId = sampleId;
            SampleVersion = sampleVersion;
            SampleJsonData = sampleJsonData;
        }
    }

    public class ChangeVersionOfSample : ICommand
    {
        public Guid? PublishedCommandId { get; set; }
        public string TokenSession { get; set; }

        public Guid SampleId { get; set; }

        public string SampleVersion { get; set; }

        public ChangeVersionOfSample() { }

        public ChangeVersionOfSample(Guid sampleId, string sampleVersion)
        {
            SampleId = sampleId;
            SampleVersion = sampleVersion;
        }
    }

    public class SampleCreated : IEvent
    {
        public Guid? PublishedEventId { get; set; }
        public long Version { get; set; }

        public Guid SampleId { get; set; }

        public string SampleVersion { get; set; }
        public string SampleJsonData { get; set; }

        public SampleCreated() { }

        public SampleCreated(Guid sampleId, string sampleVersion, string sampleJsonData)
        {
            SampleId = sampleId;
            SampleVersion = sampleVersion;
            SampleJsonData = sampleJsonData;
        }
    }

    public class SampleVersionChanged : IEvent
    {
        public Guid? PublishedEventId { get; set; }
        public long Version { get; set; }

        public Guid SampleId { get; set; }
        public string SampleVersion { get; set; }

        public SampleVersionChanged() { }

        public SampleVersionChanged(Guid sampleId, string sampleVersion)
        {
            SampleId = sampleId;
            SampleVersion = sampleVersion;
        }
    }
}
