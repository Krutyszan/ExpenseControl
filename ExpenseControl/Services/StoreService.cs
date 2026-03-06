using ExpenseControl.Data;
using ExpenseControl.Models;
using ExpenseControl.Services.Base;
using ExpenseControl.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Services
{
    public class StoreService : BaseService<Store>, IStoreService
    {
        public StoreService(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Store>> GetAllAsync()
        {
            return await _context.Stores // lub _dbSet, zależnie co masz w klasie bazowej
                                .Include(s => s.DefaultCategory) // Teraz Include zadziała
                                .AsNoTracking() // Opcjonalne: przyspiesza odczyt
                                .ToListAsync();
        }

    }

}