using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<T>(this IHost host, int? retry = 0)
        {
            int retryforAvailbility = retry.Value;

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();
                var logger = services.GetRequiredService<ILogger<T>>();
                try
                {
                    logger.LogInformation("Migrating Database");

                    using var connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:Connectionstring"));
                    connection.Open();

                    using var command = new NpgsqlCommand {
                        Connection = connection
                    };

                    command.CommandText = "Drop table if exists coupon";
                    command.ExecuteNonQuery();

                    command.CommandText = @"Create table Coupon(ID serial primary key not null,
				    ProductName varchar(24) not null,
				    Description TEXT,
				    Amount int);";
                    command.ExecuteNonQuery();

                    command.CommandText = @"Insert into coupon (Productname, description, amount) values ('IPhone X','IPhone Discount',150); Insert into coupon (Productname, description, amount) values ('Samsung 10','Samsung Discount',100);";
                    command.ExecuteNonQuery();

                    logger.LogInformation("Migrated Database");

                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the postgresql database");
                    if (retryforAvailbility < 50)
                    {
                        retryforAvailbility++;
                        System.Threading.Thread.Sleep(2000);
                        MigrateDatabase<T>(host, retryforAvailbility);
                    }
                }
            }

            return host;
        }
    }
}
