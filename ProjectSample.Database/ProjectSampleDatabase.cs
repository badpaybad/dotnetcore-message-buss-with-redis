using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectSample.Database.Entities;
using System;

namespace ProjectSample.Database
{
    public class ProjectSampleDatabase : AbstractMsSqlDbContext
    {
        public ProjectSampleDatabase()
            : base(ConfigurationManagerExtensions.GetConnectionString("ProjectSampleDatabase"))
        {
        }

        public DbSet<Sample> Samples { get; set; }
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

    public static class ConfigurationManagerExtensions
    {
        public static IConfiguration Configuration { get; private set; }

        public static void SetConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static string GetConnectionString(string name)
        {
            //if (Configuration == null)
            //{
            //    var consoleFileApp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");

            //    var builder = new ConfigurationBuilder()
            //    .AddJsonFile("appSettings.json");

            //    if (File.Exists(consoleFileApp))
            //    {
            //        builder.AddJsonFile(consoleFileApp);
            //    }

            //    Configuration = builder.Build();
            //}

            return Configuration[$"ConnectionStrings:{name}"];
        }
    }
}
