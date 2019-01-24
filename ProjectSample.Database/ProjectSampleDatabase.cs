using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectSample.Database.Entities;
using RedisUsage.CqrsCore.Ef;
using System;
using System.IO;

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

    
}
