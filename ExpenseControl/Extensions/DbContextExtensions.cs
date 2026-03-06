using ExpenseControl.Data;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<int> GetCategoryIdByNameAsync(this ApplicationDbContext context, string categoryName)
        {
            var cat = await context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Name == categoryName);
            return cat != null ? cat.Id : 0;
        }
    }
}
