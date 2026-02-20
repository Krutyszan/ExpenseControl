using ExpenseControl.Models;
using Microsoft.AspNetCore.Identity;

namespace ExpenseControl.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
        public ICollection<Store> Stores { get; set; } = new List<Store>();
    }

}
