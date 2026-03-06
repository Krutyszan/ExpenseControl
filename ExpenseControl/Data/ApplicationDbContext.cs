using ExpenseControl.Models;
using ExpenseControl.Models.Interfaces;
using ExpenseControl.Services.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentUserService _currentUserService;

        public string CurrentUserId => _currentUserService.UserId;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }


        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Store> Stores { get; set; } = null!;
        public DbSet<TransactionItem> Items { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //Globalny filtr zapytań, tylko po userId
            builder.Entity<Transaction>().HasQueryFilter(x => x.UserId == CurrentUserId);
            builder.Entity<TransactionItem>().HasQueryFilter(x => x.UserId == CurrentUserId);
            builder.Entity<Tag>().HasQueryFilter(x => x.UserId == CurrentUserId);
            builder.Entity<Store>().HasQueryFilter(x => x.UserId == CurrentUserId);
            builder.Entity<Category>().HasQueryFilter(x => x.UserId == CurrentUserId);

            builder.Entity<TransactionItem>()
                .Property(t => t.UnitPrice)
                .HasColumnType("decimal(18,2)");

        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    // 1. Nadawanie ID użytkownika (jeśli brakuje)
                    if (entry.Entity is IOwnedEntity ownedEntity)
                    {
                        if (string.IsNullOrEmpty(ownedEntity.UserId) && !string.IsNullOrEmpty(CurrentUserId))
                        {
                            ownedEntity.UserId = CurrentUserId;
                        }

                        // SZPIEG 1: Sprawdzamy, jakie UserId ostatecznie dostał obiekt
                        Console.WriteLine($"[DEBUG ZAPISU] Tabela: {entry.Entity.GetType().Name} | UserId: '{ownedEntity.UserId}'");
                    }

                    // 2. SZPIEG 2: Jeśli to paragon, sprawdzamy ID sklepu i kategorii
                    if (entry.Entity is Transaction t)
                    {
                        Console.WriteLine($"[DEBUG ZAPISU] Paragon szczegóły | StoreId: {t.StoreId}");
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
