using CheckAuthentication.Models;
using Microsoft.EntityFrameworkCore;

namespace CheckAuthentication.Data
{
    public class ApplicationDbContext : DbContext 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<AuthUserModel> Users { get; set; }
    }
}
