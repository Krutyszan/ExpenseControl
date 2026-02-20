using ExpenseControl.Data;
using ExpenseControl.Models;
using ExpenseControl.Services.Base;
using ExpenseControl.Services.Interfaces;

namespace ExpenseControl.Services
{
    public class PersonalityService : BaseService<Personality>, IPersonalityService
    {
        public PersonalityService(ApplicationDbContext context) : base(context)
        {
        }
    }
}

