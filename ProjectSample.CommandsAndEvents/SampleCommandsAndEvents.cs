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

        public CreateSample(Guid? publishedCommandId, string tokenSession, Guid sampleId, string sampleVersion, string sampleJsonData)
        {
            PublishedCommandId = publishedCommandId;
            TokenSession = tokenSession;
            SampleId = sampleId;
            SampleVersion = sampleVersion;
            SampleJsonData = sampleJsonData;
        }
    }

    public class ChangeVersionOfSample : ICommand {
        public Guid? PublishedCommandId { get; set; }
        public string TokenSession { get; set; }

        public Guid SampleId { get; set; }

        public string SampleVersion { get; set; }

        public ChangeVersionOfSample(Guid sampleId, string sampleVersion)
        {
            SampleId = sampleId;
            SampleVersion = sampleVersion;
        }
    }

    public class SampleCreated : IEvent
    {
        public Guid? PublishedEventId { get; }
        public long Version { get; }

        public Guid SampleId { get; set; }

        public string SampleVersion { get; set; }
        public string SampleJsonData { get; set; }

        public SampleCreated(Guid? publishedEventId, Guid sampleId, string sampleVersion, string sampleJsonData)
        {
            PublishedEventId = publishedEventId;
            SampleId = sampleId;
            SampleVersion = sampleVersion;
            SampleJsonData = sampleJsonData;
        }
    }

    public class SampleVersionChanged : IEvent {
        public Guid? PublishedEventId { get; }
        public long Version { get; }

        public Guid SampleId { get; set; }
        public string SampleVersion { get; set; }

        public SampleVersionChanged(Guid? publishedEventId, Guid sampleId, string sampleVersion)
        {
            PublishedEventId = publishedEventId;
            SampleId = sampleId;
            SampleVersion = sampleVersion;
        }
    }
}
