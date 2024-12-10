//DbContextFactory.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MudBlazorWeb2.Components.EntityFrameworkCore
{
    public interface IDbContextFactory
    {
        Task<BaseDbContext> CreateDbContext(string dbType, string connectionString, string scheme = null);
    }
    public class DbContextFactory : IDbContextFactory
    {
        public async Task<BaseDbContext> CreateDbContext(string dbType, string connectionString, string scheme = null)
        {
            BaseDbContext context = null;

            if (dbType == "Oracle")
            {
                var optionsBuilder = new DbContextOptionsBuilder<OracleDbContext>();
                optionsBuilder.UseOracle(connectionString);
                context = new OracleDbContext(optionsBuilder.Options);

                // Set the current schema for Oracle
                using (var tempContext = new OracleDbContext(optionsBuilder.Options))
                {
                    await tempContext.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {scheme}");
                }
            }
            else if (dbType == "Postgres")
            {
                var optionsBuilder = new DbContextOptionsBuilder<PostgresDbContext>();
                optionsBuilder.UseNpgsql(connectionString);
                context = new PostgresDbContext(optionsBuilder.Options);
            }
            else
            {
                throw new NotSupportedException("Unsupported database type");
            }
            return context;
        }
    }

}
