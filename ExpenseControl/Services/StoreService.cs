using ExpenseControl.Data;
using ExpenseControl.Models;
using ExpenseControl.Services.Base;
using ExpenseControl.Services.Interfaces;

namespace ExpenseControl.Services
{
    public class StoreService : BaseService<Store>, IStoreService
    {
        public StoreService(ApplicationDbContext context) : base(context)
        {
        }
    }
}