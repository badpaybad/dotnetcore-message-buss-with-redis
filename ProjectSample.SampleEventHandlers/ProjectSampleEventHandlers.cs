using ProjectSample.CommandsAndEvents;
using ProjectSample.Database;
using ProjectSample.Database.Entities;
using RedisUsage.CqrsCore;
using System;
using System.Linq;

namespace ProjectSample.SampleEventHandlers
{
    public class ProjectSampleEventHandlers : IEventHandle<SampleCreated>,
        IEventHandle<SampleVersionChanged>
    {
        public void Handle(SampleCreated e)
        {
            using (var db = new ProjectSampleDatabase())
            {
                db.Samples.Add(new Sample()
                {
                    Id = e.SampleId,
                    JsonData = e.SampleJsonData,
                    Version = e.SampleVersion
                });
                db.SaveChanges();
            }


            Console.WriteLine($"Save to DbRead for CreateSample.Id{e.SampleId}");
        }

        public void Handle(SampleVersionChanged e)
        {
            using (var db = new ProjectSampleDatabase())
            {
                var existed = db.Samples.FirstOrDefault(i => i.Id == e.SampleId);
                if (existed == null) return;

                existed.Version = e.SampleVersion;

                db.SaveChanges();
            }


            Console.WriteLine($"Save to DbRead for SampleVersionChanged.Id{e.SampleId}");
        }
    }
}
