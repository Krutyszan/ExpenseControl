using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExpenseControl.Models;

namespace ExpenseControl.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Transaction> Transactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>()
                .Property(t => t.TotalAmount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
