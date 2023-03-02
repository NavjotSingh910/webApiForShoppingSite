 using WebApplication3.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Items> Items { get; set; }
    }
}
