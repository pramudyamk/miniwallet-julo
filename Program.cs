using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace JwtApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateDB();
            CreateHostBuilder(args).Build().Run();
             
        }
        public static void CreateDB()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();

            //Use DB in project directory.  If it does not exist, create it:
            connectionStringBuilder.DataSource = "./SqliteDB.db";

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                //Create a table (drop if already exists first):

                var delTableCmd = connection.CreateCommand();
                delTableCmd.CommandText = "DROP TABLE IF EXISTS Wallets";
                delTableCmd.ExecuteNonQuery();
                delTableCmd.CommandText = "DROP TABLE IF EXISTS Transactions";
                delTableCmd.ExecuteNonQuery();

                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = "CREATE TABLE Wallets(id VARCHAR(100), owned_by VARCHAR(100), status VARCHAR(20), enabled_at DATETIME null, disabled_at DATETIME null, balance BIGINT)";
                createTableCmd.ExecuteNonQuery();
                createTableCmd.CommandText = "CREATE TABLE Transactions(id VARCHAR(100), deposited_by VARCHAR(100) null, withdrawn_by VARCHAR(100) null, status VARCHAR(20), deposited_at DATETIME null, withdrawn_at DATETIME null, amount BIGINT, reference_id VARCHAR(100))";
                createTableCmd.ExecuteNonQuery();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
