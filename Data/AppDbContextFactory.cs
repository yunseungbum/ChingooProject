using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Chingoo.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = "server=localhost;port=3306;database=Chingoo_db;user=root;password=1234;SslMode=None;";

            optionsBuilder.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36)));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
