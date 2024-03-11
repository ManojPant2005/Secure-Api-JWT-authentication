using Microsoft.EntityFrameworkCore;
using SecureApiWithAuth.Data.Entity;

namespace SecureApiWithAuth.Data.Configuration
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<TokenInfo> TokenInfo { get; set; }
    }
}
