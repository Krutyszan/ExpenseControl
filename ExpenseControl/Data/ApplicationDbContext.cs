using ExpenseControl.Models;
using ExpenseControl.Models.Interfaces;
using ExpenseControl.Services.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

            builder.Entity<Transaction>()
                .Property(t => t.TotalAmount)
                .HasColumnType("decimal(18,2)");

         /*   builder.Entity<Category>().HasData(
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
         */
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Przeszukujemy wszystko, co czeka na zapis i implementuje IOwnedEntity
            foreach (var entry in ChangeTracker.Entries<IOwnedEntity>())
            {
                // Jeśli jest to nowy obiekt (dodawany)
                if (entry.State == EntityState.Added)
                {
                    // Wymuszamy przypisanie ID aktualnego usera
                    entry.Entity.UserId = _currentUserService.UserId;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
