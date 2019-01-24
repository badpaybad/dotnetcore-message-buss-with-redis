using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisUsage.CqrsCore.Ef
{

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
