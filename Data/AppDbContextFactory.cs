using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using pokearcanumbe.Data;

namespace pokearcanumbe.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlServer("Server=localhost;Database=pokearcanumDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}