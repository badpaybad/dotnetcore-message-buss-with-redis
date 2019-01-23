using ProjectSample.CommandsAndEvents;
using ProjectSample.Database;
using ProjectSample.Database.Entities;
using RedisUsage.CqrsCore;
using System;

namespace ProjectSample.SampleEventHandlers
{
    public class ProjectSampleEventHandlers : IEventHandle<SampleCreated>
    {
        public void Handle(SampleCreated c)
        {
            using (var db = new ProjectSampleDatabase())
            {
                db.Samples.Add(new Sample()
                {
                    Id = c.SampleId,
                    JsonData = c.SampleJsonData,
                    Version = c.SampleVersion
                });
                db.SaveChanges();
            }
        }

     
    }
}
