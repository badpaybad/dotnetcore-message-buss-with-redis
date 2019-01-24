using Microsoft.EntityFrameworkCore;
using RedisUsage.CqrsCore.Ef;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedisUsage.CqrsCore.CqrsRepository
{
    internal class EventSourcingDbContext : AbstractMsSqlDbContext
    {
        public EventSourcingDbContext()
            : base(ConfigurationManagerExtensions.GetConnectionString("EventSourcingDbContext"))
        {
        }

        public DbSet<EventSourcingDescription> EventSoucings { get; set; }
    }

    [Table("EventSourcingDescription")]
    public class EventSourcingDescription
    {
        [Key]
        public Guid EsdId { get; set; }

        public Guid AggregateId { get; set; }

        public long Version { get; set; }

        //[StringLength(512)]
        public string AggregateType { get; set; } = string.Empty;

        //[StringLength(512)]
        public string EventType { get; set; } = string.Empty;

        public string EventData { get; set; }

        public DateTime CreatedDate { get; set; }
    }

}
