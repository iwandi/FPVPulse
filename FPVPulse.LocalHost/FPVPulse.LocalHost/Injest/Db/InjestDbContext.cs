using FPVPulse.Ingest;
using Microsoft.EntityFrameworkCore;

namespace FPVPulse.LocalHost.Injest.Db
{
    public class InjestDbContext : DbContext
    {
        public DbSet<DbInjestEvent> Events { get; set; }
        public DbSet<DbInjestRace> Races { get; set; }
        public DbSet<DbInjestPilotResult> PilotResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=injest.db");
        }
    }
}
