using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Sql
{
    public class SalesDbContext:DbContext
    {
        public SalesDbContext(DbContextOptions<SalesDbContext> options):base(options) 
        {
            
        }

        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Sale>(e =>
            {
                e.HasKey(e => e.Id);
                e.HasIndex(x => x.RowId).IsUnique();
                e.Property(x => x.UnitsSold).HasColumnType("decimal(18,2)");
                e.Property(x => x.Revenue).HasColumnType("decimal(18,2)");
                e.Property(x => x.Cogs).HasColumnType("decimal(18,2)");
                e.Property(x => x.Profit).HasColumnType("decimal(18,2)");

                

            });

            b.Entity<ProcessedEvent>(e =>
            {
                e.HasKey(x => x.RowId);
                e.Property(x => x.ProcessedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            });
        }


    }
}
