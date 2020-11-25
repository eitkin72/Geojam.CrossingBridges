using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geojam.Api.Db
{
    public class GeojamDbContext : DbContext
    {
        public DbSet<Bridge> Bridges { get; set; }
        public DbSet<Hiker> Hikers { get; set; }

        public GeojamDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Hiker>()
                .HasOne(h => h.Bridge)
                .WithMany(b => b.Hikers)
                .HasForeignKey(h => h.BridgeId);
        }
    }
}
