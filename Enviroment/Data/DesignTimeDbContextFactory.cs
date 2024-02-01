using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Enviroment.Data
{
    // Creates DbContext for design-time actions.
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HelpdeskContext>
    {
        // Called by EF Core for migrations.
        public HelpdeskContext CreateDbContext(string[] args)
        {
            // Builds configuration from a JSON file.
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Prepares DbContext options with a connection string.
            var builder = new DbContextOptionsBuilder<HelpdeskContext>();
            var connectionString = configuration.GetConnectionString("HelpdeskContext");

            // Sets SQL Server as the database.
            builder.UseSqlServer(connectionString);

            // Returns a new HelpdeskContext.
            return new HelpdeskContext(builder.Options);
        }
    }
}
