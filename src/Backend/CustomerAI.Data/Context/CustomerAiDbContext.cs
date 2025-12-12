using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomerAI.Core.Entities;

namespace CustomerAI.Data.Context
{
    public class CustomerAiDbContext : DbContext
    {
        public CustomerAiDbContext(DbContextOptions<CustomerAiDbContext> options) : base(options)
        {
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<AiPredictionLog> AiPredictionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)"); 

            modelBuilder.Entity<AiPredictionLog>()
                .Property(l => l.ChurnScore)
                .HasColumnType("float"); 

            base.OnModelCreating(modelBuilder);
        }
    }
}
