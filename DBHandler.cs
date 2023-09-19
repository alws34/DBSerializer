using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;
using DBSerializer;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace DBSerializer
{
    public class DBHandler : DbContext
    {
        public DbSet<MyTable> MyStructs { get; set; }

        public DBHandler(string connectionString) : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Define the table name using Fluent API
            modelBuilder.Entity<MyTable>().ToTable("MyStructs");

            base.OnModelCreating(modelBuilder);
        }
    }

    [Table("MyStructs")]
    public class MyTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class SqlTableGenerator<T> where T : struct
    {
        private readonly string connectionString;

        public SqlTableGenerator(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void CreateSqlTable()
        {
            using (var context = new DBHandler(connectionString))
            {
                context.Database.CreateIfNotExists();
                var tableName = context.GetTableName<T>();
                Console.WriteLine($"Table {tableName} created successfully.");
            }
        }

        public void DropTable()
        {
            using (var context = new DBHandler(connectionString))
            {
                var tableName = context.GetTableName<T>();

                if (!string.IsNullOrEmpty(tableName))
                {
                    var sql = $"DROP TABLE {tableName}";

                    try
                    {
                        context.Database.ExecuteSqlCommand(sql);
                        Console.WriteLine($"Table {tableName} dropped successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error dropping table {tableName}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Table for type {typeof(T).Name} not found.");
                }
            }
        }

        public T RetrieveData<T>(string query) where T : class, new()
        {
            using (var context = new DBHandler(connectionString))
            {
                try
                {
                    var result = context.Database.SqlQuery<string>(query).FirstOrDefault();

                    if (!string.IsNullOrEmpty(result))
                    {
                        T data = JsonConvert.DeserializeObject<T>(result);
                        Console.WriteLine($"Data retrieved and deserialized successfully.");
                        return data;
                    }
                    else
                    {
                        Console.WriteLine("No data found for the provided query.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving and deserializing data: {ex.Message}");
                }
            }

            return null;
        }
    }
}
