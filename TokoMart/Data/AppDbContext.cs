using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using TokoMart.Models;

namespace TokoMart.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Classification> Classifications { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<ProductWithCategory> ProductWithCategories { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Ubah nama kolom menjadi snake_case
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }
            }
        }

        public async Task<List<T>> RunNativeQueryAsync<T>(string sql, params object[] parameters) where T : class
        {
            return await Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
        }

        public async Task<int> ExecuteNativeCommandAsync(string sql, params object[] parameters)
        {
            return await Database.ExecuteSqlRawAsync(sql, parameters);
        }


        //public async Task<int> GetTotalDataAsync(string countQuery, List<NpgsqlParameter> parameters)
        public async Task<int> GetTotalDataAsync(string countQuery, List<NpgsqlParameter> parameters)
        {
            int totalData = 0;

            using (var command = Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = countQuery;

                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                await Database.OpenConnectionAsync();
                try
                {
                    var result = await command.ExecuteScalarAsync();
                    totalData = Convert.ToInt32(result);
                }
                finally
                {
                    await Database.CloseConnectionAsync();
                }
            }

            return totalData;
        }

        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder();
            sb.Append(char.ToLower(input[0]));

            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    sb.Append('_');
                    sb.Append(char.ToLower(input[i]));
                }
                else
                {
                    sb.Append(input[i]);
                }
            }

            return sb.ToString();
        }
    }
}
