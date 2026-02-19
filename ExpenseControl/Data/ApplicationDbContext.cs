using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExpenseControl.Models;

namespace ExpenseControl.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<TransactionItem> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>()
                .Property(t => t.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Jedzenie" },
                new Category { Id = 2, Name = "Rozrywka" },
                new Category { Id = 3, Name = "Rachunki" },
                new Category { Id = 4, Name = "Inne" }
                );
            builder.Entity<Store>().HasData(
                new Store { Id = 1, Name = "Biedronka", CategoryId = 1 },
                new Store { Id = 2, Name = "Auchan", CategoryId = 1 },
                new Store { Id = 3, Name = "Lidl", CategoryId = 1 }
                );
        }
    }
}
