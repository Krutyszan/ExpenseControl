using ExpenseControl.Data;
using ExpenseControl.Models;
using ExpenseControl.Services.Base;
using ExpenseControl.Services.Interfaces;

namespace ExpenseControl.Services
{
    public class TagService : BaseService<Tag>, ITagService
    {
        public TagService(ApplicationDbContext context) : base(context)
        {
        }
    }
}
