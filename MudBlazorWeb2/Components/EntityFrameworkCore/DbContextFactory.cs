//DbContextFactory.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MudBlazorWeb2.Components.EntityFrameworkCore
{
    public interface IDbContextFactory
    {
        BaseDbContext CreateDbContext(string dbType, string connectionString, string scheme = null);
    }
    public class DbContextFactory : IDbContextFactory
    {
        public BaseDbContext CreateDbContext(string dbType, string connectionString, string scheme = null)
        {
            if (dbType == "Oracle")
            {
                var optionsBuilder = new DbContextOptionsBuilder<OracleDbContext>();
                optionsBuilder.UseOracle(connectionString);
                var oracleContext = new OracleDbContext(optionsBuilder.Options);
                if (!string.IsNullOrEmpty(scheme))
                {
                    oracleContext.Database.OpenConnectionAsync().Wait();
                    oracleContext.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {scheme}").Wait();
                }

                return oracleContext;
            }
            else if (dbType == "Postgres")
            {
                var optionsBuilder = new DbContextOptionsBuilder<PostgresDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                return new PostgresDbContext(optionsBuilder.Options);
            }
            else
            {
                throw new NotSupportedException("Unsupported database type");
            }
        }
    }

}
