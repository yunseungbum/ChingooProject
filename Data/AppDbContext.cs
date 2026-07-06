using Chingoo.Models;
using Microsoft.EntityFrameworkCore;

namespace Chingoo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts => Set<Post>();

        public DbSet<Notice> Notices => Set<Notice>();
    }
}