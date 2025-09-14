using Microsoft.EntityFrameworkCore;
using SmartCut.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCut.Shared.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // Define your DbSets (tables) here. Example:
        public DbSet<Block> Blocks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Block>()
                         .HasKey(i => i.Id);
            modelBuilder.Entity<Order>()
                         .HasKey(i => i.OrderId);
            modelBuilder.Entity<OrderLine>()
                         .HasKey(i => i.Id);
            modelBuilder.Entity<OrderLine>()
                .HasOne(o => o.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Cascade);



        }

    }
}
