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
        public DbSet<Position> Positions { get; set; }
        public DbSet<Dimension> Dimensions { get; set; }
        public DbSet<CuttingPlan> CuttingPlans { get; set; }
        public DbSet<CutEntry> CutEntries { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Block>()
                         .HasKey(i => i.Id);
            modelBuilder.Entity<Order>()
                         .HasKey(i => i.OrderId);
            modelBuilder.Entity<OrderLine>()
                         .HasKey(i => i.Id);
            modelBuilder.Entity<Position>()
                         .HasKey(i => i.Id);
            modelBuilder.Entity<Dimension>()
                            .HasKey(i => i.Id);
            modelBuilder.Entity<CuttingPlan>()
                            .HasKey(i => i.Id);
            modelBuilder.Entity<CutEntry>()
                            .HasKey(i => i.Id);

            modelBuilder.Entity<OrderLine>()
                .HasOne(o => o.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(o => o.OrderId);
            modelBuilder.Entity<Position>()
                .HasOne(o => o.OrderLine)
                .WithMany(o => o.Positions)
                .HasForeignKey(o => o.OrderLineId);
            modelBuilder.Entity<Dimension>()
                .HasOne(o => o.OrderLine)
                .WithOne(o => o.Dimension)
                .HasForeignKey<Dimension>(o => o.OrderLineId);
            modelBuilder.Entity<CutEntry>()
                .HasOne(o => o.OrderLine)
                .WithMany(o => o.CutEntries)
                .HasForeignKey(o => o.OrderLineId);
            modelBuilder.Entity<CutEntry>()
                .HasOne(o => o.CuttingPlan)
                .WithMany(o => o.CutEntries)
                .HasForeignKey(o => o.CuttingPlanId);
        }

    }
}
