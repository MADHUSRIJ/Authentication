using JwtAuth.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace JwtAuth.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
        public DbSet<AuthUserModel> Users { get; set; }
    }
}
