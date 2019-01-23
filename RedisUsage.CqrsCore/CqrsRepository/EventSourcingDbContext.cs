using Microsoft.EntityFrameworkCore;
using RedisUsage.CqrsCore.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

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

    public abstract class AbstractMsSqlDbContext : DbContext
    {
        protected string _connectionString = string.Empty;

        public AbstractMsSqlDbContext()
        {

        }

        public AbstractMsSqlDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public AbstractMsSqlDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        //public virtual void UpdateBatch<T>(params T[] entities ) where T : class
        //{
        //    var db = this;
        //    foreach (var entity in entities)
        //    {
        //        var entry = db.Entry(entity);
        //        var dbSet = db.Set<T>();
        //        if (entry.State == EntityState.Detached)
        //        {
        //            dbSet.Attach(entity);
        //        }

        //        db.Entry(entity).State = EntityState.Modified;

        //    }

        //    var affected = db.SaveChanges();

        //}
    }
}
