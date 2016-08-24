using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkyDriftCoreWeb.Models;

namespace SkyDriftCoreWeb.Data
{
    public class SkyDriftDbContext : DbContext
    {
        public DbSet<Serial> Serials { get; set; }
        public DbSet<SkyRoom> Rooms { get; set; }

        public DbSet<Match> Matches { get; set; }
        public DbSet<Record> Records { get; set; }


        public SkyDriftDbContext() :base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
#if SQLCE
            base.OnConfiguring(builder.UseSqlCe(Startup.ConnectionString));
#else
            base.OnConfiguring(builder.UseSqlite(Startup.ConnectionString));
#endif
        }
    }
}
