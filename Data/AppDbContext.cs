using Microsoft.EntityFrameworkCore;
using pokearcanumbe.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace pokearcanumbe.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> opts) : IdentityDbContext<User>(opts)
    {
        public DbSet<Card> Cards { get; set; }
        public DbSet<Marketplace> Marketplaces { get; set; }
        public DbSet<Trade> Trades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Trade>()
                .HasOne(t => t.Seller)
                .WithMany()
                .HasForeignKey(t => t.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trade>()
                .HasOne(t => t.Buyer)
                .WithMany()
                .HasForeignKey(t => t.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trade>()
                .HasOne(t => t.Marketplace)
                .WithMany()
                .HasForeignKey(t => t.MarketplaceId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
    
}