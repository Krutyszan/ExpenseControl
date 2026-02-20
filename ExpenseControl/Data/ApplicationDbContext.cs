using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExpenseControl.Models;

namespace ExpenseControl.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Store> Stores { get; set; } = null!;
        public DbSet<TransactionItem> Items { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>()
                .Property(t => t.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Jedzenie", UserId ="" },
                new Category { Id = 2, Name = "Rozrywka", UserId = "" },
                new Category { Id = 3, Name = "Rachunki", UserId = "" },
                new Category { Id = 4, Name = "Inne", UserId = "" }
                );
            builder.Entity<Store>().HasData(
                new Store { Id = 1, Name = "Biedronka", CategoryId = 1, UserId = "" },
                new Store { Id = 2, Name = "Auchan", CategoryId = 1, UserId = "" },
                new Store { Id = 3, Name = "Lidl", CategoryId = 1, UserId = "" }
                );
        }
    }
}
