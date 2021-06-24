using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
        {
            var retryForAvailability = retry.Value;
            
            // Use this to access DI services
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;
            
            // Example how to get a service from DI
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                logger.LogInformation("Migrating postgresql database");

                using var connection =
                    new NpgsqlConnection(
                        configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
                
                // Needed to perform sql commands to the DB
                connection.Open();

                using var command = new NpgsqlCommand
                {
                    Connection = connection
                };
                
                // We don't have to worry about the database because when the connection
                // is opened, the database is create if it does not exist.

                command.CommandText = "DROP TABLE IF EXISTS Coupon";
                command.ExecuteNonQuery();

                command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY,
                                        ProductName VARCHAR(24) NOT NULL,
                                        Description TEXT,
                                        Amount INT)";
                command.ExecuteNonQuery();

                command.CommandText = @"INSERT INTO Coupon(ProductName, Description, Amount) 
                                        VALUES('IPhone X', 'IPhone Discount', 150);";
                command.ExecuteNonQuery();
                
                command.CommandText = @"INSERT INTO Coupon(ProductName, Description, Amount) 
                                        VALUES('Samsung 10', 'Samsung Discount', 100);";
                command.ExecuteNonQuery();

                logger.LogInformation("Migrated postgresql database");

            }
            catch (NpgsqlException e)
            {
                logger.LogError(e, "An error occurred whole migrating the postgresql database");

                if (retryForAvailability < 50)
                {
                    retryForAvailability++;
                    Thread.Sleep(2000);
                    MigrateDatabase<TContext>(host, retryForAvailability);
                }
            }
            
            return host;
        }
    }
}